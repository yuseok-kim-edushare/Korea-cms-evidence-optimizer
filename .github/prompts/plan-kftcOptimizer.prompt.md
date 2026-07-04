# Plan: KFTC CMS 동의자료 최적화 라이브러리 (KftcOptimizer)

빈 워크스페이스에 그린필드로 구축합니다. Gemini 초안의 아키텍처(net48 + .NET 10 Native AOT C-Export)를 유지하되, 패키지는 버전 하드코딩 없이 `dotnet add package` 최신 버전(확인 완료: NAudio 2.3.0, NAudio.Lame 2.1.0, System.Drawing.Common 10.0.9, PDFtoImage 5.2.1)을 사용합니다.

## 확정된 요구사항

- 이미지: 입력 JPG/PNG/BMP/TIF/PDF → 출력 JPG
- 녹취: 입력 WAV/MP3/M4A/WMA → 출력 MP3
- 300KB 초과 시: 자동 반복 압축(품질/해상도/비트레이트 단계적 하향), 실패 시 에러 코드
- 산출물: 라이브러리 + 단위테스트 + CI(GitHub Actions) + NuGet 패키징 + 샘플(콘솔/C++)

## Steps

1. **Phase 1 — 스캐폴딩**: 솔루션 + `src/KftcOptimizer` multi-target(`net48;net10.0-windows`) csproj 생성, `dotnet add package`로 4개 패키지 최신 버전 추가, net10 조건부 `PublishAot`/`NativeLib=Shared` 설정
2. **Phase 2 — 이미지 파이프라인** (*Phase 1 이후, Phase 3과 병렬 가능*): `ImageProcessor`(GDI+: JPG/PNG/BMP/TIF 디코드 → JPEG 품질 90→20 단계 하향 → 초과 시 해상도 90%씩 축소, 최소 변 하한 유지) + `PdfProcessor`(PDFtoImage로 페이지 렌더 후 동일 파이프라인 재사용) + xUnit 테스트
3. **Phase 3 — 오디오 파이프라인** (*Phase 2와 병렬*): `AudioProcessor` — WAV/MP3는 순수 managed reader, M4A/WMA는 `MediaFoundationReader`(net48) → LAME MP3 인코드, 재생시간 기반 비트레이트 산출 후 64→16kbps 및 모노/샘플레이트 단계 하향
4. **Phase 4 — Native AOT C-Export** (*Phase 2·3 이후*): `NativeExports.cs`(`[UnmanagedCallersOnly]`, UTF-16 포인터, int 에러코드 체계: 0 성공 / -1 파일없음 / -2 미지원포맷 / -3 목표크기 불가 / -4 예외), `dotnet publish -r win-x64` 후 export 심볼 및 네이티브 동반 DLL(pdfium, libSkiaSharp, libmp3lame) 배포 검증
5. **Phase 5 — 샘플**: 콘솔 샘플(net48/net10) + C++ `LoadLibrary`/`GetProcAddress` 호출 예제
6. **Phase 6 — CI/패키징**: GitHub Actions — net48 빌드+테스트, AOT publish(win-x64, win-x86), `dotnet pack`으로 nupkg 생성, artifact 업로드

## Relevant files (신규 생성)

- `src/KftcOptimizer/KftcOptimizer.csproj` — multi-target, AOT 조건부 설정, `SuppressTrimAnalysisWarnings`
- `src/KftcOptimizer/Optimizer.cs`, `OptimizeResult.cs` — 공개 관리형 API(`maxBytes` 기본 307,200 파라미터화)
- `src/KftcOptimizer/ImageProcessor.cs`, `PdfProcessor.cs`, `AudioProcessor.cs`, `NativeExports.cs`
- `tests/KftcOptimizer.Tests/` — 테스트 에셋은 코드로 생성(대형 비트맵, 사인파 WAV)
- `samples/ConsoleSample/`, `samples/CppSample/`, `.github/workflows/build.yml`

## Verification

1. `dotnet test` — net48/net10 양 타겟에서 300KB 이하 출력 및 에러코드 검증
2. `dotnet publish -c Release -r win-x64 -f net10.0-windows` 후 `dumpbin /exports`로 `OptimizeKftcImage`/`OptimizeKftcAudio` 노출 확인
3. C++ 샘플 빌드·실행으로 실제 네이티브 호출 스모크 테스트
4. CI 워크플로 전 잡 그린 확인

## Decisions

- Gemini 초안의 msbuild x86/x64 매트릭스 대신 net48은 **AnyCPU** 단일 빌드 (managed DLL이므로 x86/x64 프로세스 모두 로드 가능, libmp3lame은 NAudio.Lame이 런타임 플랫폼 감지로 선택) — 산출물 단순화
- 에러코드를 Gemini 초안(1/0/-1)보다 세분화하여 진단성 확보
- PDF는 페이지 지정 파라미터로 렌더 (기본 첫 페이지)

## Further Considerations

1. **M4A/WMA의 Native AOT 지원**: `MediaFoundationReader`는 built-in COM 의존이라 AOT 미지원. MF 수동 vtable interop 구현 

