using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace KoreaCmsEvidenceOptimizer;

/// <summary>
/// GDI+ 기반 이미지(JPG/PNG/BMP/TIF) → JPG 반복 압축 파이프라인.
/// 품질을 90→20 단계로 낮추고, 그래도 목표 크기를 넘으면 해상도를 90%씩 축소한다.
/// </summary>
internal static class ImageProcessor
{
    private static readonly string[] SupportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff" };

    private const int MaxQuality = 90;
    private const int MinQuality = 20;
    private const int QualityStep = 10;
    private const double ResizeFactor = 0.9;
    private const int MinSidePixels = 200;

    public static bool IsSupportedExtension(string extension) =>
        Array.IndexOf(SupportedExtensions, extension.ToLowerInvariant()) >= 0;

    public static OptimizeResult Optimize(string inputPath, string outputPath, long maxBytes)
    {
        if (!File.Exists(inputPath))
            return OptimizeResult.Fail(EvidenceErrorCode.FileNotFound, $"입력 파일을 찾을 수 없습니다: {inputPath}");

        var extension = Path.GetExtension(inputPath);
        if (!IsSupportedExtension(extension))
            return OptimizeResult.Fail(EvidenceErrorCode.UnsupportedFormat, $"지원하지 않는 이미지 포맷입니다: {extension}");

        try
        {
            using var original = new Bitmap(inputPath);
            return OptimizeBitmap(original, outputPath, maxBytes);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return OptimizeResult.Fail(EvidenceErrorCode.Exception, ex.Message);
        }
    }

    /// <summary>
    /// 이미 로드된 비트맵을 대상으로 반복 압축을 수행한다. (PdfProcessor에서 렌더된 페이지에도 재사용)
    /// </summary>
    internal static OptimizeResult OptimizeBitmap(Bitmap original, string outputPath, long maxBytes)
    {
        var jpegCodec = GetJpegCodecInfo();
        Bitmap current = original;
        Bitmap? ownedResized = null;

        try
        {
            while (true)
            {
                for (var quality = MaxQuality; quality >= MinQuality; quality -= QualityStep)
                {
                    using var ms = EncodeJpeg(current, jpegCodec, quality);
                    if (ms.Length <= maxBytes)
                    {
                        File.WriteAllBytes(outputPath, ms.ToArray());
                        return OptimizeResult.Ok(outputPath, ms.Length);
                    }
                }

                var newWidth = (int)(current.Width * ResizeFactor);
                var newHeight = (int)(current.Height * ResizeFactor);

                if (Math.Min(newWidth, newHeight) < MinSidePixels)
                {
                    return OptimizeResult.Fail(
                        EvidenceErrorCode.TargetSizeUnreachable,
                        $"목표 크기({maxBytes} bytes)를 만족하는 이미지를 생성할 수 없습니다 (최소 해상도 도달).");
                }

                var resized = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.DrawImage(current, 0, 0, newWidth, newHeight);
                }

                ownedResized?.Dispose();
                ownedResized = resized;
                current = resized;
            }
        }
        finally
        {
            ownedResized?.Dispose();
        }
    }

    private static MemoryStream EncodeJpeg(Image image, ImageCodecInfo jpegCodec, int quality)
    {
        var ms = new MemoryStream();
        using var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
        image.Save(ms, jpegCodec, encoderParams);
        return ms;
    }

    private static ImageCodecInfo GetJpegCodecInfo() =>
        ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
}
