namespace KoreaCmsEvidenceOptimizer;

/// <summary>
/// KoreaCmsEvidenceOptimizer 처리 결과 에러 코드.
/// Native export(NativeExports, net10.0-windows Native AOT 빌드)에서도 동일한 정수 값이 사용됩니다.
/// </summary>
public enum EvidenceErrorCode
{
    /// <summary>처리 성공.</summary>
    Success = 0,

    /// <summary>입력 파일을 찾을 수 없음.</summary>
    FileNotFound = -1,

    /// <summary>지원하지 않는 입력/출력 포맷.</summary>
    UnsupportedFormat = -2,

    /// <summary>목표 크기(maxBytes)를 만족하는 결과를 생성할 수 없음.</summary>
    TargetSizeUnreachable = -3,

    /// <summary>처리 중 예외 발생.</summary>
    Exception = -4,
}
