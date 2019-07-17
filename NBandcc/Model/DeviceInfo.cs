using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBandcc
{
    public class DeviceInfo
    {
        public int ID { get; set; }
        public string DateTime { get; set; }
        public string AID { get; set; }
        public string MAC { get; set; }
        public string AppVersion { get; set; }
        public bool IsOnline { get; set; }
    }
}
