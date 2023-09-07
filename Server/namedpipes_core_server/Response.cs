using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace namedpipes_core_client
{

    [Serializable]
    public class Response
    {
        public ulong Id { get; set; }
        public string ResponseData { get; set; }
        public Response(ulong id, string responseData)
        {
            Id = id;
            ResponseData = responseData;
        }
        public static string getSerialized(Response r)
        {
            string json = JsonSerializer.Serialize(r);
            return json;
        }

        public static Response getResponse(string s)
        {
            Response r = null;
            try
            {
                r = JsonSerializer.Deserialize<Response>(s);
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
