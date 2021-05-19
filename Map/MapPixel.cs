using System.Collections.Generic;

namespace WorldTens.Map
{
    public enum PixelStatus {
        City,
        Grass,
        Road,
        Nuked,
        Ruines,
        Water,
        Airport,
    }
    public class MapPixel
    {
        public PixelStatus status = PixelStatus.Grass;
    }
}