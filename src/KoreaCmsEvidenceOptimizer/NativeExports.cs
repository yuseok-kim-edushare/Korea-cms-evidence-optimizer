#if NET
using System.Runtime.InteropServices;

namespace KoreaCmsEvidenceOptimizer;

/// <summary>
/// Native AOT(C-Export) 진입점. net10.0-windows(NativeLib=Shared) 게시 시 네이티브 DLL에서
/// UTF-16(char*) 포인터 기반으로 호출 가능한 함수들을 노출한다.
/// 반환값은 <see cref="EvidenceErrorCode"/>의 정수 값과 동일하다 (0 = 성공).
/// </summary>
public static unsafe class NativeExports
{
    /// <summary>
    /// 이미지(JPG/PNG/BMP/TIF) 또는 PDF를 읽어 JPG로 압축한다.
    /// </summary>
    /// <param name="inputPath">UTF-16(널 종료) 입력 파일 경로.</param>
    /// <param name="outputPath">UTF-16(널 종료) 출력 파일 경로.</param>
    /// <param name="maxBytes">목표 최대 크기(바이트). 0 이하이면 기본값(300KB)을 사용한다.</param>
    /// <param name="pdfPageIndex">PDF인 경우 렌더링할 0-기반 페이지 인덱스.</param>
    /// <returns><see cref="EvidenceErrorCode"/> 정수 값 (0 = 성공).</returns>
    [UnmanagedCallersOnly(EntryPoint = "OptimizeEvidenceImage", CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static int OptimizeEvidenceImage(char* inputPath, char* outputPath, long maxBytes, int pdfPageIndex)
    {
        try
        {
            if (inputPath == null || outputPath == null)
                return (int)EvidenceErrorCode.FileNotFound;

            var input = new string(inputPath);
            var output = new string(outputPath);
            var effectiveMaxBytes = maxBytes > 0 ? maxBytes : Optimizer.DefaultMaxBytes;

            var result = Optimizer.OptimizeImage(input, output, effectiveMaxBytes, pdfPageIndex);
            return (int)result.ErrorCode;
        }
        catch
        {
            return (int)EvidenceErrorCode.Exception;
        }
    }

    /// <summary>
    /// 녹취(WAV/MP3/M4A/WMA)를 읽어 MP3로 압축한다.
    /// </summary>
    /// <param name="inputPath">UTF-16(널 종료) 입력 파일 경로.</param>
    /// <param name="outputPath">UTF-16(널 종료) 출력 파일 경로.</param>
    /// <param name="maxBytes">목표 최대 크기(바이트). 0 이하이면 기본값(300KB)을 사용한다.</param>
    /// <returns><see cref="EvidenceErrorCode"/> 정수 값 (0 = 성공).</returns>
    [UnmanagedCallersOnly(EntryPoint = "OptimizeEvidenceAudio", CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static int OptimizeEvidenceAudio(char* inputPath, char* outputPath, long maxBytes)
    {
        try
        {
            if (inputPath == null || outputPath == null)
                return (int)EvidenceErrorCode.FileNotFound;

            var input = new string(inputPath);
            var output = new string(outputPath);
            var effectiveMaxBytes = maxBytes > 0 ? maxBytes : Optimizer.DefaultMaxBytes;

            var result = Optimizer.OptimizeAudio(input, output, effectiveMaxBytes);
            return (int)result.ErrorCode;
        }
        catch
        {
            return (int)EvidenceErrorCode.Exception;
        }
    }
}
#endif
