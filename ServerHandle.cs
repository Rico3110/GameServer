using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Communication;
using Shared.HexGrid;
using Shared.DataTypes;
using Shared.Structures;
using Shared.Game;

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

        public static void HandleMapRequest(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }
            Console.WriteLine("fdsajkllhgdsj");

            ServerSend.SendHexGrid(GameLogic.grid, fromClient);                  
        }

        public static void HandlePlaceBuilding(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            HexCoordinates coordinates = packet.ReadHexCoordinates();
            Type type = packet.ReadType();
            Structure structure = (Structure)Activator.CreateInstance(type);

            if (structure is Building)
                ((Building)structure).Tribe = (byte)fromClient;

            if(GameLogic.verifyBuild(coordinates, structure))
            {
                GameLogic.applyBuild(coordinates, structure);
                ServerSend.SendStructure(coordinates, structure);             
            }
        }

        public static void HandleUpgradeBuilding(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            HexCoordinates coordinates = packet.ReadHexCoordinates();
            HexCell cell = GameLogic.grid.GetCell(coordinates);
            if(cell != null && cell.Structure is Building)
            {
                GameLogic.applyUpgrade(coordinates);
                ServerSend.SendUpgradeBuilding(coordinates);
            }
        }
    }
}
