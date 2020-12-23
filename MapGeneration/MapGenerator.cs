using System;
using System.Collections.Generic;
using System.Drawing;
using Shared.HexGrid;
using Shared.DataTypes;
using UnityEngine;
using Color = System.Drawing.Color;

namespace Shared.MapGeneration
{
    internal struct Cell
    {
        public int x;
        public int z;

        public Cell(int x, int z)
        {
            this.x = x;
            this.z = z;
        }
    }

    public class MapGenerator
    {      
        private readonly float LONGITUDE;
        private readonly float LATITUDE;

        private readonly int TILE_COUNT_X;
        private readonly int TILE_COUNT_Z;
        
        private readonly int IMAGE_WIDTH;
        private readonly int IMAGE_HEIGHT;

        private readonly int CHUNK_COUNT_X;
        private readonly int CHUNK_COUNT_Z;

        private readonly int CELL_COUNT_X;
        private readonly int CELL_COUNT_Z;

        private readonly float HEX_WIDTH;
        private readonly float HEX_HEIGHT;

        private const int CHUNKS_PER_TILE_X = 2;
        private const int CHUNKS_PER_TILE_Z = 2;

        private const int SINGLE_IMAGE_WIDTH = 256;
        private const int SINGLE_IMAGE_HEIGHT = 256;


        private HexGrid.HexGrid hexGrid;

        List<List<HexCell>> waterAreas;

        private Bitmap[,] landImages;
        private Bitmap[,] heightImages;
        private Bitmap[,] waterImages;

        private bool[] visited;
        

        public MapGenerator(float lat, float lon, int size)
        {
            LONGITUDE = lon;
            LATITUDE = lat;

            TILE_COUNT_X = size;
            TILE_COUNT_Z = size;

            IMAGE_WIDTH = size * SINGLE_IMAGE_WIDTH;
            IMAGE_HEIGHT = size * SINGLE_IMAGE_HEIGHT;

            CHUNK_COUNT_X = TILE_COUNT_X * CHUNKS_PER_TILE_X;
            CHUNK_COUNT_Z = TILE_COUNT_Z * CHUNKS_PER_TILE_Z;

            CELL_COUNT_X = CHUNK_COUNT_X * HexMetrics.chunkSizeX;
            CELL_COUNT_Z = CHUNK_COUNT_Z * HexMetrics.chunkSizeZ;

            HEX_WIDTH = HexMetrics.innerRadius + 2f * HexMetrics.innerRadius * (float)CELL_COUNT_X;
            HEX_HEIGHT = 0.5f * HexMetrics.outerRadius + 1.5f * HexMetrics.outerRadius * (float)CELL_COUNT_Z;

            hexGrid = new HexGrid.HexGrid(CHUNK_COUNT_X, CHUNK_COUNT_Z);

            waterAreas = new List<List<HexCell>>();
        }

        public HexGrid.HexGrid createMap()
        {
            FetchMaps();           
         
            for (int z = 0; z < CELL_COUNT_Z; z++)
            {
                for (int x = 0; x < CELL_COUNT_X; x++)
                {
                    HexCellBiome biome = parseBiome(x, z);
                    ushort height = parseHeight(x, z);
                    byte waterDepth = parseWater(x, z);                   
                    if(waterDepth != 0)
                    {
                        biome = HexCellBiome.WATER;
                    }
                    HexCellData cellData = new HexCellData(height, biome, waterDepth);
                    hexGrid.GetCell(x, z).Data = cellData;                    
                }
            }

            updateWater();
            UpdateRocks();
            return hexGrid;
        }

        private HexCellBiome parseBiome(int x, int z)
        {
            float posX = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f) + 0.5f * HexMetrics.outerRadius;
            float posZ = z * (HexMetrics.outerRadius * 1.5f) + 0.5f * HexMetrics.innerRadius;
          
            int pixelX = (int)((posX / HEX_WIDTH) * (float)IMAGE_WIDTH);
            int pixelZ = (int)((posZ / HEX_WIDTH) * (float)IMAGE_HEIGHT);

            Vector3 position = new Vector3(posX, 0, posZ);

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                position += 0.5f * HexMetrics.GetFirstCorner(d);

                int pixX = (int)((position.x / HEX_WIDTH) * (float)IMAGE_WIDTH);
                int pixZ = (int)((position.z / HEX_WIDTH) * (float)IMAGE_HEIGHT);

                System.Drawing.Color landPix = landImages[pixX / 256, pixZ / 256].GetPixel(pixX % 256, 255 - pixZ % 256);

                if(fromColorToBiome(landPix) == HexCellBiome.WATER){
                    return HexCellBiome.WATER;
                }

                position -= 0.5f * HexMetrics.GetFirstCorner(d);
            }

            Color landPixel = landImages[pixelX / 256, pixelZ / 256].GetPixel(pixelX % 256, 255 - pixelZ % 256);
            return fromColorToBiome(landPixel);
        }

        private ushort parseHeight(int x, int z)
        {
            float posX = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
            float posZ = z * (HexMetrics.outerRadius * 1.5f);
        
            int pixelX = (int)((posX / HEX_WIDTH) * (float)IMAGE_WIDTH);
            int pixelZ = (int)((posZ / HEX_WIDTH) * (float)IMAGE_HEIGHT);

            Color heightPixel = heightImages[pixelX / 256, pixelZ / 256].GetPixel(pixelX % 256, 255 - pixelZ % 256);
            float height = -10000f + ((float)((heightPixel.R * 256 * 256) + (heightPixel.G * 256) + heightPixel.B) * 0.1f);

            return (ushort)height;
        }

        private byte parseWater(int x, int z){
            float posX = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f) + 0.5f * HexMetrics.outerRadius;
            float posZ = z * (HexMetrics.outerRadius * 1.5f) + 0.5f * HexMetrics.innerRadius;

            int pixelX = (int)((posX / HEX_WIDTH) * (float)IMAGE_WIDTH);
            int pixelZ = (int)((posZ / HEX_WIDTH) * (float)IMAGE_HEIGHT);

            Vector3 position = new Vector3(posX, 0, posZ);

            int waterCount = 0;

            Color waterPixel = waterImages[pixelX / 256, pixelZ / 256].GetPixel(pixelX % 256, 255 - pixelZ % 256);

            if (waterPixel == Color.FromArgb(255, 59, 176, 170))
            {
                waterCount++;
            }
            
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                position += 0.5f * HexMetrics.GetFirstCorner(d);

                int pixX = (int)((position.x / HEX_WIDTH) * (float)IMAGE_WIDTH);
                int pixZ = (int)((position.z / HEX_WIDTH) * (float)IMAGE_HEIGHT);

                waterPixel = waterImages[pixX / 256, pixZ / 256].GetPixel(pixX % 256, 255 - pixZ % 256);
                
                if (waterPixel == Color.FromArgb(255, 59, 176, 170))
                {
                    waterCount++;
                }

                position -= 0.5f * HexMetrics.GetFirstCorner(d);
            }    
            
            if(waterCount > 1)
            {
                return 1;
            }
            return 0;
        }

        public void updateWater()
        {
            visited = new bool[CELL_COUNT_X * CELL_COUNT_Z];

            for (int z = 0; z < CELL_COUNT_Z; z++)
            {
                for (int x = 0; x < CELL_COUNT_X; x++)
                {
                    HexCell cell = hexGrid.GetCell(x, z);
                    if (visited[x + z * CELL_COUNT_X] == false)
                    {
                        if (cell.Data.WaterDepth == 1)
                        {
                            waterAreas.Add(findWaterArea(cell));
                        }
                        visited[x + z * CELL_COUNT_X] = true;                       
                    }
                }
            }
            
            foreach (List<HexCell> area in waterAreas)
            {
                adjustWaterArea(area);
            }

            updateWaterDepth();
        }

        private void adjustWaterArea(List<HexCell> area)
        {
            uint minElevation = int.MaxValue;
            HexCell neighbor;
            uint neighborElevation;
            foreach (HexCell cell in area)
            {
                uint cellElevation = cell.Elevation;
                if (cellElevation < minElevation)
                {
                    minElevation = cellElevation;
                }
                
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    neighbor = cell.GetNeighbor(d);
                    if(neighbor != null)
                    {
                        neighborElevation = neighbor.Elevation;
                        if (neighborElevation < minElevation)
                        {
                            minElevation = neighborElevation;
                        }
                    }                    
                }
            }
            foreach (HexCell cell in area)
            {
                HexCellData cellData = cell.Data;         
                cell.Data = new HexCellData((ushort)minElevation, cell.Data.Biome, cell.Data.WaterDepth);
            }
        }

        public List<HexCell> findWaterArea(HexCell cell)
        {
            Queue<HexCell> Queue = new Queue<HexCell>();
            Queue.Enqueue(cell);

            List<HexCell> area = new List<HexCell>();

            HexCell current;
            HexCell neighbor;
            while (Queue.Count != 0)
            {
                current = Queue.Dequeue();
                area.Add(current);               
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    neighbor = current.GetNeighbor(d);             
                    if (neighbor != null &&
                        neighbor.Data.WaterDepth == 1 && 
                        !visited[neighbor.coordinates.ToOffsetX() + neighbor.coordinates.ToOffsetZ() * CELL_COUNT_X])
                    {
                        Queue.Enqueue(neighbor);
                        visited[neighbor.coordinates.ToOffsetX() + neighbor.coordinates.ToOffsetZ() * CELL_COUNT_X] = true;
                    }
                }
            }
            return area;
        }

        private void updateWaterDepth()
        {
            foreach(HexCell cell in hexGrid.cells)
            {
                if(cell.Data.Biome == HexCellBiome.WATER)
                {
                    int waterCount = 0;
                    HexCell neighbor;
                    for(HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                    {
                        neighbor = cell.GetNeighbor(d);
                        if(neighbor != null && neighbor.Data.Biome == HexCellBiome.WATER)
                        {
                            waterCount++;
                        }
                    }
                    HexCellData data = cell.Data;
                    cell.Data = new HexCellData(data.Elevation, data.Biome, (byte)(5 + 3 * waterCount));
                }
            }
        }

        private void UpdateRocks()
        {
            foreach (HexCell cell in hexGrid.cells)
            {
                if(cell.Data.Biome != HexCellBiome.WATER)
                {
                    for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                    {
                        if (cell.GetElevationDifference(d) > 150)
                        {
                            HexCellData data = cell.Data;
                            cell.Data = new HexCellData(data.Elevation, HexCellBiome.ROCK, data.WaterDepth);
                        }
                    }
                }
                
            }
        }

        private void FetchMaps()
        {           
            landImages = MapboxHandler.FetchLandcoverMap(LONGITUDE, LATITUDE, TILE_COUNT_X, TILE_COUNT_Z);
            heightImages = MapboxHandler.FetchHeightMap(LONGITUDE, LATITUDE, TILE_COUNT_X, TILE_COUNT_Z);
            waterImages = MapboxHandler.FetchWaterMap(LONGITUDE, LATITUDE, TILE_COUNT_X, TILE_COUNT_Z);
        }



        private HexCellBiome fromColorToBiome(Color color)
        {
            if (color.Equals(Color.FromArgb(255, 55, 136, 48)))
            {
                return HexCellBiome.FOREST;
            }else if(color.Equals(Color.FromArgb(255, 139, 183, 128)))
            {
                return HexCellBiome.SCRUB;
            }else if (color.Equals(Color.FromArgb(255, 89, 220, 65)))
            {
                return HexCellBiome.GRASS;
            }else if (color.Equals(Color.FromArgb(255, 75, 189, 221)))
            {
                return HexCellBiome.WATER;
            }else if (color.Equals(Color.FromArgb(255, 189, 137, 97)))
            {
                return HexCellBiome.CROP;
            }else if (color.Equals(Color.FromArgb(255, 48, 48, 48)))
            {
                return HexCellBiome.CITY;
            }else if (color.Equals(Color.FromArgb(255, 255, 76, 77)))
            {
                return HexCellBiome.BUILDINGS;
            }
            else
            {
                return HexCellBiome.ROCK;
            }
        }
    }
}
