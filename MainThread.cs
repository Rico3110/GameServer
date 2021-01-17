using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class MainThread
    {
        static int counter = 0;
        public static void Update()
        {
            ThreadManager.UpdateMain();
            counter++;
            if(counter > 1000)
            {

            }
        }
    }
}
