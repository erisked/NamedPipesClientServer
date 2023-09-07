using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace namedpipes_core_client
{
    class Driver
    {
        public static Dictionary<ulong, ulong> responseTimeMap = new Dictionary<ulong, ulong>();

        static void Main()
        {
            //StartClientAsync().Wait();
            //NamedPipesHelper commHelper = new NamedPipesHelper();
            // Send the first request
            List<Request> requests = new List<Request>();
            string filePath = "C:\\Users\\rikediya\\source\\repos\\namedpipes_core_client\\namedpipes_core_client\\data.txt";
            string fileContent = File.ReadAllText(filePath);
            for (int i = 0; i < 20; i++)
            {
                requests.Add(new Request(NamedPipesHelper.getUniqueId(), fileContent));
            }

            Console.WriteLine(DateTime.Now);
            List<Response> responses = new List<Response>();
            string str;
            foreach (Request r in requests)
            {
                Thread thread = new Thread(() => SendCommand(r));
                thread.Start();
            }

            foreach (ulong key in responseTimeMap.Keys)
            {
                Console.WriteLine("Time taken for " + key + " = " + responseTimeMap[key]);
            }

            Console.ReadLine();
        }

        public static Response SendCommand(Request r)
        {
            Console.WriteLine("Continue with other tasks or send additional requests");
            // Wait for the response for the first request
            NamedPipesHelper helper = NamedPipesHelper.getPipeHelper();
            Response response = helper.SendRequestGetResponse(r);
            //ulong tiksNow = helper.getId();
            //responseTimeMap[response.Id] = tiksNow - responseTimeMap[response.Id];
            Console.WriteLine($"Response recieved" + response.ResponseData.Length);
            return response;
        }
    }
}
