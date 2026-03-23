namespace MovieRentalAPI.Helpers
{
    public static class IstDateTime
    {
        private static readonly TimeZoneInfo Ist =
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

        public static DateTime Now =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);
    }
}
