using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Structures;
using Shared.Game;

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
        public static void InitGameLogic(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.initGameLogic))
            {
                packet.Write(GameLogic.grid);

                Player ownPlayer = Server.clients[toClient].Player;
                packet.Write(ownPlayer.Name);
                if (ownPlayer.Tribe == null)
                {
                    packet.Write(255);
                }
                else
                {
                    packet.Write(ownPlayer.Tribe.Id);
                }
                packet.Write(ownPlayer.Position);
                packet.Write(ownPlayer.TroopInventory);

                packet.Write(GameLogic.Players.Count);
                foreach (Player player in GameLogic.Players)
                {
                    packet.Write(player.Name);
                    Tribe tribe = player.Tribe;
                    if (tribe == null)
                        packet.Write(255);
                    else
                        packet.Write(player.Tribe.Id);
                    packet.Write(player.Position);
                    packet.Write(player.TroopInventory);
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
            using (Packet packet = new Packet((int)ServerPackets.gameTick))
            {
                SendTCPDataToAll(packet);
            }
        }
        
        public static void BroadcastUpgradeBuilding(HexCoordinates coordinates)
        {
            using (Packet packet = new Packet((int)ServerPackets.upgradeBuilding))
            {
                packet.Write(coordinates);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastApplyBuild(HexCoordinates coords, Type buildingType, byte tribeID)
        {
            using (Packet packet = new Packet((int)ServerPackets.applyBuild))
            {
                packet.Write(coords);
                packet.Write(buildingType);
                packet.Write(tribeID);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastApplyBuildHQ(HexCoordinates coords)
        {
            using (Packet packet = new Packet((int)ServerPackets.applyBuildHQ))
            {
                packet.Write(coords);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastPlayer(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastPlayer))
            {
                packet.Write(player.Name);
                if (player.Tribe == null)
                {
                    packet.Write((byte)255);
                }
                else 
                {
                    packet.Write(player.Tribe.Id);
                }
                packet.Write(player.Position);
                packet.Write(player.TroopInventory);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastTribe(Tribe tribe)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastTribe))
            {
                packet.Write(tribe.Id);
                packet.Write(tribe.HQ.Cell.coordinates);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastMoveTroops(Player player, HexCoordinates coordinates, TroopType type, int amount)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastMoveTroops))
            {
                packet.Write(player.Name);
                packet.Write(coordinates);
                packet.Write((byte)type);
                packet.Write(amount);
            }
        }

        public static void BroadcastFight(Player player, HexCoordinates coordinates)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastFight))
            {
                packet.Write(player.Name);
                packet.Write(coordinates);
            }
        }
    }
}
