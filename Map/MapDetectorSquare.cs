using System.Collections.Generic;
using WorldTens.Politics;

namespace WorldTens.Map
{
    public class MapDetectorSquare
    {
        public List<Creation> creations = new List<Creation>();
        public Vector2 position;
        public Vector2 wh = new Vector2(20, 20);
        public Country country = null;

        public MapDetectorSquare(Vector2 pos) {
            position = pos;
        }
    }
}