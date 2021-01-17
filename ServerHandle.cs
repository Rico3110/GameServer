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

        public static void RequestHexGrid(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }
            Console.WriteLine("fdsajkllhgdsj");

            using (Packet newPacket = ServerSend.HexGrid(GameLogic.grid))
            {
                ServerSend.SendTCPData(fromClient, newPacket);
            }                     
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

            if(GameLogic.verifyBuild(coordinates, structure))
            {
                GameLogic.applyBuild(coordinates, structure);
                using (Packet newPacket = ServerSend.SendStructure(coordinates, structure))
                {
                    ServerSend.SendTCPData(fromClient, newPacket);
                }                  
            }
        }
    }
}
