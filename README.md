# KoreaCmsEvidenceOptimizer

한국 CMS 출금이체 동의자료 증빙(이미지/녹취) 압축 최적화를 위한 비공식 오픈소스 라이브러리입니다.

- 이미지: JPG/PNG/BMP/TIF/PDF 입력 → JPG 출력
- 녹취: WAV/MP3/M4A/WMA 입력 → MP3 출력
- 목표 크기(기본 300KB, 307,200 bytes) 초과 시 품질/해상도/비트레이트를 단계적으로 낮추며 재시도
    - 300KB 는 금융결제원 CMS 공지(https://www.cmsedi.or.kr/cms/board/workdata/cms/view/987) 기준
- `net48` / `net10.0-windows`(Native AOT, C-Export) 멀티 타겟
    - Windows 7 이상 호환 보장 위한 net 48 / win 10 + 에서 성능 보장위한 Native AOT 구성입니다.

## 프로젝트 구조

```
src/KoreaCmsEvidenceOptimizer/        라이브러리 본체
tests/KoreaCmsEvidenceOptimizer.Tests/  xUnit 단위 테스트 (에셋은 코드로 생성)
samples/ConsoleSample/     .NET 콘솔 샘플
samples/CppSample/         Native AOT DLL을 호출하는 C++ 샘플
.github/workflows/ci.yaml   CI (빌드/테스트, Dependabot auto-merge)
.github/workflows/cd.yaml   CD (main 머지 시 ZIP GitHub Release)
```

## 사용법 (관리형 API)

```csharp
using KoreaCmsEvidenceOptimizer;

var result = Optimizer.OptimizeImage(@"C:\in\scan.pdf", @"C:\out\scan.jpg");
if (result.Success)
{
    Console.WriteLine($"{result.OutputPath} ({result.FinalSizeBytes} bytes)");
}
else
{
    Console.WriteLine($"실패: {result.ErrorCode} - {result.Message}");
}

var audioResult = Optimizer.OptimizeAudio(@"C:\in\call.wav", @"C:\out\call.mp3");
```

## 에러 코드 (`EvidenceErrorCode`)

| 값 | 이름 | 의미 |
|---|---|---|
| 0 | Success | 성공 |
| -1 | FileNotFound | 입력 파일 없음 |
| -2 | UnsupportedFormat | 지원하지 않는 포맷 |
| -3 | TargetSizeUnreachable | 목표 크기 달성 불가 |
| -4 | Exception | 처리 중 예외 발생 |

## 빌드 / 테스트

```powershell
dotnet build KoreaCmsEvidenceOptimizer.slnx -c Release
dotnet test tests\KoreaCmsEvidenceOptimizer.Tests\KoreaCmsEvidenceOptimizer.Tests.csproj -f net48 -c Release
dotnet test tests\KoreaCmsEvidenceOptimizer.Tests\KoreaCmsEvidenceOptimizer.Tests.csproj -f net10.0-windows -c Release
```

## Native AOT 게시 (C-Export)

```powershell
dotnet publish src\KoreaCmsEvidenceOptimizer\KoreaCmsEvidenceOptimizer.csproj -f net10.0-windows -r win-x64 -c Release
```

`OptimizeEvidenceImage`, `OptimizeEvidenceAudio` 두 함수가 UTF-16(`char*`/`wchar_t*`) 포인터
기반으로 네이티브 DLL에서 내보내집니다 (`samples/CppSample` 참고).

`net10.0-windows` 타겟은 `BuiltInComInteropSupport`를 활성화하여, M4A/WMA 디코딩에 사용되는
`MediaFoundationReader`(COM 기반)도 Native AOT 환경에서 정상 동작합니다.

## NuGet 패키징

```powershell
dotnet pack src\KoreaCmsEvidenceOptimizer\KoreaCmsEvidenceOptimizer.csproj -c Release -o .\nupkg
```
