using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shabalin.Proxy
{
    class Settings
    {
        public int localPort = 0;
        public int serverPort = 0;
        public UInt32 IPv4 = 0;
        public string IPv6;
        public int delay = 0;

        public static Settings ParseArgs(string[] args)
        {
            Settings settings = new Settings();

            return new Settings();
        }
    }
}
