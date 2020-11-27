using System.Drawing;
using System;

namespace GameServer.Map
{
    public class Map
    {
        MapboxHandle mapbox;
        HexCell[,] map;

        private int numberOfHexTilesPerTileX = 10;
        private int numberOfHexTilesPerTileY = 10;

        public Map()
        {
            this.initMap(49.889347, 8.667032);
        }
        public Map(double lat, double lon)
        {
            this.initMap(lat, lon);
        }

        private void initMap(double lat, double lon)
        {
            if (map == null)
            {
                this.mapbox = new MapboxHandle();
                this.mapbox.fetchMap(lat, lon);
                this.map = new HexCell[this.mapbox.GetTileCountX() * this.numberOfHexTilesPerTileX, this.mapbox.GetTileCountY() * this.numberOfHexTilesPerTileY];

                for (int i = 0; i < this.mapbox.GetTileCountX(); i++)
                {
                    for (int j = 0; j < this.mapbox.GetTileCountY(); j++)
                    {
                        Bitmap landcoverImg = new Bitmap(this.mapbox.landCoverMapImages[i,j]);
                        Bitmap heightImg = new Bitmap(this.mapbox.heightMapImages[i,j]);
                        for (int k = 0; k < this.numberOfHexTilesPerTileX; k++)
                        {
                            for (int l = 0; l < this.numberOfHexTilesPerTileY; l++)
                            {
                                Color landPixel = landcoverImg.GetPixel(k * (256 / 10), l * (256 / 10));
                                Color heightPixel = heightImg.GetPixel(k * (256 / 10), l * (256 / 10));
                                float height = -10000f + (((heightPixel.R * 255f * 256f * 256f) + (heightPixel.G * 255f * 256f) + heightPixel.B * 255f) * 0.1f);

                                HexCell hc = new HexCell(BiomeExtension.toBiome(landPixel), (ushort)(height - 100), Ressource.NONE);
                                this.map[k, l] = hc;
                                Console.WriteLine(hc.ToString());
                            }
                        }
                    }
                }
            }
        }
    }
}