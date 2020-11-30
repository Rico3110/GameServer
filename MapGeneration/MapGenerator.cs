﻿using System;
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

            Console.WriteLine("cellCountX: " + cellCountX);
            Console.WriteLine("cellCountZ: " + cellCountZ);


            float hexWidth = HexMetrics.innerRadius + 2f * HexMetrics.innerRadius * (float)cellCountX;
            float hexHeight = 0.5f * HexMetrics.outerRadius + 1.5f * HexMetrics.outerRadius * (float)cellCountZ;

            uint[] map = new uint[cellCountX * cellCountZ];

            for (int z = 0; z < cellCountZ; z++)
            {
                for (int x = 0; x < cellCountX; x++)
                {
                    float posX = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
                    float posZ = z * (HexMetrics.outerRadius * 1.5f);

                    int pixelX = (int)((posX / hexWidth) * (float)IMAGE_WIDTH);
                    int pixelZ = (int)((posZ / hexWidth) * (float)IMAGE_HEIGHT);

                    Console.WriteLine(pixelX + ", " + pixelZ);

                    Color landPixel = landImages[pixelX / 256, pixelZ / 256].GetPixel(pixelX % 256, pixelZ % 256);
                    Color heightPixel = heightImages[pixelX / 256, pixelZ / 256].GetPixel(pixelX % 256, pixelZ % 256);
                    float height = -10000f + ((float)((heightPixel.R * 256 * 256) + (heightPixel.G * 256) + heightPixel.B) * 0.1f);                    
                    HexCellData data = new HexCellData((ushort)(height), fromColorToBiome(landPixel), HexCellRessource.NONE);
                    
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

        private HexCellBiome fromColorToBiome(Color color)
        {
            if (color.Equals(Color.FromArgb(255, 55, 136, 48)) || color.Equals(Color.FromArgb(255,139,183,128)))
            {
                return HexCellBiome.FOREST;
            }else if (color.Equals(Color.FromArgb(255, 89, 220, 65)))
            {
                return HexCellBiome.GRASS;
            }else if (color.Equals(Color.FromArgb(255, 75, 189, 221)))
            {
                return HexCellBiome.WATER;
            }else if (color.Equals(Color.FromArgb(255, 189, 137, 97)))
            {
                return HexCellBiome.CROP;
            }else if (color.Equals(Color.FromArgb(255, 48, 48, 48)) || color.Equals(Color.FromArgb(255,255,76,77)))
            {
                return HexCellBiome.CITY;
            }
            else
            {
                return HexCellBiome.ROCK;
            }
        }
    }
}
