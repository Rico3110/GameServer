using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.GameState;
using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;

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

        public static void TestArray(int toClient)
        {
            //Creating test Array
            uint[] test = new uint[10000];
            for(int i = 0;i < test.Length; i++)
            {
                test[i] = (uint)i * 9; 
            }

            //Send Uint Array to Client
            using (Packet packet = new Packet((int)ServerPackets.testArray))
            {
                packet.Write(test);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }

        /// <summary>
        /// Sends the given mapdata as an uint array to the choosen client
        /// </summary>
        /// <param name="toClient">The client, who gets send the data</param>
        /// <param name="data">The hexmap data as an uint array</param>
        public static void SendHexData(int toClient,uint[] data)
        {
            //Send Uint Array to Client
            using (Packet packet = new Packet((int)ServerPackets.hexData))
            {
                packet.Write(data);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }

        public static void SendHexMap(int toClient, HexMap hexMap)
        {
            using (Packet packet = new Packet((int)ServerPackets.hexMap))
            {
                packet.Write(hexMap.data);
                packet.Write(hexMap.chunkCountX);
                packet.Write(hexMap.chunkCountZ);

                packet.Write(hexMap.lat);
                packet.Write(hexMap.lon);

                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }

        public static Packet createBuildingDataPacket(HexCoordinates coords, BuildingData buildingData)
        {
            Packet packet = new Packet((int)ServerPackets.sendBuildingData);
            packet.Write(coords);
            packet.Write(buildingData);
            return packet;
        }

        public static Packet createHexGridPacket(HexGrid grid)
        {
            Packet packet = new Packet((int)ServerPackets.sendHexGrid);
            packet.Write(grid);
            return packet;
        }
    }
}
