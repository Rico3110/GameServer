using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.MapGeneration
{
    class Slippy
    {
        public static int long2tilex(double lon, int z)
        {
            return (int)(Math.Floor((lon + 180.0) / 360.0 * (1 << z)));
        }

        public static int lat2tiley(double lat, int z)
        {
            return (int)Math.Floor((1 - Math.Log(Math.Tan(ToRadians(lat)) + 1 / Math.Cos(ToRadians(lat))) / Math.PI) / 2 * (1 << z));
        }

        public static double tilex2long(int x, int z)
        {
            return (double)x / (double)(1 << z) * 360.0 - 180;
        }

        public static double tiley2lat(int y, int z)
        {
            double n = Math.PI - 2.0 * Math.PI * (double)y / (double)(1 << z);
            return 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)));
        }

        private static double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
    }
}
