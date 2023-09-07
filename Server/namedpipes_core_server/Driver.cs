using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace namedpipes_core_server
{
    public class Driver
    {
        static void Main()
        {
            NamedPipeServer server = NamedPipeServer.getNamedPipeServer(new RequestHandler());
            server.StartServer();
            Console.WriteLine("Server started, Moving on!!");
            //StartServerMessageMode();
        }
    }
}
