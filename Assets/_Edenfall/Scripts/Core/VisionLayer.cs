namespace Edenfall.Core
{
    public enum VisionLayer
    {
        Normal,
        MagicalAura,
        EnergyPathways,
        CelestialSilhouette,
        MiasmaSight
    }

    public static class VisionLayerExtensions
    {
        public static string ToDisplayString(this VisionLayer layer, bool vietnamese = true)
        {
            if (vietnamese)
            {
                return layer switch
                {
                    VisionLayer.Normal => "Thị Giác Thực Tại",
                    VisionLayer.MagicalAura => "Hào Quang Tinh Tú",
                    VisionLayer.EnergyPathways => "Kinh Mạch Địa Mạch",
                    VisionLayer.CelestialSilhouette => "Hình Bóng Thiên Giới",
                    VisionLayer.MiasmaSight => "Cảm Tri Hắc Ám",
                    _ => layer.ToString()
                };
            }

            return layer switch
            {
                VisionLayer.Normal => "Normal Vision",
                VisionLayer.MagicalAura => "Magical Aura",
                VisionLayer.EnergyPathways => "Energy Pathways",
                VisionLayer.CelestialSilhouette => "Celestial Silhouette",
                VisionLayer.MiasmaSight => "Miasma Sight",
                _ => layer.ToString()
            };
        }
    }
}
