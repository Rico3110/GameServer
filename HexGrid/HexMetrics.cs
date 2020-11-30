using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace GameServer.HexGrid
{
    public static class HexMetrics
    {
        public const float outerRadius = 10f;
        public const float innerRadius = outerRadius * 0.866025404f;

        public const float elevationStep = .4f;

        public const int chunkSizeX = 5, chunkSizeZ = 5;
        
        public static Vector3[] corners =
        {
            new Vector3(0f, 0f, outerRadius),
            new Vector3(innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(0f, 0f, -outerRadius),
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0f, 0.5f * outerRadius)
        };

        public static Vector3 GetFirstCorner(HexDirection direction)
        {
            return corners[(int)direction];
        }

        public static Vector3 GetSecondCorner(HexDirection direction)
        {
            return corners[((int)direction + 1) % 6];
        }              
    }
}

