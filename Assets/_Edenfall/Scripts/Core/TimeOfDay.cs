namespace Edenfall.Core
{
    public enum TimeOfDay
    {
        Morning,
        Afternoon,
        Evening,
        Night,

        // Aliases
        Dawn = Morning,
        Noon = Afternoon,
        Dusk = Evening,
        Midnight = Night
    }

    public static class TimeOfDayExtensions
    {
        public static string ToDisplayString(this TimeOfDay time, bool vietnamese = true)
        {
            if (vietnamese)
            {
                return time switch
                {
                    TimeOfDay.Morning => "Bình Minh",
                    TimeOfDay.Afternoon => "Chính Ngọ",
                    TimeOfDay.Evening => "Hoàng Hôn",
                    TimeOfDay.Night => "Đêm Tinh Tú",
                    _ => time.ToString()
                };
            }

            return time switch
            {
                TimeOfDay.Morning => "Morning",
                TimeOfDay.Afternoon => "Afternoon",
                TimeOfDay.Evening => "Evening",
                TimeOfDay.Night => "Night",
                _ => time.ToString()
            };
        }

        public static TimeOfDay Next(this TimeOfDay time)
        {
            return time switch
            {
                TimeOfDay.Morning => TimeOfDay.Afternoon,
                TimeOfDay.Afternoon => TimeOfDay.Evening,
                TimeOfDay.Evening => TimeOfDay.Night,
                TimeOfDay.Night => TimeOfDay.Morning,
                _ => TimeOfDay.Morning
            };
        }
    }
}
