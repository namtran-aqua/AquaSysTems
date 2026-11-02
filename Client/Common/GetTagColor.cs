namespace AquaSolution.Client.Common
{
    public static class GetTagColor
    {
        public static string GetTagColorCss(string status)
        {
            return status switch
            {
                "Open" => "#d3d0d0",       
                "OnHold" => "#d48806",     
                "Resolved" => "#389e0d",   
                "InProgress" => "#096dd9", 
                "Cancel" => "#cf1322",     
                "New" => "#13c2c2",        
                "Approved" => "#389e0d",
                "Done" => "#2f54eb",       
                "Rejected" => "#F44336",
                "WaitingForApproval" => "#1677ff",
                "WaitingForConfirmation" => "#fa8c16",
                "InReview" => "#2196F3",
                "Pending" => "#9E9E9E",
                _ => "#bfbfbf",
            };
        }
    }
}
