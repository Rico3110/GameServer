using System;
using System.Collections;
using System.Collections.Generic;


namespace GameServer.HexGrid
{
    public struct HexCoordinates
    {
        private int x, z;

        public int X
        {
            get
            {
                return x;
            }
        }

        public int Y
        {
            get
            {
                return -X - Z;
            }
        }

        public int Z
        {
            get
            {
                return z;
            }
        }

        public HexCoordinates(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public static HexCoordinates FromOffsetCoordinates(int x, int z)
        {
            return new HexCoordinates(x - z / 2, z);
        }

        public static HexCoordinates FromPosition(float px, float pz)
        {
            float x = px / (HexMetrics.innerRadius * 2f);
            float y = -x;
            float offset = pz / (HexMetrics.outerRadius * 3f);
            x -= offset;
            y -= offset;
            int iX = (int)Math.Round(x);
            int iY = (int)Math.Round(y);
            int iZ = (int)Math.Round(-x - y);

            if (iX + iY + iZ != 0)
            {
                float dX = Math.Abs(x - iX);
                float dY = Math.Abs(y - iY);
                float dZ = Math.Abs(-x - y - iZ);

                if (dX > dY && dX > dZ)
                {
                    iX = -iY - iZ;
                }
                else if (dZ > dY)
                {
                    iZ = -iX - iY;
                }
            }

            return new HexCoordinates(iX, iZ);
        }



        public override string ToString()
        {
            return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
        }

        public string ToStringOnSeperateLines()
        {
            return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
        }
    }
}

