using System.Drawing;

namespace KoreaCmsEvidenceOptimizer.Tests;

public class PowerBuilderOptimizerTests : IDisposable
{
    private readonly string _workDir;
    private readonly PowerBuilderOptimizer _optimizer = new();

    public PowerBuilderOptimizerTests()
    {
        _workDir = TestAssets.CreateTempDirectory("cms-evidence-pb-");
    }

    public void Dispose()
    {
        try { Directory.Delete(_workDir, recursive: true); } catch { /* best-effort cleanup */ }
    }

    [Fact]
    public void OptimizeImage_Success_MatchesOptimizerResult()
    {
        var input = Path.Combine(_workDir, "input.png");
        var output = Path.Combine(_workDir, "output.jpg");
        TestAssets.CreateSolidBitmapFile(input, 100, 100, Color.CornflowerBlue);

        var direct = Optimizer.OptimizeImage(input, output);

        var success = false;
        var outputPathResult = string.Empty;
        long finalSizeBytes = -1;
        var message = string.Empty;

        var errorCode = _optimizer.OptimizeImage(
            input,
            output,
            maxBytes: Optimizer.DefaultMaxBytes,
            pdfPageIndex: 0,
            ref success,
            ref outputPathResult,
            ref finalSizeBytes,
            ref message);

        Assert.Equal((int)direct.ErrorCode, errorCode);
        Assert.Equal(direct.Success, success);
        Assert.Equal(direct.OutputPath, outputPathResult);
        Assert.Equal(direct.FinalSizeBytes, finalSizeBytes);
        Assert.Equal(direct.Message ?? string.Empty, message);
        Assert.Equal(_optimizer.ErrorCodeSuccess, errorCode);
    }

    [Fact]
    public void OptimizeAudio_Success_MatchesOptimizerResult()
    {
        var input = Path.Combine(_workDir, "input.wav");
        var output = Path.Combine(_workDir, "output.mp3");
        TestAssets.CreateSineWaveFile(input, seconds: 3);

        var direct = Optimizer.OptimizeAudio(input, output);

        var success = false;
        var outputPathResult = string.Empty;
        long finalSizeBytes = -1;
        var message = string.Empty;

        var errorCode = _optimizer.OptimizeAudio(
            input,
            output,
            maxBytes: Optimizer.DefaultMaxBytes,
            ref success,
            ref outputPathResult,
            ref finalSizeBytes,
            ref message);

        Assert.Equal((int)direct.ErrorCode, errorCode);
        Assert.Equal(direct.Success, success);
        Assert.Equal(direct.OutputPath, outputPathResult);
        Assert.Equal(direct.FinalSizeBytes, finalSizeBytes);
        Assert.Equal(direct.Message ?? string.Empty, message);
        Assert.Equal(_optimizer.ErrorCodeSuccess, errorCode);
    }

    [Fact]
    public void OptimizeImage_EmptyInputPath_ReturnsFileNotFound()
    {
        var success = true;
        var outputPathResult = "unchanged";
        long finalSizeBytes = 123;
        var message = "unchanged";

        var errorCode = _optimizer.OptimizeImage(
            string.Empty,
            Path.Combine(_workDir, "output.jpg"),
            ref success,
            ref outputPathResult,
            ref finalSizeBytes,
            ref message);

        Assert.Equal(_optimizer.ErrorCodeFileNotFound, errorCode);
        Assert.False(success);
        Assert.Equal(string.Empty, outputPathResult);
        Assert.Equal(0, finalSizeBytes);
        Assert.False(string.IsNullOrEmpty(message));
    }

    [Fact]
    public void OptimizeImage_UnsupportedExtension_ReturnsUnsupportedFormat()
    {
        var input = Path.Combine(_workDir, "input.gif");
        File.WriteAllBytes(input, new byte[] { 1, 2, 3 });

        var success = true;
        var outputPathResult = string.Empty;
        long finalSizeBytes = -1;
        var message = string.Empty;

        var errorCode = _optimizer.OptimizeImage(
            input,
            Path.Combine(_workDir, "output.jpg"),
            ref success,
            ref outputPathResult,
            ref finalSizeBytes,
            ref message);

        Assert.Equal(_optimizer.ErrorCodeUnsupportedFormat, errorCode);
        Assert.False(success);
    }

    [Fact]
    public void OptimizeImage_NonPositiveMaxBytes_UsesDefaultMaxBytes()
    {
        var input = Path.Combine(_workDir, "input.png");
        var output = Path.Combine(_workDir, "output-default.jpg");
        TestAssets.CreateSolidBitmapFile(input, 80, 80, Color.Red);

        var success = false;
        var outputPathResult = string.Empty;
        long finalSizeBytes = 0;
        var message = string.Empty;

        var errorCode = _optimizer.OptimizeImage(
            input,
            output,
            maxBytes: 0,
            pdfPageIndex: 0,
            ref success,
            ref outputPathResult,
            ref finalSizeBytes,
            ref message);

        Assert.Equal(_optimizer.ErrorCodeSuccess, errorCode);
        Assert.True(success);
        Assert.True(finalSizeBytes <= Optimizer.DefaultMaxBytes);
    }

    [Fact]
    public void ErrorCodeProperties_MatchEvidenceErrorCodeValues()
    {
        Assert.Equal((int)EvidenceErrorCode.Success, _optimizer.ErrorCodeSuccess);
        Assert.Equal((int)EvidenceErrorCode.FileNotFound, _optimizer.ErrorCodeFileNotFound);
        Assert.Equal((int)EvidenceErrorCode.UnsupportedFormat, _optimizer.ErrorCodeUnsupportedFormat);
        Assert.Equal((int)EvidenceErrorCode.TargetSizeUnreachable, _optimizer.ErrorCodeTargetSizeUnreachable);
        Assert.Equal((int)EvidenceErrorCode.Exception, _optimizer.ErrorCodeException);
        Assert.Equal(Optimizer.DefaultMaxBytes, _optimizer.DefaultMaxBytes);
    }
}
