using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace KoreaCmsEvidenceOptimizer;

/// <summary>
/// NAudio(+NAudio.Lame) 기반 녹취(WAV/MP3/M4A/WMA) → MP3 반복 압축 파이프라인.
/// 비트레이트를 64→16kbps 단계로 낮추고, 필요 시 모노 전환 및 샘플레이트 하향을 추가로 시도한다.
/// </summary>
internal static class AudioProcessor
{
    private static readonly string[] SupportedExtensions = { ".wav", ".mp3", ".m4a", ".wma" };

    private const int MaxBitrateKbps = 64;
    private const int MinBitrateKbps = 16;
    private const int BitrateStep = 8;

    private static readonly int[] SampleRateSteps = { 32000, 22050, 16000, 11025, 8000 };

    public static bool IsSupportedExtension(string extension) =>
        Array.IndexOf(SupportedExtensions, extension.ToLowerInvariant()) >= 0;

    public static OptimizeResult Optimize(string inputPath, string outputPath, long maxBytes)
    {
        if (!File.Exists(inputPath))
            return OptimizeResult.Fail(EvidenceErrorCode.FileNotFound, $"입력 파일을 찾을 수 없습니다: {inputPath}");

        var extension = Path.GetExtension(inputPath);
        if (!IsSupportedExtension(extension))
            return OptimizeResult.Fail(EvidenceErrorCode.UnsupportedFormat, $"지원하지 않는 오디오 포맷입니다: {extension}");

        try
        {
            using var reader = OpenReader(inputPath, extension);

            var isStereo = reader.WaveFormat.Channels > 1;
            var monoOptions = isStereo ? new[] { false, true } : new[] { false };

            foreach (var sampleRate in BuildSampleRateCandidates(reader.WaveFormat.SampleRate))
            {
                foreach (var mono in monoOptions)
                {
                    for (var bitrate = MaxBitrateKbps; bitrate >= MinBitrateKbps; bitrate -= BitrateStep)
                    {
                        var mp3Bytes = EncodeMp3(reader, bitrate, mono, sampleRate);
                        if (mp3Bytes.Length <= maxBytes)
                        {
                            File.WriteAllBytes(outputPath, mp3Bytes);
                            return OptimizeResult.Ok(outputPath, mp3Bytes.Length);
                        }
                    }
                }
            }

            return OptimizeResult.Fail(
                EvidenceErrorCode.TargetSizeUnreachable,
                $"목표 크기({maxBytes} bytes)를 만족하는 MP3를 생성할 수 없습니다.");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return OptimizeResult.Fail(EvidenceErrorCode.Exception, ex.Message);
        }
    }

    private static WaveStream OpenReader(string path, string extension) => extension.ToLowerInvariant() switch
    {
        ".wav" => new WaveFileReader(path),
        ".mp3" => new Mp3FileReader(path),
        ".m4a" or ".wma" => new MediaFoundationReader(path),
        _ => throw new NotSupportedException($"지원하지 않는 오디오 포맷입니다: {extension}"),
    };

    /// <summary>원본 샘플레이트부터 시작해 표준 샘플레이트를 단계적으로 낮춰가며 후보를 반환한다. null = 원본 유지.</summary>
    private static IEnumerable<int?> BuildSampleRateCandidates(int originalSampleRate)
    {
        yield return null;

        foreach (var step in SampleRateSteps)
        {
            if (step < originalSampleRate)
                yield return step;
        }
    }

    private static byte[] EncodeMp3(WaveStream reader, int bitrateKbps, bool mono, int? outputSampleRate)
    {
        reader.Position = 0;

        ISampleProvider sampleProvider = reader.ToSampleProvider();
        if (mono && sampleProvider.WaveFormat.Channels == 2)
            sampleProvider = new StereoToMonoSampleProvider(sampleProvider) { LeftVolume = 0.5f, RightVolume = 0.5f };

        var waveProvider = new SampleToWaveProvider16(sampleProvider);

        var config = new LameConfig { BitRate = bitrateKbps };
        if (outputSampleRate.HasValue)
            config.OutputSampleRate = outputSampleRate.Value;

        using var ms = new MemoryStream();
        using (var writer = new LameMP3FileWriter(ms, waveProvider.WaveFormat, config))
        {
            var buffer = new byte[8192];
            int read;
            while ((read = waveProvider.Read(buffer, 0, buffer.Length)) > 0)
                writer.Write(buffer, 0, read);
        }

        return ms.ToArray();
    }
}
