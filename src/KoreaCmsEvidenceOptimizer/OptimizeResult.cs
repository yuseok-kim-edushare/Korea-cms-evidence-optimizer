namespace KoreaCmsEvidenceOptimizer;

/// <summary>
/// 이미지/녹취 최적화 처리 결과.
/// </summary>
public sealed class OptimizeResult
{
    /// <summary>결과 에러 코드. <see cref="EvidenceErrorCode.Success"/>이면 성공.</summary>
    public EvidenceErrorCode ErrorCode { get; }

    /// <summary>처리가 성공했는지 여부.</summary>
    public bool Success => ErrorCode == EvidenceErrorCode.Success;

    /// <summary>생성된 출력 파일 경로 (성공 시).</summary>
    public string? OutputPath { get; }

    /// <summary>최종 산출물 크기(바이트).</summary>
    public long FinalSizeBytes { get; }

    /// <summary>부가 설명 메시지 (실패 시 에러 상세 등).</summary>
    public string? Message { get; }

    private OptimizeResult(EvidenceErrorCode errorCode, string? outputPath, long finalSizeBytes, string? message)
    {
        ErrorCode = errorCode;
        OutputPath = outputPath;
        FinalSizeBytes = finalSizeBytes;
        Message = message;
    }

    internal static OptimizeResult Ok(string outputPath, long finalSizeBytes) =>
        new(EvidenceErrorCode.Success, outputPath, finalSizeBytes, message: null);

    internal static OptimizeResult Fail(EvidenceErrorCode errorCode, string message)
    {
        if (errorCode == EvidenceErrorCode.Success)
            throw new ArgumentException("실패 결과는 Success 코드를 사용할 수 없습니다.", nameof(errorCode));

        return new OptimizeResult(errorCode, outputPath: null, finalSizeBytes: 0, message);
    }
}
