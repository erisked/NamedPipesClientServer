using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace namedpipes_core_client
{

    [Serializable]
    class Request
    {
        public ulong Id { get; set; }
        public string Command { get; set; }
        public Request(ulong id, string command)
        {
            Id = id;
            Command = command;
        }
        public string getSerialized()
        {
            string json = JsonSerializer.Serialize(this);
            return json;
        }

        public static Request getRequest(string s)
        {
            Request r = null;
            try 
            {
                r = JsonSerializer.Deserialize<Request>(s);
            }
            catch (Exception e)
            {
                Console.WriteLine("Serialization failed");
                Console.WriteLine(e);
            }
            return r;
        }
    }
}
