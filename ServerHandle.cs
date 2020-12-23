using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Communication;
using Shared.HexGrid;
using Shared.DataTypes;

namespace GameServer
{
    class ServerHandle
    {
        public static void WelcomeRecieved(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();
            string username = packet.ReadString();

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}");

            if(fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIDCheck})!");
            }
            //TODO send player into game

            //Stop Timer
            Server.StopPingTest(fromClient);
        }

        public static void RequestBuildingData(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();
            HexCoordinates coordinates = packet.ReadHexCoordinates();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            HexCell cell = Server.gameLogic.grid.GetCell(coordinates);
            using (Packet newPacket = ServerSend.createBuildingDataPacket(coordinates, cell.Building))
            {
                ServerSend.SendTCPData(fromClient, newPacket);
            }

        }

        public static void TryBuildBuilding(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();
            HexCoordinates coords = packet.ReadHexCoordinates();
            BuildingData buildingData = packet.ReadBuildingData();
            
            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }
            
            if (Server.gameLogic.verifyBuild(coords, buildingData))
            {
                HexCell newCell = Server.gameLogic.applyBuild(coords, buildingData);
                using (Packet newPacket = ServerSend.createBuildingDataPacket(coords, newCell.Building))
                {
                    ServerSend.SendTCPDataToAll(newPacket);

                }
            }
        }

        public static void RequestAllMapData(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            using (Packet newPacket = ServerSend.createHexGridPacket(Server.gameLogic.grid))
            {
                ServerSend.SendTCPData(fromClient, newPacket);
            }
        }
    }
}
