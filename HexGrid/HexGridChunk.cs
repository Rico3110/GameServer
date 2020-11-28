using System.Collections;
using System.Collections.Generic;

public class HexGridChunk
{
    HexCell[] cells;    

    private HexGridChunk()
    {        
        cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
    }    

    public void AddCell(int index, HexCell cell)
    {
        cells[index] = cell;
        cell.chunk = this;       
    }    
}
