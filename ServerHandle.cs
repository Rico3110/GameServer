using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static void SendBuildingData(int fromClient, Packet packet)
        {
            HexCoordinates coordinates = packet.ReadHexCoordinates();
        }

        public static void TryBuildBuilding(int fromClient, Packet packet)
        {
            BuildingData buildingData = packet.ReadBuildingData();
            if (Server.gameLogic.verifyBuild(buildingData))
            {
                HexCell newCell = Server.gameLogic.applyBuild(buildingData);
                ServerSend.SendTCPDataToAll(ServerSend.createBuildingDataPacket(newCell.Building));
            }
        }
    }
}
