using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    static class PacketBuffer
    {
        public static int head = -1;
        public const int size = 50;

        public static byte[][] buffer = new byte[50][];

        public static void Add(byte[] packet)
        {
            head = (head + 1) % size;
            buffer[head] = packet;
        }

        public static List<byte[]> GetLostPackets(int id)
        {
            List<byte[]> lostPackets = new List<byte[]>();
            
            while(id <= head){
                id = (id + 1) % size;
                lostPackets.Add(buffer[id]);
            }

            return lostPackets;
        }
    }
}
