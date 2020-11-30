using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GameServer.DataTypes
{
    public enum HexCellBiome
    {
        FOREST, GRASS, CROP, ROCK, SNOW, CITY, WATER
    }

    public enum HexCellRessource
    {
        NONE, TREES, ROCKS, IRON_ORE
    }

    public struct HexCellData
    {
        public uint Elevation { get; }
        public HexCellBiome Biome { get; }
        public HexCellRessource Ressource { get; }

        public HexCellData(uint elevation, HexCellBiome biome, HexCellRessource ressource)
        {
            Elevation = elevation;
            Biome = biome;
            Ressource = ressource;
        }

        public HexCellData(uint data)
        {
            Elevation = data.toElevation();
            Biome = data.toBiome();
            Ressource = data.toRessource();
        }

        public uint toUint()
        {
            uint data = 0;
            data = data.SetSubBits(Elevation, 0, 16);
            data = data.SetSubBits((uint)Biome, 16, 4);
            data = data.SetSubBits((uint)Ressource, 20, 4);
            return data;
        }

        public string toString()
        {
            return this.Biome + ", " + this.Elevation + ", " + this.Ressource;
        }
    }

    internal static class HexCellDataHelper
    {
        internal static uint toElevation(this uint data)
        {
            return data.GetSubBits(0, 16);
        }

        internal static HexCellBiome toBiome(this uint data)
        {
            return (HexCellBiome)data.GetSubBits(16, 4);
        }
        internal static HexCellRessource toRessource(this uint data)
        {
            return (HexCellRessource)data.GetSubBits(20, 4);
        }
    }
}
