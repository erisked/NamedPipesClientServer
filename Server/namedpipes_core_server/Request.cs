using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace namedpipes_core_client
{

    [Serializable]
    public class Request
    {
        public ulong Id { get; set; }
        public string Command { get; set; }
        public Request(ulong id, string command)
        {
            Id = id;
            Command = command;
        }
        public static string getSerialized(Request r)
        {
            string json = JsonSerializer.Serialize(r);
            return json;
        }

        public static Request getRequest(string s)
        {
            Request r = null;
            if (s != null)
            {
                try
                {
                    r = JsonSerializer.Deserialize<Request>(RemoveBOMFromJson(s));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Serialization failed");
                    Console.WriteLine(e);
                }
            }
            return r;
        }
        private static string RemoveBOMFromJson(string jsonWithBOM)
        {
            const string BOM = "\uFEFF";
            return jsonWithBOM.TrimStart(BOM.ToCharArray());
        }
    }
}
