using System;
using System.Collections.Generic;
using System.Drawing;
using Shared.HexGrid;
using Shared.DataTypes;
using UnityEngine;
using Color = System.Drawing.Color;
using Shared.Structures;
using System.Linq;

namespace Shared.MapGeneration
{

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

        private readonly int CELL_COUNT;

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

        private Random random;

        private Dictionary<HexCellBiome, int> parsedBiomes;
        private Dictionary<HexCellBiome, int> cellQueryThresholds;
        private Dictionary<HexCellBiome, Tuple<int, int>> biomeThresholds;

        private int[] parsedRessources;
        private Dictionary<RessourceType, Tuple<int, int>> ressourceThresholds;

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

            CELL_COUNT = CELL_COUNT_X * CELL_COUNT_Z;

            HEX_WIDTH = HexMetrics.innerRadius + 2f * HexMetrics.innerRadius * (float)CELL_COUNT_X;
            HEX_HEIGHT = 0.5f * HexMetrics.outerRadius + 1.5f * HexMetrics.outerRadius * (float)CELL_COUNT_Z;


            float cornerLon = (float)Slippy.tilex2long((Slippy.long2tilex(LONGITUDE, MapboxHandler.ZOOM) - TILE_COUNT_X / 2), MapboxHandler.ZOOM);
            float deltaLon = TILE_COUNT_X * Math.Abs((float)Slippy.tilex2long(Slippy.long2tilex(LONGITUDE, MapboxHandler.ZOOM), MapboxHandler.ZOOM) - (float)Slippy.tilex2long((Slippy.long2tilex(LONGITUDE, MapboxHandler.ZOOM) + 1), MapboxHandler.ZOOM));

            float deltaLat = TILE_COUNT_Z * Math.Abs((float)Slippy.tiley2lat(Slippy.lat2tiley(LATITUDE, MapboxHandler.ZOOM), MapboxHandler.ZOOM) - (float)Slippy.tiley2lat((Slippy.lat2tiley(LATITUDE, MapboxHandler.ZOOM) + 1), MapboxHandler.ZOOM));
            float cornerLat = (float)Slippy.tiley2lat((Slippy.lat2tiley(LATITUDE, MapboxHandler.ZOOM) + TILE_COUNT_Z / 2 + 1), MapboxHandler.ZOOM) + 0.5f * ((HEX_WIDTH - HEX_HEIGHT) / HEX_WIDTH) * deltaLat;
            /*
            Console.WriteLine(cornerLon);
            Console.WriteLine(cornerLat);

            Console.WriteLine(cornerLon + deltaLon);
            Console.WriteLine(cornerLat + deltaLat);

            Console.WriteLine(0.5f * ((HEX_WIDTH - HEX_HEIGHT) / HEX_WIDTH) * deltaLat);
            */
            hexGrid = new HexGrid.HexGrid(CHUNK_COUNT_X, CHUNK_COUNT_Z, cornerLon, cornerLat, deltaLon, (HEX_HEIGHT / HEX_WIDTH) * deltaLat);

            waterAreas = new List<List<HexCell>>();

            random = new Random();

            cellQueryThresholds = new Dictionary<HexCellBiome, int>();
            foreach (HexCellBiome biome in Enum.GetValues(typeof(HexCellBiome)))
            {
                cellQueryThresholds.Add(biome, 4);
            }
            
            biomeThresholds = new Dictionary<HexCellBiome, Tuple<int, int>>
            {
                { HexCellBiome.FOREST,      new Tuple<int, int>((int)(0.05f * CELL_COUNT), (int)(0.4f * CELL_COUNT)) },
                { HexCellBiome.SCRUB,       new Tuple<int, int>((int)(0.05f * CELL_COUNT), (int)(0.4f * CELL_COUNT)) },
                { HexCellBiome.GRASS,       new Tuple<int, int>((int)(0.05f * CELL_COUNT), (int)(0.4f * CELL_COUNT)) },
                { HexCellBiome.CROP,        new Tuple<int, int>((int)(0.05f * CELL_COUNT), (int)(0.4f * CELL_COUNT)) },
                { HexCellBiome.COAL,        new Tuple<int, int>((int)(0.02f * CELL_COUNT), (int)(0.1f * CELL_COUNT)) },
                { HexCellBiome.ROCK,        new Tuple<int, int>((int)(0.04f * CELL_COUNT), (int)(0.4f * CELL_COUNT)) },
                { HexCellBiome.CITY,        new Tuple<int, int>((int)(0.1f * CELL_COUNT), (int)(0.2f * CELL_COUNT)) },
                { HexCellBiome.BUILDINGS,   new Tuple<int, int>((int)(0.05f * CELL_COUNT), (int)(0.2f * CELL_COUNT)) },
                { HexCellBiome.WATER,       new Tuple<int, int>((int)(0.005f * CELL_COUNT), (int)(0.1f * CELL_COUNT)) }
            };
            /*
            parsedRessources = new int[Enum.GetValues(RessourceType).Length];
            ressourceThresholds = new Dictionary<Type, int>{
                { RessourceType.COAL, new Tuple<int, int>((int)(0.005f * CELL_COUNT), (int)(0.01f * CELL_COUNT)) },
                { RessourceType.WOOD, new Tuple<int, int>((int)(0.005f * CELL_COUNT), (int)(0.01f * CELL_COUNT)) },
                { RessourceType.WHEAT, new Tuple<int, int>((int)(0.005f * CELL_COUNT), (int)(0.01f * CELL_COUNT)) },
                { RessourceType.FISH, new Tuple<int, int>((int)(0.005f * CELL_COUNT), (int)(0.01f * CELL_COUNT)) },
                { RessourceType.IRON_ORE, new Tuple<int, int>((int)(0.005f * CELL_COUNT), (int)(0.01f * CELL_COUNT)) },
                { RessourceType, new Tuple<int, int>((int)(0.005f * CELL_COUNT), (int)(0.01f * CELL_COUNT)) },
                { RessourceType.COAL, new Tuple<int, int>((int)(0.005f * CELL_COUNT), (int)(0.01f * CELL_COUNT)) },
            }
            */
        }

        public HexGrid.HexGrid CreateMap()
        {
            FetchMaps();           

            //Iterative rework generated map until its balanced
            int i = 0;
            do
            {
                ApplyBiomes();
                i++;
            }
            while(!CheckMap() && i < 10);

            updateWater();

            AddMissingBiomes();
            
            AddRessources();
            
            return hexGrid;
        }

        private void ApplyBiomes()
        {
            parsedBiomes = new Dictionary<HexCellBiome, int>();
            foreach (HexCellBiome biome in Enum.GetValues(typeof(HexCellBiome)))
            {
                parsedBiomes.Add(biome, 0);
            }
            
            for (int z = 0; z < CELL_COUNT_Z; z++)
            {
                for (int x = 0; x < CELL_COUNT_X; x++)
                {
                    HexCellBiome biome = parseBiome(x, z);
                    int height = parseHeight(x, z);
                    byte waterDepth = 0;
                    if (biome == HexCellBiome.WATER)
                    {
                        waterDepth = 1;
                    }
                    parsedBiomes[biome] += 1;
                    HexCellData cellData = new HexCellData(height, biome, waterDepth);
                    hexGrid.GetCell(x, z).Data = cellData;
                }
            }
        }

        private bool CheckMap() {
            bool mapOK = true;
            foreach (HexCellBiome biome in biomeThresholds.Keys)
            {
                if (parsedBiomes[biome] < biomeThresholds[biome].Item1)
                {
                    mapOK = false;
                    this.cellQueryThresholds[biome] = Mathf.Clamp(--this.cellQueryThresholds[biome], 0, 8);
                }
                if (parsedBiomes[biome] > biomeThresholds[biome].Item2)
                {
                    mapOK = false;
                    this.cellQueryThresholds[biome] = Mathf.Clamp(++this.cellQueryThresholds[biome], 0, 8);
                }
            }
            return mapOK;
        }

        private void AddMissingBiomes()
        {
            foreach(HexCellBiome biome in biomeThresholds.Keys)
            {
                if (biome != HexCellBiome.WATER && biome != HexCellBiome.BUILDINGS && biome != HexCellBiome.CITY && biome != HexCellBiome.SNOW)
                {
                    if(parsedBiomes[biome] < biomeThresholds[biome].Item1)
                    {
                        //Console.WriteLine("Adjusting missing: " + biome.ToString());
                        if (biome == HexCellBiome.ROCK || biome == HexCellBiome.COAL)
                        {
                            List<HexCell> buildings = new List<HexCell>();
                            foreach(HexCell cell in hexGrid.cells)
                            {
                                if(cell.Data.Biome == HexCellBiome.BUILDINGS)
                                {
                                    buildings.Add(cell);
                                }
                            }
                            int count3 = 0;
                            if(biome == HexCellBiome.COAL)
                                count3 = Mathf.Min(biomeThresholds[biome].Item1 - parsedBiomes[biome], (int)(((float)biomeThresholds[biome].Item1 / (float)(biomeThresholds[HexCellBiome.COAL].Item1 + (float)biomeThresholds[HexCellBiome.ROCK].Item1 + 1.0f)) * (float)buildings.Count));
                            else
                                count3 = Mathf.Min(biomeThresholds[biome].Item1 - parsedBiomes[biome], buildings.Count);
                            ReplaceMissingBiome(biome, HexCellBiome.BUILDINGS, count3);
                        }
                        /*
                        HexCellBiome majority = this.parsedBiomes.Aggregate(
                            HexCellBiome.FOREST, 
                            (HexCellBiome agg, KeyValuePair<HexCellBiome, int> elem) => agg = (elem.Value > parsedBiomes[agg] ? elem.Key : agg));
                        */
                        HexCellBiome majorityBiome = this.parsedBiomes.Aggregate(HexCellBiome.FOREST, (HexCellBiome elem1, KeyValuePair<HexCellBiome, int> elem2) =>
                        {
                            if (parsedBiomes[elem1] < elem2.Value)
                                return elem2.Key;
                            else
                                return elem1;
                        });

                        int count = biomeThresholds[biome].Item1 - parsedBiomes[biome];
                        ReplaceMissingBiome(biome, majorityBiome, count);
                        
                    }

                }

            }
        }

        private void ReplaceMissingBiome(HexCellBiome biome, HexCellBiome replace, int count)
        {
            List<HexCell> replaceCells = new List<HexCell>();
            foreach (HexCell cell in hexGrid.cells)
            {
                if (cell.Data.Biome == replace)
                {
                    replaceCells.Add(cell);
                }
            }
            while (count > 0)
            {
                //Console.WriteLine("Replacing " + replace.ToString() + " with " + biome.ToString());
                HexCell cell = replaceCells[random.Next(0, replaceCells.Count)];
                //cell.Data.SetBiome(biome);
                cell.Data = new HexCellData(cell.Data.Elevation, biome, cell.Data.WaterDepth);
                //Console.WriteLine(cell.Data.Biome.ToString() + biome.ToString());
                this.parsedBiomes[biome]++;
                this.parsedBiomes[replace]--;
                replaceCells.Remove(cell);
                count--;
            }
        }

        //mandatory: Stone, Wood (Forest/Scrub), Coal, Cows, 
        //Optional: Fish, (Wheat), 
        private void AddRessources()
        {
            foreach(HexCell cell in this.hexGrid.cells)
            {
                switch (cell.Data.Biome)
                {
                    case HexCellBiome.FOREST:
                    {
                        cell.Structure = new Tree(cell, 0);
                        break;
                    }
                    case HexCellBiome.ROCK:
                    {
                        cell.Structure = new Rock(cell, 0);
                        break;
                    }
                    case HexCellBiome.WATER:
                    {
                        double r = random.NextDouble();
                        if (r < 0.1)
                            cell.Structure = new Fish(cell, 0);
                        break;
                    }
                    case HexCellBiome.SCRUB:
                    {
                        cell.Structure = new Scrub(cell, 0);
                        break;
                    }
                    case HexCellBiome.GRASS:
                    {
                        cell.Structure = new Grass(cell, 0);
                        break;
                    }
                    case HexCellBiome.CITY:
                    {
                        break;
                    }
                    case HexCellBiome.COAL:
                        {
                            cell.Structure = new CoalOre(cell, 0);
                            break;
                        }
                    case HexCellBiome.CROP:
                    {
                        //if (random.NextDouble() < 0.2)
                        //    cell.Structure = new Wheat(cell, 0);
                        break;
                    }
                    default: 
                    {
                        break;
                    }
                }
            }
        }

        private HexCellBiome parseBiome(int x, int z)
        {
            float posX = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f) + 0.5f * HexMetrics.outerRadius;
            float posZ = z * (HexMetrics.outerRadius * 1.5f) + 0.5f * HexMetrics.innerRadius;
           
            Color landPixel = GetPixel(landImages, posX, posZ);
            //return fromColorToBiome(landPixel);
            
            List<Tuple<HexCellBiome, int>> foundBiomes = new List<Tuple<HexCellBiome, int>>();

            int index = foundBiomes.FindIndex(elem => elem.Item1 == fromColorToBiome(landPixel));
            if (index == -1)
                foundBiomes.Add(new Tuple<HexCellBiome, int>(fromColorToBiome(landPixel), 2));
            else
                foundBiomes[index] = new Tuple<HexCellBiome, int>(fromColorToBiome(landPixel), foundBiomes[index].Item2 + 2);

            Vector3 position = new Vector3(posX, 0, posZ);


            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                position += 0.5f * HexMetrics.GetFirstCorner(d);

                Color pixel = GetPixel(landImages, position.x, position.z);
                HexCellBiome biome = fromColorToBiome(pixel);

                index = foundBiomes.FindIndex(elem => elem.Item1 == biome);
                if(index == -1)
                    foundBiomes.Add(new Tuple<HexCellBiome, int>(biome, 1));
                else
                    foundBiomes[index] = new Tuple<HexCellBiome, int>(biome, foundBiomes[index].Item2 + 1);

                position -= 0.5f * HexMetrics.GetFirstCorner(d);
            }


            foundBiomes.Sort((a, b) => b.Item2 - a.Item2);
            foreach (Tuple<HexCellBiome, int> tpl in foundBiomes)
            {
                if (cellQueryThresholds[tpl.Item1] <= tpl.Item2)
                {
                    return tpl.Item1;
                }    
            }
            
            //gooaphtermayoriti
            return foundBiomes[0].Item1;
        }

        private int parseHeight(int x, int z)
        {
            float posX = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
            float posZ = z * (HexMetrics.outerRadius * 1.5f);

            Color heightPixel = GetPixel(heightImages, posX, posZ);
            float height = -10000f + ((float)((heightPixel.R * 256 * 256) + (heightPixel.G * 256) + heightPixel.B) * 0.1f);
            
            return (int)height / 8;
        }

        private byte parseWater(int x, int z){
            float posX = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f) + 0.5f * HexMetrics.outerRadius;
            float posZ = z * (HexMetrics.outerRadius * 1.5f) + 0.5f * HexMetrics.innerRadius;

            Vector3 position = new Vector3(posX, 0, posZ);

            int waterCount = 0;

            Color waterPixel = GetPixel(waterImages, posX, posZ);

            if (waterPixel == Color.FromArgb(255, 59, 176, 170))
            {
                waterCount++;
            }
            
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                position += 0.5f * HexMetrics.GetFirstCorner(d);

                waterPixel = GetPixel(waterImages, position.x, position.z);
                
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
            int minElevation = int.MaxValue;
            HexCell neighbor;
            int neighborElevation;
            foreach (HexCell cell in area)
            {
                int cellElevation = cell.Elevation;
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
                cell.Data = new HexCellData(minElevation, cell.Data.Biome, cell.Data.WaterDepth);
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
                    cell.Data = new HexCellData(data.Elevation, data.Biome, (byte)(1 + waterCount / 2));
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
                        if (cell.GetElevationDifference(d) > 100)
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
            // waterImages = MapboxHandler.FetchWaterMap(LONGITUDE, LATITUDE, TILE_COUNT_X, TILE_COUNT_Z);
        }

        private Color GetPixel(Bitmap[,] imageArray, float x, float z)
        {
            int pixelX = (int)((x / HEX_WIDTH) * (float)IMAGE_WIDTH);
            int pixelZ = (int)((z / HEX_WIDTH) * (float)IMAGE_HEIGHT + (HEX_WIDTH - HEX_HEIGHT) / 2f);

            return imageArray[pixelX / 256, pixelZ / 256].GetPixel(pixelX % 256, 255 - pixelZ % 256);
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
            }else if (color.Equals(Color.FromArgb(255, 96, 173, 195)))
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
            else if(color.Equals(Color.FromArgb(255, 107, 107, 107)))
            {
                return HexCellBiome.ROCK;
            }
            return HexCellBiome.WATER;
        }
    }
}
