using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRWorker
{
    internal class Request
    {
        public required string Method { get; set; }
        public string[] Args { get; set; } = [];
    }
}
