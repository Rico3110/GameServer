using System.Collections;
using System.Collections.Generic;
using GameServer.DataTypes;

namespace GameServer.HexGrid
{
    public class HexCell
    {
        public HexCoordinates coordinates;

        public HexGridChunk chunk;

        private HexCell[] neighbors;

        public HexCellData Data { get; set; }

        public BuildingData Building { get; set; }



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
    }
}

