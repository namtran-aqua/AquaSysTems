namespace AquaSolution.Client.Common
{
    public static class GetTagColor
    {
        public static string GetTagColorCss(string status)
        {
            return status switch
            {
                "Open" => "#595959",         // Xám đậm
                "OnHold" => "#d48806",       // Cam vàng
                "Resolved" => "#389e0d",     // Xanh lá đậm
                "InProgress" => "#096dd9",   // Xanh dương đậm
                "Cancel" => "#cf1322",       // Đỏ đậm
                "New" => "#13c2c2",          // Xanh ngọc (khác AntD)
                "Approval" => "#389e0d",     // Tím đậm
                "Done" => "#2f54eb",         // Xanh đậm khác
                "Rejected" => "#ff4d4f",
                _ => "#bfbfbf",
            };
        }
    }
}
