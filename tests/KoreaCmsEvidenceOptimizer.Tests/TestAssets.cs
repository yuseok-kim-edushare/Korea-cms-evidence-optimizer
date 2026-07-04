using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using NAudio.Wave;

namespace KoreaCmsEvidenceOptimizer.Tests;

/// <summary>테스트에서 사용할 이미지/PDF/오디오 자산을 코드로 생성하는 헬퍼.</summary>
internal static class TestAssets
{
    /// <summary>net48/net10 양쪽에서 동작하는 임시 디렉터리 생성 헬퍼.</summary>
    public static string CreateTempDirectory(string prefix)
    {
        var path = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// 압축이 잘 되지 않도록 픽셀 단위 노이즈로 채워진 대형 비트맵을 생성해 파일로 저장한다.
    /// (품질/해상도 반복 하향 로직을 실제로 거치도록 유도하기 위함)
    /// </summary>
    public static string CreateNoisyBitmapFile(string path, int width, int height, int seed = 12345)
    {
        using var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        var rect = new Rectangle(0, 0, width, height);
        var data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        try
        {
            var random = new Random(seed);
            var stride = data.Stride;
            var buffer = new byte[stride * height];
            random.NextBytes(buffer);
            System.Runtime.InteropServices.Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        bitmap.Save(path, ImageFormat.Png);
        return path;
    }

    /// <summary>단색의 작은 비트맵(고압축 가능)을 생성해 파일로 저장한다.</summary>
    public static string CreateSolidBitmapFile(string path, int width, int height, Color color)
    {
        using var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(color);
        }

        bitmap.Save(path, ImageFormat.Png);
        return path;
    }

    /// <summary>
    /// PDFium이 인식할 수 있는 최소 구성의 단일 페이지 PDF(빨간 사각형)를 생성해 파일로 저장한다.
    /// </summary>
    public static string CreateMinimalPdfFile(string path, int widthPt = 400, int heightPt = 400)
    {
        var content = $"1 0 0 RG 1 0 0 rg\n20 20 {widthPt - 40} {heightPt - 40} re\nf\n";
        var contentBytes = Encoding.ASCII.GetBytes(content);

        var sb = new StringBuilder();
        var offsets = new List<int>();

        void AppendObject(string body)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(sb.ToString()));
            sb.Append(body);
        }

        sb.Append("%PDF-1.4\n");

        AppendObject($"{offsets.Count + 1} 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        AppendObject($"{offsets.Count + 1} 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
        AppendObject(
            $"{offsets.Count + 1} 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {widthPt} {heightPt}] /Contents 4 0 R >>\nendobj\n");
        AppendObject(
            $"{offsets.Count + 1} 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n{content}endstream\nendobj\n");

        var xrefOffset = Encoding.ASCII.GetByteCount(sb.ToString());
        sb.Append($"xref\n0 {offsets.Count + 1}\n");
        sb.Append("0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            sb.Append(offset.ToString("D10", CultureInfo.InvariantCulture));
            sb.Append(" 00000 n \n");
        }

        sb.Append($"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");

        File.WriteAllText(path, sb.ToString(), Encoding.ASCII);
        return path;
    }

    /// <summary>지정한 길이/샘플레이트/채널 수의 사인파 WAV 파일을 생성한다.</summary>
    public static string CreateSineWaveFile(
        string path,
        double seconds = 5,
        int sampleRate = 44100,
        int channels = 2,
        double frequencyHz = 440)
    {
        var waveFormat = new WaveFormat(sampleRate, 16, channels);
        using var writer = new WaveFileWriter(path, waveFormat);

        var totalSamples = (int)(seconds * sampleRate);
        var buffer = new short[channels];

        for (var i = 0; i < totalSamples; i++)
        {
            var t = i / (double)sampleRate;
            var sampleValue = (short)(Math.Sin(2 * Math.PI * frequencyHz * t) * short.MaxValue * 0.8);
            for (var c = 0; c < channels; c++)
                buffer[c] = sampleValue;

            writer.WriteSamples(buffer, 0, channels);
        }

        return path;
    }
}
