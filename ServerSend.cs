using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Structures;

namespace GameServer
{
    class ServerSend
    {
        public static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        public static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }

        public static void SendTCPDataToAll(int exceptClient,Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if(i != exceptClient)
                {
                    Server.clients[i].tcp.SendData(packet);
                }
                
            }
        }

        public static void Welcome(int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                Server.StartPingTest();
                SendTCPData(toClient, packet);
            }
        }

        public static void Ping(int toClient,long ping)
        {
            //Send ping to Client
            using (Packet packet = new Packet((int)ServerPackets.ping))
            {
                packet.Write(ping);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }

        public static void SendHexGrid(HexGrid grid, int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.sendHexGrid))
            {     
                packet.Write(grid.chunkCountX);
                packet.Write(grid.chunkCountZ);
                packet.Write(grid.cells);

                foreach (HexCell cell in grid.cells)
                {
                    packet.Write(cell.Structure);
                }
                SendTCPData(toClient, packet);
            }   
        }   
    
        public static void SendStructure(HexCoordinates coordinates, Structure structure)
        {
            using (Packet packet = new Packet((int)ServerPackets.sendStructure))
            {
                packet.Write(coordinates);
                packet.Write(structure);
                SendTCPDataToAll(packet);
            }
        }

        public static void SendGameTick()
        {
            using (Packet packet = new Packet((int)ServerPackets.sendGameTick))
            {
                SendTCPDataToAll(packet);
            }
        }
        
        public static void SendUpgradeBuilding(HexCoordinates coordinates)
        {
            using (Packet packet = new Packet((int)ServerPackets.sendUpgradeBuilding))
            {
                packet.Write(coordinates);
                SendTCPDataToAll(packet);
            }
        }
    }
}
