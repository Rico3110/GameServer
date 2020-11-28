using System.Collections;
using System.Collections.Generic;

namespace GameServer.HexGrid
{
    public class HexGridChunk
    {
        HexCell[] cells;

        public HexGridChunk()
        {
            cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
        }

        public void AddCell(int index, HexCell cell)
        {
            cells[index] = cell;
            cell.chunk = this;
        }
    }
}
