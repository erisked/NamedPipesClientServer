using namedpipes_core_client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace namedpipes_core_server
{
    public class RequestHandler : IRequestHandler
    {
        public Response ProcessRequest(Request r)
        {
            string s = "dummy";
            ulong id = 0;
            if (r != null)
            { 
                s += r.Id.ToString();
                s += "PROCESSED";
                s += r.Command;
                id = r.Id;
            }
            return new Response(id, s);
        }
    }
}
