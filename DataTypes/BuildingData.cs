using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DataTypes
{
    public enum BuildingType
    {
        NONE, HQ, FARM
    }

    public class BuildingData
    {
        BuildingType Type { get; }
        byte Level { get; }
        byte TeamID { get; }
    }
}
