// KoreaCmsEvidenceOptimizer C++ 샘플
// Native AOT로 게시된 KoreaCmsEvidenceOptimizer.dll(net10.0-windows, NativeLib=Shared)을
// LoadLibraryW/GetProcAddress로 동적 로드하여 호출하는 예제.
//
// 빌드 전 준비:
//   dotnet publish ..\..\src\KoreaCmsEvidenceOptimizer\KoreaCmsEvidenceOptimizer.csproj -f net10.0-windows -r win-x64 -c Release
//   위 게시 결과(KoreaCmsEvidenceOptimizer.dll 및 동반 네이티브 DLL)를 이 실행 파일과 같은 폴더에 복사한다.
//
// 사용법:
//   EvidenceCppSample.exe image <input> <output.jpg> [maxBytes] [pdfPageIndex]
//   EvidenceCppSample.exe audio <input> <output.mp3> [maxBytes]

#include <windows.h>
#include <cstdio>
#include <cwchar>
#include <string>

// NativeExports.cs의 시그니처와 일치해야 한다.
// int OptimizeEvidenceImage(char* inputPath, char* outputPath, long maxBytes, int pdfPageIndex)
// int OptimizeEvidenceAudio(char* inputPath, char* outputPath, long maxBytes)
// C# char*(UTF-16)는 Windows의 wchar_t*와 동일한 표현을 사용한다.
typedef int(__cdecl* OptimizeEvidenceImageFn)(const wchar_t* inputPath, const wchar_t* outputPath, long long maxBytes, int pdfPageIndex);
typedef int(__cdecl* OptimizeEvidenceAudioFn)(const wchar_t* inputPath, const wchar_t* outputPath, long long maxBytes);

static void PrintUsage()
{
    wprintf(L"KoreaCmsEvidenceOptimizer C++ Sample\n\n");
    wprintf(L"사용법:\n");
    wprintf(L"  EvidenceCppSample.exe image <input> <output.jpg> [maxBytes] [pdfPageIndex]\n");
    wprintf(L"  EvidenceCppSample.exe audio <input> <output.mp3> [maxBytes]\n");
}

int wmain(int argc, wchar_t* argv[])
{
    if (argc < 4)
    {
        PrintUsage();
        return 1;
    }

    std::wstring mode = argv[1];
    const wchar_t* inputPath = argv[2];
    const wchar_t* outputPath = argv[3];
    long long maxBytes = (argc > 4) ? _wtoi64(argv[4]) : 307200; // Optimizer.DefaultMaxBytes

    HMODULE hModule = LoadLibraryW(L"KoreaCmsEvidenceOptimizer.dll");
    if (hModule == nullptr)
    {
        wprintf(L"KoreaCmsEvidenceOptimizer.dll 로드 실패 (오류 코드: %lu)\n", GetLastError());
        wprintf(L"게시된 Native AOT 결과물을 이 실행 파일과 같은 폴더에 복사했는지 확인하세요.\n");
        return 1;
    }

    int errorCode = 0;

    if (mode == L"image")
    {
        auto optimizeImage = reinterpret_cast<OptimizeEvidenceImageFn>(GetProcAddress(hModule, "OptimizeEvidenceImage"));
        if (optimizeImage == nullptr)
        {
            wprintf(L"OptimizeEvidenceImage 심볼을 찾을 수 없습니다.\n");
            FreeLibrary(hModule);
            return 1;
        }

        int pdfPageIndex = (argc > 5) ? _wtoi(argv[5]) : 0;
        errorCode = optimizeImage(inputPath, outputPath, maxBytes, pdfPageIndex);
    }
    else if (mode == L"audio")
    {
        auto optimizeAudio = reinterpret_cast<OptimizeEvidenceAudioFn>(GetProcAddress(hModule, "OptimizeEvidenceAudio"));
        if (optimizeAudio == nullptr)
        {
            wprintf(L"OptimizeEvidenceAudio 심볼을 찾을 수 없습니다.\n");
            FreeLibrary(hModule);
            return 1;
        }

        errorCode = optimizeAudio(inputPath, outputPath, maxBytes);
    }
    else
    {
        wprintf(L"알 수 없는 모드: %s (image 또는 audio 사용)\n", mode.c_str());
        FreeLibrary(hModule);
        return 1;
    }

    if (errorCode == 0)
    {
        wprintf(L"성공: %s\n", outputPath);
    }
    else
    {
        wprintf(L"실패: 에러 코드 %d\n", errorCode);
    }

    FreeLibrary(hModule);
    return errorCode;
}
