namespace Hermes.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo BrazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

        public static DateTime ConvertToBrazilDateTime(this DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, BrazilTimeZone);        
        }

        public static DateTime GetCurrentBrazilDateTime()
        {
            return ConvertToBrazilDateTime(DateTime.UtcNow);
        }
    }
}