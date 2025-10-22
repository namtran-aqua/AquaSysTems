namespace AquaSolution.Client.Common.ConvertNumber
{
    public static class ConvertNumberCommon
    {
        public static decimal ConvertNumber(decimal number)
        {
            return Math.Round(number, 2, MidpointRounding.AwayFromZero);
        }
    }
}
