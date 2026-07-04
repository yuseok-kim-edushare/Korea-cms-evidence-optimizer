using System.Drawing;

namespace KoreaCmsEvidenceOptimizer.Tests;

public class ImageProcessorTests : IDisposable
{
    private readonly string _workDir;

    public ImageProcessorTests()
    {
        _workDir = TestAssets.CreateTempDirectory("cms-evidence-img-");
    }

    public void Dispose()
    {
        try { Directory.Delete(_workDir, recursive: true); } catch { /* best-effort cleanup */ }
    }

    [Theory]
    [InlineData(".png")]
    [InlineData(".bmp")]
    public void Optimize_NoisyLargeImage_ProducesJpegUnderMaxBytes(string sourceExtension)
    {
        var input = Path.Combine(_workDir, $"input{sourceExtension}");
        var output = Path.Combine(_workDir, "output.jpg");
        TestAssets.CreateNoisyBitmapFile(input, 3000, 2000);

        var result = Optimizer.OptimizeImage(input, output, maxBytes: 307_200);

        Assert.True(result.Success, result.Message);
        Assert.Equal(EvidenceErrorCode.Success, result.ErrorCode);
        Assert.True(File.Exists(output));
        var actualSize = new FileInfo(output).Length;
        Assert.True(actualSize <= 307_200, $"Output size {actualSize} exceeds limit");
        Assert.Equal(actualSize, result.FinalSizeBytes);
    }

    [Fact]
    public void Optimize_SmallSolidImage_SucceedsAtHighQuality()
    {
        var input = Path.Combine(_workDir, "small.png");
        var output = Path.Combine(_workDir, "small.jpg");
        TestAssets.CreateSolidBitmapFile(input, 100, 100, Color.CornflowerBlue);

        var result = Optimizer.OptimizeImage(input, output);

        Assert.True(result.Success, result.Message);
        Assert.True(new FileInfo(output).Length <= Optimizer.DefaultMaxBytes);
    }

    [Fact]
    public void Optimize_MissingFile_ReturnsFileNotFound()
    {
        var input = Path.Combine(_workDir, "does-not-exist.jpg");
        var output = Path.Combine(_workDir, "output.jpg");

        var result = Optimizer.OptimizeImage(input, output);

        Assert.False(result.Success);
        Assert.Equal(EvidenceErrorCode.FileNotFound, result.ErrorCode);
    }

    [Fact]
    public void Optimize_UnsupportedExtension_ReturnsUnsupportedFormat()
    {
        var input = Path.Combine(_workDir, "input.gif");
        File.WriteAllBytes(input, new byte[] { 1, 2, 3 });
        var output = Path.Combine(_workDir, "output.jpg");

        var result = Optimizer.OptimizeImage(input, output);

        Assert.False(result.Success);
        Assert.Equal(EvidenceErrorCode.UnsupportedFormat, result.ErrorCode);
    }

    [Fact]
    public void Optimize_ImpossiblySmallMaxBytes_ReturnsTargetSizeUnreachable()
    {
        var input = Path.Combine(_workDir, "input.png");
        var output = Path.Combine(_workDir, "output.jpg");
        TestAssets.CreateNoisyBitmapFile(input, 1000, 1000);

        var result = Optimizer.OptimizeImage(input, output, maxBytes: 10);

        Assert.False(result.Success);
        Assert.Equal(EvidenceErrorCode.TargetSizeUnreachable, result.ErrorCode);
    }
}
