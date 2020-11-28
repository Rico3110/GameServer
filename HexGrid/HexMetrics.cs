using System.Collections;
using System.Collections.Generic;

public static class HexMetrics
{
    public const float outerRadius = 10f;
    public const float innerRadius = outerRadius * 0.866025404f;

    public const float solidFactor = 0.8f;
    public const float blendFactor = 1f - solidFactor;

    public const float elevationStep = .4f;

    public const int chunkSizeX = 5, chunkSizeZ = 5;
    

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
}
