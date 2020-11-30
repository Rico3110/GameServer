using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using GameServer.GameState;
using GameServer.HexGrid;
using GameServer.DataTypes;

namespace GameServer.MapGeneration
{
    public class MapGenerator
    {      
        private readonly double LONGITUDE;
        private readonly double LATITUDE;

        private readonly int TILE_COUNT_X;
        private readonly int TILE_COUNT_Y;
        
        private readonly int IMAGE_WIDTH;
        private readonly int IMAGE_HEIGHT;

        private const int CHUNKS_PER_TILE_X = 5;
        private const int CHUNKS_PER_TILE_Y = 5;

        private const int SINGLE_IMAGE_WIDTH = 256;
        private const int SINGLE_IMAGE_HEIGHT = 256;


        private Map map;

        private Bitmap[,] landImages;
        private Bitmap[,] heightImages;


        public MapGenerator(double lat, double lon, int size)
        {
            LONGITUDE = lon;
            LATITUDE = lat;

            TILE_COUNT_X = size;
            TILE_COUNT_Y = size;

            IMAGE_WIDTH = size * SINGLE_IMAGE_WIDTH;
            IMAGE_HEIGHT = size * SINGLE_IMAGE_HEIGHT;            
        }

        public uint[] createMap()
        {
            FetchMaps();

            int cellCountX = TILE_COUNT_X * CHUNKS_PER_TILE_X * HexMetrics.chunkSizeX;
            int cellCountZ = TILE_COUNT_Y * CHUNKS_PER_TILE_Y * HexMetrics.chunkSizeZ;

            float hexWidth = HexMetrics.innerRadius + 2f * HexMetrics.innerRadius * (float)cellCountX;
            float hexHeight = 0.5f * HexMetrics.outerRadius + 1.5f * HexMetrics.outerRadius * (float)cellCountZ;

            uint[] map = new uint[cellCountX * cellCountZ];

            for (int z = 0; z < cellCountZ; z++)
            {
                for (int x = 0; x < cellCountX; x++)
                {
                    double posX = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
                    double posZ = z * (HexMetrics.outerRadius * 1.5f);

                    int pixelX = (int)((posX / hexWidth) * IMAGE_WIDTH);
                    int pixelZ = (int)((posZ / hexWidth) * IMAGE_HEIGHT);

                    Color landPixel = landImages[pixelX / 256, pixelZ / 256].GetPixel(pixelX % 256, pixelZ % 256);
                    Color heightPixel = heightImages[pixelX / 256, pixelZ / 256].GetPixel(pixelX % 256, pixelZ % 256);
                    float height = -10000f + ((float)((heightPixel.R * 256 * 256) + (heightPixel.G * 256) + heightPixel.B) * 0.1f);
                    Console.WriteLine(height);
                    HexCellData data = new HexCellData((ushort)(height), HexCellBiome.CROP, HexCellRessource.NONE);
                    Console.WriteLine(data.toString());
                    Console.WriteLine("kjdsagds");
                    map[z * cellCountX + x] = data.toUint();
                }
            }
            return map;
        }

        private void FetchMaps()
        {           
            landImages = MapboxHandler.FetchLandcoverMap(LONGITUDE, LATITUDE, TILE_COUNT_X, TILE_COUNT_Y);
            heightImages = MapboxHandler.FetchHeightMap(LONGITUDE, LATITUDE, TILE_COUNT_X, TILE_COUNT_Y);
        }
    }
}
