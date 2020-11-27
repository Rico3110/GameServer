using System;
using System.Collections;
using System.Drawing;
using System.Collections.Generic;

namespace GameServer.Map
{
    public enum Biome
    {
        FOREST = 0x0,
        WATER = 0x1,
        GRASS = 0x2,
        FARM = 0x3,
        CITY = 0x4,
        DESERT = 0x5,
        SNOW = 0x6,
        MOUNTAIN = 0x7,
    }

    public static class BiomeExtension
    {
        private static Dictionary<Color, Biome> colorToBiome = new Dictionary<Color, Biome>();
        
        static BiomeExtension()
        {
            colorToBiome.Add(Color.FromArgb(255, 48, 48, 48), Biome.CITY);
        }
        
        public static Biome toBiome(Color color)
        {
            Biome ret = new Biome();
            if (colorToBiome.TryGetValue(color, out ret))
            {
                return ret;
            } 
            else
            {
                return Biome.GRASS;
            }
        }
    }
}