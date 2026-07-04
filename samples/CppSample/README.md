# KoreaCmsEvidenceOptimizer C++ 샘플

`KoreaCmsEvidenceOptimizer.dll`(net10.0-windows, Native AOT `NativeLib=Shared` 게시물)을
`LoadLibraryW`/`GetProcAddress`로 동적 로드해 `OptimizeEvidenceImage`/`OptimizeEvidenceAudio`
내보내기 함수를 호출하는 최소 예제입니다.

## 사전 준비: 네이티브 라이브러리 게시

```powershell
cd ..\..
dotnet publish src\KoreaCmsEvidenceOptimizer\KoreaCmsEvidenceOptimizer.csproj -f net10.0-windows -r win-x64 -c Release
```

게시 결과(`src\KoreaCmsEvidenceOptimizer\bin\Release\net10.0-windows\win-x64\publish\` 폴더의
`KoreaCmsEvidenceOptimizer.dll` 및 동반 네이티브 DLL 일체)를 이 샘플의 실행 파일과 같은 폴더로 복사해야 합니다.

## 빌드 (Visual Studio Developer Command Prompt + CMake)

```powershell
cmake -S . -B build -G "Visual Studio 17 2022" -A x64
cmake --build build --config Release
```

또는 MSVC가 설치된 환경에서 `cl.exe`로 직접 빌드할 수도 있습니다:

```powershell
cl /EHsc /std:c++17 main.cpp /Fe:EvidenceCppSample.exe
```

## 실행

```powershell
copy ..\..\src\KoreaCmsEvidenceOptimizer\bin\Release\net10.0-windows\win-x64\publish\*.dll build\Release\
build\Release\EvidenceCppSample.exe image C:\in\scan.jpg C:\out\scan.jpg
build\Release\EvidenceCppSample.exe audio C:\in\call.wav C:\out\call.mp3 200000
```

반환값은 `EvidenceErrorCode`와 동일한 정수 코드입니다 (0 = 성공, -1 = 파일 없음,
-2 = 지원하지 않는 포맷, -3 = 목표 크기 달성 불가, -4 = 예외).
