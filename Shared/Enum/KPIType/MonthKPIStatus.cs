namespace AquaSolution.Shared.Enum.KPIType
{
    public enum MonthKPIStatus
    {
        NotApplicable = 1, // Miễn KPI (ngoài phạm vi đánh giá / thai sản / mới vào)
        Approved = 2,      // Đã duyệt (hợp lệ)
        Pending = 3,       // Đang chờ duyệt
        Rejected = 4,      // Bị từ chối
        Missing = 5,       // Thuộc phạm vi đánh giá nhưng chưa nộp (quá hạn)
        Open = 6           // Chưa đến kỳ đánh giá (tháng tương lai hoặc tháng hiện tại chưa hết kỳ)
    }
}
