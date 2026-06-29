namespace EduNexus.Api.Common.Entities;

// FT-10 / FT-11 — Danh mục, gói học phí, đăng ký & thanh toán
public class CoursePrice                              // H1: mua lẻ vĩnh viễn
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
}

public class Package                                  // H3: gói nhóm khóa học theo thời hạn
{
    public Guid Id { get; set; }
    public Guid CourseGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DurationMonths { get; set; }            // 1 | 3 | 6 | 12
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class PackageSubscription
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime ExpiresAt { get; set; }            // gia hạn cộng dồn (AC-10b)
    public string Status { get; set; } = "Active";     // Active | Expired
}

public class AccessGrant                              // quyền truy cập nội dung (H1/H2/H3)
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid? CourseId { get; set; }
    public string GrantType { get; set; } = string.Empty; // H1 | H2 | H3
    public Guid? SourceRefId { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }           // null = vĩnh viễn (H1)
    public bool IsRevoked { get; set; }                // thu hồi nhưng giữ lịch sử (BR-18)
}

public class Payment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public decimal Amount { get; set; }
    public string Provider { get; set; } = string.Empty;     // VNPay | SePay
    public string PurchaseType { get; set; } = string.Empty; // H1 | H2 | H3
    public Guid? RefId { get; set; }
    public string? ProviderTxnId { get; set; }
    public string Status { get; set; } = "Pending";    // Pending | Success | Failed | Refunded
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
