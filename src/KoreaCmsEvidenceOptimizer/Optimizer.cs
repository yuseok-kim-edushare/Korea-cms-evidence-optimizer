namespace KoreaCmsEvidenceOptimizer;

/// <summary>
/// KoreaCmsEvidenceOptimizer 공개 API. 이미지/PDF/녹취 파일을 지정된 최대 크기 이하로 반복 압축한다.
/// </summary>
public static class Optimizer
{
    /// <summary>CMS 동의자료 증빙 기본 최대 크기(300KB = 307,200 bytes).</summary>
    public const long DefaultMaxBytes = 307_200;

    /// <summary>
    /// 이미지(JPG/PNG/BMP/TIF) 또는 PDF를 읽어 JPG로 <paramref name="maxBytes"/> 이하까지 반복 압축한다.
    /// </summary>
    /// <param name="inputPath">입력 파일 경로 (jpg/jpeg/png/bmp/tif/tiff/pdf).</param>
    /// <param name="outputPath">생성될 JPG 출력 파일 경로.</param>
    /// <param name="maxBytes">목표 최대 크기(바이트). 기본 300KB.</param>
    /// <param name="pdfPageIndex">입력이 PDF인 경우 렌더링할 0-기반 페이지 인덱스.</param>
    public static OptimizeResult OptimizeImage(
        string inputPath,
        string outputPath,
        long maxBytes = DefaultMaxBytes,
        int pdfPageIndex = 0)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
            return OptimizeResult.Fail(EvidenceErrorCode.FileNotFound, "입력 경로가 비어 있습니다.");

        var extension = Path.GetExtension(inputPath);

        if (PdfProcessor.IsSupportedExtension(extension))
            return PdfProcessor.Optimize(inputPath, outputPath, maxBytes, pdfPageIndex);

        if (ImageProcessor.IsSupportedExtension(extension))
            return ImageProcessor.Optimize(inputPath, outputPath, maxBytes);

        return OptimizeResult.Fail(EvidenceErrorCode.UnsupportedFormat, $"지원하지 않는 이미지 포맷입니다: {extension}");
    }

    /// <summary>
    /// 녹취(WAV/MP3/M4A/WMA)를 읽어 MP3로 <paramref name="maxBytes"/> 이하까지 반복 압축한다.
    /// </summary>
    /// <param name="inputPath">입력 파일 경로 (wav/mp3/m4a/wma).</param>
    /// <param name="outputPath">생성될 MP3 출력 파일 경로.</param>
    /// <param name="maxBytes">목표 최대 크기(바이트). 기본 300KB.</param>
    public static OptimizeResult OptimizeAudio(
        string inputPath,
        string outputPath,
        long maxBytes = DefaultMaxBytes)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
            return OptimizeResult.Fail(EvidenceErrorCode.FileNotFound, "입력 경로가 비어 있습니다.");

        var extension = Path.GetExtension(inputPath);

        if (!AudioProcessor.IsSupportedExtension(extension))
            return OptimizeResult.Fail(EvidenceErrorCode.UnsupportedFormat, $"지원하지 않는 오디오 포맷입니다: {extension}");

        return AudioProcessor.Optimize(inputPath, outputPath, maxBytes);
    }
}
