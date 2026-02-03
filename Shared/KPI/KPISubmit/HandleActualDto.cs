using AquaSolution.Shared.Enum.KPIType;


namespace AquaSolution.Shared.KPI.KPISubmit
{
    public class HandleActualDto
    {
        public Guid TaskId { get; set; }
        public int? Month { get; set; }
        public int? Quarter { get; set; }
        public int? HalfYear { get; set; }
        public int Year { get; set; }
        public decimal? ActualValue { get; set; }
        public int? Index { get; set; }
        public Guid CreatedBy { get; set; }
        public string? UserName { get; set; }
        public string? calculated { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? HeaderTitle { get; set; }
        public string? TaskName { get; set; }
        public string? Description { get; set; }
        public string? CalculateMethod { get; set; }
        public KPIIndexType KPIIndexType { get; set; }
        public KPICategoryType KPICategory { get; set; }
        public decimal? Max { get; set; }
        public decimal? Bottom { get; set; }
        public QuarterCalculateType QuarterCalculateType { get; set; }
        public string MaxString
        {
            get
            {
                if (Max.HasValue)
                {
                    return $"{Max.Value * 100:0.##}%";
                }
                return string.Empty;
            }
            set
            {
                if (Max.HasValue)
                {
                    value = $"{Max.Value * 100:0.##}%";
                }
            }
        }
        public string? BottomString
        {
            get
            {
                if (Bottom.HasValue)
                {
                    return $"{Bottom.Value * 100:0.##}%";
                }
                return string.Empty;
            }
            set
            {
                if (Bottom.HasValue)
                {
                    value = $"{Bottom.Value * 100:0.##}%";
                }
            }
        }
        public decimal? Weight { get; set; }
        public string? WeightString
        {
            get
            {
                if (Weight.HasValue)
                {
                    return $"{Weight.Value * 100:0.##}%";
                }
                return string.Empty;
            }
            set
            {
                if (Weight.HasValue)
                {
                    value = $"{Weight.Value * 100:0.##}%";
                }
            }
        }
        public string? Unit { get; set; }
        public string? PIC { get; set; }
        public string? DataSource { get; set; }
        public string? Formula { get; set; }
        public decimal? TargetValue { get; set; }
        public decimal? Achiement { get; set; }
        public string AchiementString
        {
            get
            {
                if (Achiement.HasValue)
                {
                    return $"{Achiement.Value * 100:0.##} %";
                }
                return string.Empty;
            }
            set
            {
                if (Achiement.HasValue)
                {
                    value = $"{Achiement.Value * 100:0.##} %";
                }
            }
        }

        public decimal? Score { get; set; }
        public decimal? KPIScore { get; set; }
        public decimal? KeyTaskScore { get; set; }
        public decimal? OMGScore { get; set; }
        public KPIFormulaType KPIFormulaType { get; set; }
        public decimal IndexWeight { get; set; }
        public Guid TargetId { get; set; }

    }
}
