using KoreaCmsEvidenceOptimizer;

if (args.Length < 3)
{
    Console.WriteLine("KoreaCmsEvidenceOptimizer Console Sample");
    Console.WriteLine();
    Console.WriteLine("사용법:");
    Console.WriteLine("  KoreaCmsEvidenceOptimizerConsoleSample image <input> <output.jpg> [maxBytes] [pdfPageIndex]");
    Console.WriteLine("  KoreaCmsEvidenceOptimizerConsoleSample audio <input> <output.mp3> [maxBytes]");
    Console.WriteLine();
    Console.WriteLine("예시:");
    Console.WriteLine("  KoreaCmsEvidenceOptimizerConsoleSample image C:\\in\\scan.pdf C:\\out\\scan.jpg");
    Console.WriteLine("  KoreaCmsEvidenceOptimizerConsoleSample audio C:\\in\\call.wav C:\\out\\call.mp3 200000");
    return 1;
}

var mode = args[0].ToLowerInvariant();
var inputPath = args[1];
var outputPath = args[2];

OptimizeResult result;

switch (mode)
{
    case "image":
        {
            var maxBytes = args.Length > 3 ? long.Parse(args[3]) : Optimizer.DefaultMaxBytes;
            var pdfPageIndex = args.Length > 4 ? int.Parse(args[4]) : 0;
            result = Optimizer.OptimizeImage(inputPath, outputPath, maxBytes, pdfPageIndex);
            break;
        }
    case "audio":
        {
            var maxBytes = args.Length > 3 ? long.Parse(args[3]) : Optimizer.DefaultMaxBytes;
            result = Optimizer.OptimizeAudio(inputPath, outputPath, maxBytes);
            break;
        }
    default:
        Console.WriteLine($"알 수 없는 모드: {mode} (image 또는 audio 사용)");
        return 1;
}

if (result.Success)
{
    Console.WriteLine($"성공: {result.OutputPath} ({result.FinalSizeBytes:N0} bytes)");
    return 0;
}

Console.WriteLine($"실패: [{result.ErrorCode}] {result.Message}");
return (int)result.ErrorCode;
