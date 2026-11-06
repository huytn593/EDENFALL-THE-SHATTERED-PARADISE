namespace Edenfall.Core
{
    public enum WeatherType
    {
        CelestialClear,
        AstralAurora,
        StarlightMist,
        EclipseSurge
    }

    public static class WeatherTypeExtensions
    {
        public static string ToDisplayString(this WeatherType weather, bool vietnamese = true)
        {
            if (vietnamese)
            {
                return weather switch
                {
                    WeatherType.CelestialClear => "Trời Sao Tĩnh Lặng",
                    WeatherType.AstralAurora => "Cực Quang Tinh Thể",
                    WeatherType.StarlightMist => "Sương Mù Ánh Sao",
                    WeatherType.EclipseSurge => "Thủy Triều Nhật Thực",
                    _ => weather.ToString()
                };
            }

            return weather switch
            {
                WeatherType.CelestialClear => "Celestial Clear",
                WeatherType.AstralAurora => "Astral Aurora",
                WeatherType.StarlightMist => "Starlight Mist",
                WeatherType.EclipseSurge => "Eclipse Surge",
                _ => weather.ToString()
            };
        }
    }
}
