using System.Drawing;
using PDFtoImage;
using SkiaSharp;

namespace KoreaCmsEvidenceOptimizer;

/// <summary>
/// PDFtoImage(Pdfium/Skia)로 PDF 페이지를 렌더링한 뒤 <see cref="ImageProcessor"/> 파이프라인을 재사용해 JPG로 압축한다.
/// </summary>
internal static class PdfProcessor
{
    public static bool IsSupportedExtension(string extension) =>
        string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase);

    /// <param name="inputPath">입력 PDF 파일 경로.</param>
    /// <param name="outputPath">생성될 JPG 출력 파일 경로.</param>
    /// <param name="maxBytes">목표 최대 크기(바이트).</param>
    /// <param name="pageIndex">렌더링할 0-기반 페이지 인덱스 (기본값 0 = 첫 페이지).</param>
    public static OptimizeResult Optimize(string inputPath, string outputPath, long maxBytes, int pageIndex = 0)
    {
        if (!File.Exists(inputPath))
            return OptimizeResult.Fail(EvidenceErrorCode.FileNotFound, $"입력 파일을 찾을 수 없습니다: {inputPath}");

        var extension = Path.GetExtension(inputPath);
        if (!IsSupportedExtension(extension))
            return OptimizeResult.Fail(EvidenceErrorCode.UnsupportedFormat, $"지원하지 않는 포맷입니다: {extension}");

        try
        {
            var pdfBytes = File.ReadAllBytes(inputPath);
            using var skBitmap = Conversion.ToImage(pdfBytes, pageIndex);
            using var bitmap = SKBitmapToBitmap(skBitmap);
            return ImageProcessor.OptimizeBitmap(bitmap, outputPath, maxBytes);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return OptimizeResult.Fail(EvidenceErrorCode.UnsupportedFormat, $"PDF 페이지 인덱스가 유효하지 않습니다: {ex.Message}");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return OptimizeResult.Fail(EvidenceErrorCode.Exception, ex.Message);
        }
    }

    private static Bitmap SKBitmapToBitmap(SKBitmap skBitmap)
    {
        using var skImage = SKImage.FromBitmap(skBitmap);
        using var encoded = skImage.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = encoded.AsStream();
        // System.Drawing.Bitmap이 소스 스트림 수명에 의존하지 않도록 복제본을 반환한다.
        using var temp = new Bitmap(stream);
        return new Bitmap(temp);
    }
}
