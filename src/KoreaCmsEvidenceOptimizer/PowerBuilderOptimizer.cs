namespace KoreaCmsEvidenceOptimizer;

/// <summary>
/// Appeon PowerBuilder .NET DLL Importer용 어댑터.
/// static class, enum, <see cref="OptimizeResult"/> 등 Importer가 지원하지 않는 타입을
/// ref/out 기반의 평탄화(flat) API로 노출한다.
/// </summary>
public class PowerBuilderOptimizer
{
    /// <summary>CMS 동의자료 증빙 기본 최대 크기(300KB = 307,200 bytes).</summary>
    public long DefaultMaxBytes => Optimizer.DefaultMaxBytes;

    /// <summary>처리 성공.</summary>
    public int ErrorCodeSuccess => (int)EvidenceErrorCode.Success;

    /// <summary>입력 파일을 찾을 수 없음.</summary>
    public int ErrorCodeFileNotFound => (int)EvidenceErrorCode.FileNotFound;

    /// <summary>지원하지 않는 입력/출력 포맷.</summary>
    public int ErrorCodeUnsupportedFormat => (int)EvidenceErrorCode.UnsupportedFormat;

    /// <summary>목표 크기(maxBytes)를 만족하는 결과를 생성할 수 없음.</summary>
    public int ErrorCodeTargetSizeUnreachable => (int)EvidenceErrorCode.TargetSizeUnreachable;

    /// <summary>처리 중 예외 발생.</summary>
    public int ErrorCodeException => (int)EvidenceErrorCode.Exception;

    /// <summary>
    /// 이미지(JPG/PNG/BMP/TIF) 또는 PDF를 읽어 JPG로 <see cref="Optimizer.DefaultMaxBytes"/> 이하까지 압축한다.
    /// </summary>
    /// <param name="inputPath">입력 파일 경로.</param>
    /// <param name="outputPath">생성될 JPG 출력 파일 경로.</param>
    /// <param name="success">처리 성공 여부.</param>
    /// <param name="outputPathResult">생성된 출력 파일 경로 (성공 시).</param>
    /// <param name="finalSizeBytes">최종 산출물 크기(바이트).</param>
    /// <param name="message">부가 설명 메시지 (실패 시 에러 상세 등).</param>
    /// <returns><see cref="ErrorCodeSuccess"/> 등 에러 코드 정수 값.</returns>
    public int OptimizeImage(
        string inputPath,
        string outputPath,
        ref bool success,
        ref string outputPathResult,
        ref long finalSizeBytes,
        ref string message)
    {
        return OptimizeImage(
            inputPath,
            outputPath,
            Optimizer.DefaultMaxBytes,
            pdfPageIndex: 0,
            ref success,
            ref outputPathResult,
            ref finalSizeBytes,
            ref message);
    }

    /// <summary>
    /// 이미지(JPG/PNG/BMP/TIF) 또는 PDF를 읽어 JPG로 <paramref name="maxBytes"/> 이하까지 압축한다.
    /// </summary>
    /// <param name="inputPath">입력 파일 경로.</param>
    /// <param name="outputPath">생성될 JPG 출력 파일 경로.</param>
    /// <param name="maxBytes">목표 최대 크기(바이트). 0 이하이면 <see cref="DefaultMaxBytes"/>를 사용한다.</param>
    /// <param name="pdfPageIndex">입력이 PDF인 경우 렌더링할 0-기반 페이지 인덱스.</param>
    /// <param name="success">처리 성공 여부.</param>
    /// <param name="outputPathResult">생성된 출력 파일 경로 (성공 시).</param>
    /// <param name="finalSizeBytes">최종 산출물 크기(바이트).</param>
    /// <param name="message">부가 설명 메시지 (실패 시 에러 상세 등).</param>
    /// <returns><see cref="ErrorCodeSuccess"/> 등 에러 코드 정수 값.</returns>
    public int OptimizeImage(
        string inputPath,
        string outputPath,
        long maxBytes,
        int pdfPageIndex,
        ref bool success,
        ref string outputPathResult,
        ref long finalSizeBytes,
        ref string message)
    {
        var effectiveMaxBytes = maxBytes > 0 ? maxBytes : Optimizer.DefaultMaxBytes;
        var result = Optimizer.OptimizeImage(inputPath, outputPath, effectiveMaxBytes, pdfPageIndex);
        return ApplyResult(result, ref success, ref outputPathResult, ref finalSizeBytes, ref message);
    }

    /// <summary>
    /// 녹취(WAV/MP3/M4A/WMA)를 읽어 MP3로 <see cref="Optimizer.DefaultMaxBytes"/> 이하까지 압축한다.
    /// </summary>
    /// <param name="inputPath">입력 파일 경로.</param>
    /// <param name="outputPath">생성될 MP3 출력 파일 경로.</param>
    /// <param name="success">처리 성공 여부.</param>
    /// <param name="outputPathResult">생성된 출력 파일 경로 (성공 시).</param>
    /// <param name="finalSizeBytes">최종 산출물 크기(바이트).</param>
    /// <param name="message">부가 설명 메시지 (실패 시 에러 상세 등).</param>
    /// <returns><see cref="ErrorCodeSuccess"/> 등 에러 코드 정수 값.</returns>
    public int OptimizeAudio(
        string inputPath,
        string outputPath,
        ref bool success,
        ref string outputPathResult,
        ref long finalSizeBytes,
        ref string message)
    {
        return OptimizeAudio(
            inputPath,
            outputPath,
            Optimizer.DefaultMaxBytes,
            ref success,
            ref outputPathResult,
            ref finalSizeBytes,
            ref message);
    }

    /// <summary>
    /// 녹취(WAV/MP3/M4A/WMA)를 읽어 MP3로 <paramref name="maxBytes"/> 이하까지 압축한다.
    /// </summary>
    /// <param name="inputPath">입력 파일 경로.</param>
    /// <param name="outputPath">생성될 MP3 출력 파일 경로.</param>
    /// <param name="maxBytes">목표 최대 크기(바이트). 0 이하이면 <see cref="DefaultMaxBytes"/>를 사용한다.</param>
    /// <param name="success">처리 성공 여부.</param>
    /// <param name="outputPathResult">생성된 출력 파일 경로 (성공 시).</param>
    /// <param name="finalSizeBytes">최종 산출물 크기(바이트).</param>
    /// <param name="message">부가 설명 메시지 (실패 시 에러 상세 등).</param>
    /// <returns><see cref="ErrorCodeSuccess"/> 등 에러 코드 정수 값.</returns>
    public int OptimizeAudio(
        string inputPath,
        string outputPath,
        long maxBytes,
        ref bool success,
        ref string outputPathResult,
        ref long finalSizeBytes,
        ref string message)
    {
        var effectiveMaxBytes = maxBytes > 0 ? maxBytes : Optimizer.DefaultMaxBytes;
        var result = Optimizer.OptimizeAudio(inputPath, outputPath, effectiveMaxBytes);
        return ApplyResult(result, ref success, ref outputPathResult, ref finalSizeBytes, ref message);
    }

    private static int ApplyResult(
        OptimizeResult result,
        ref bool success,
        ref string outputPathResult,
        ref long finalSizeBytes,
        ref string message)
    {
        success = result.Success;
        outputPathResult = result.OutputPath ?? string.Empty;
        finalSizeBytes = result.FinalSizeBytes;
        message = result.Message ?? string.Empty;
        return (int)result.ErrorCode;
    }
}
