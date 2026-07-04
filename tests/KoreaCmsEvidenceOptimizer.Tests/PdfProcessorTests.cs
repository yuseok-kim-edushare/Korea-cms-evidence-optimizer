namespace KoreaCmsEvidenceOptimizer.Tests;

public class PdfProcessorTests : IDisposable
{
    private readonly string _workDir;

    public PdfProcessorTests()
    {
        _workDir = TestAssets.CreateTempDirectory("cms-evidence-pdf-");
    }

    public void Dispose()
    {
        try { Directory.Delete(_workDir, recursive: true); } catch { /* best-effort cleanup */ }
    }

    [Fact]
    public void Optimize_ValidSinglePagePdf_RendersAndCompressesUnderMaxBytes()
    {
        var input = Path.Combine(_workDir, "input.pdf");
        var output = Path.Combine(_workDir, "output.jpg");
        TestAssets.CreateMinimalPdfFile(input, widthPt: 800, heightPt: 600);

        var result = Optimizer.OptimizeImage(input, output, maxBytes: 307_200);

        Assert.True(result.Success, result.Message);
        Assert.True(File.Exists(output));
        Assert.True(new FileInfo(output).Length <= 307_200);
    }

    [Fact]
    public void Optimize_MissingPdfFile_ReturnsFileNotFound()
    {
        var input = Path.Combine(_workDir, "missing.pdf");
        var output = Path.Combine(_workDir, "output.jpg");

        var result = Optimizer.OptimizeImage(input, output);

        Assert.False(result.Success);
        Assert.Equal(EvidenceErrorCode.FileNotFound, result.ErrorCode);
    }
}
