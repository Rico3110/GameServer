using System.Collections;
using System.Collections.Generic;


public class HexCell 
{
    public HexCoordinates coordinates;

    public HexGridChunk chunk;
       
    private HexCell[] neighbors;

    private HexCellData data;

    private BuildingData building;

    public BuildingData Building
    {
        get
        {
            return building;
        }
    }

    public HexCellData Data 
    {
        get
        {
            return data;
        }
        set
        {
            data = value;           
            Refresh();
        }
    }   

       
    public int GetElevationDifference(HexDirection direction)
    {
        int difference = (int)Data.Elevation - (int)GetNeighbor(direction).Data.Elevation;
        return difference;
    }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void setNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    void RefreshSelfOnly()
    {
        chunk.Refresh();
    }

    void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();
            for(int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if(neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }
}
