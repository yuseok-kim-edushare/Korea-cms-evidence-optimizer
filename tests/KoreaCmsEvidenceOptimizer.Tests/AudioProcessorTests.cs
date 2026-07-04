namespace KoreaCmsEvidenceOptimizer.Tests;

public class AudioProcessorTests : IDisposable
{
    private readonly string _workDir;

    public AudioProcessorTests()
    {
        _workDir = TestAssets.CreateTempDirectory("cms-evidence-audio-");
    }

    public void Dispose()
    {
        try { Directory.Delete(_workDir, recursive: true); } catch { /* best-effort cleanup */ }
    }

    [Fact]
    public void Optimize_SineWaveWav_ProducesMp3UnderMaxBytes()
    {
        var input = Path.Combine(_workDir, "input.wav");
        var output = Path.Combine(_workDir, "output.mp3");
        TestAssets.CreateSineWaveFile(input, seconds: 8, sampleRate: 44100, channels: 2);

        var result = Optimizer.OptimizeAudio(input, output, maxBytes: 307_200);

        Assert.True(result.Success, result.Message);
        Assert.True(File.Exists(output));
        var actualSize = new FileInfo(output).Length;
        Assert.True(actualSize <= 307_200, $"Output size {actualSize} exceeds limit");
        Assert.Equal(actualSize, result.FinalSizeBytes);
    }

    [Fact]
    public void Optimize_MissingFile_ReturnsFileNotFound()
    {
        var input = Path.Combine(_workDir, "missing.wav");
        var output = Path.Combine(_workDir, "output.mp3");

        var result = Optimizer.OptimizeAudio(input, output);

        Assert.False(result.Success);
        Assert.Equal(EvidenceErrorCode.FileNotFound, result.ErrorCode);
    }

    [Fact]
    public void Optimize_UnsupportedExtension_ReturnsUnsupportedFormat()
    {
        var input = Path.Combine(_workDir, "input.ogg");
        File.WriteAllBytes(input, new byte[] { 1, 2, 3 });
        var output = Path.Combine(_workDir, "output.mp3");

        var result = Optimizer.OptimizeAudio(input, output);

        Assert.False(result.Success);
        Assert.Equal(EvidenceErrorCode.UnsupportedFormat, result.ErrorCode);
    }

    [Fact]
    public void Optimize_ImpossiblySmallMaxBytes_ReturnsTargetSizeUnreachable()
    {
        var input = Path.Combine(_workDir, "input.wav");
        var output = Path.Combine(_workDir, "output.mp3");
        TestAssets.CreateSineWaveFile(input, seconds: 5);

        var result = Optimizer.OptimizeAudio(input, output, maxBytes: 10);

        Assert.False(result.Success);
        Assert.Equal(EvidenceErrorCode.TargetSizeUnreachable, result.ErrorCode);
    }
}
