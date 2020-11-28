using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexMetrics
{
    public const float outerRadius = 10f;
    public const float innerRadius = outerRadius * 0.866025404f;

    public const float solidFactor = 0.8f;
    public const float blendFactor = 1f - solidFactor;

    public const float elevationStep = .4f;

    public const int chunkSizeX = 5, chunkSizeZ = 5;

    public static Texture2D noiseSource;

    public const float noiseScale = 0.003f;
    public const float cellPerturbStrength = 1.5f;
    public const float elevationPerturbStrength = 0.5f;

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

    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return corners[(int)direction] * solidFactor;
    }

    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return corners[((int)direction + 1) % 6] * solidFactor;
    }

    public static Vector3 GetBridge(HexDirection direction)
    {
        return (corners[(int)direction] + corners[((int)direction + 1) % 6]) * blendFactor;
    }

    public static Vector4 SampleNoise(Vector3 position)
    {
        return noiseSource.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);
    }

    public static Color ToColor(this HexCellBiome biome)
    {
        switch (biome)
        {
            case HexCellBiome.FOREST:
                return new Color32(95, 163, 105, 255);
            case HexCellBiome.GRASS:
                return new Color32(159, 209, 113, 255);
            case HexCellBiome.CROP:
                return new Color32(204, 154, 84, 255);
            case HexCellBiome.ROCK:
                return new Color32(82, 82, 82, 255);
            case HexCellBiome.SNOW:
                return new Color32(252, 252, 252, 255);
            case HexCellBiome.CITY:
                return new Color(207, 60, 60, 255);
            default:
                return Color.white;
        }
    }
}
