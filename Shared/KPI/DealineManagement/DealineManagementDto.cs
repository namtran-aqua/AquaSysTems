using System.Globalization;


namespace AquaSolution.Shared.KPI.DealineManagement
{
    public class DealineManagementDto
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? Month { get; set; }
        public int Year { get; set; }
        public string MonthString
        {
            get
            {
                if (Month.HasValue)
                {
                    var s = new DateTime(Year, Month.Value, 1)
                            .ToString("MMMM - yyyy", CultureInfo.InvariantCulture);
                    return s.ToUpperInvariant();
                }
                return string.Empty;
            }
        }

    }
}
