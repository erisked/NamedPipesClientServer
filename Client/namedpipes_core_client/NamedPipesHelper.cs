using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace namedpipes_core_client
{
    class NamedPipesHelper
    {
        static NamedPipesHelper helper;
        private NamedPipeClientStream clientStream;
        public static Dictionary<ulong, ulong> responseTimeMap = new Dictionary<ulong, ulong>();
        public static Dictionary<ulong, Response> responseCache = new Dictionary<ulong, Response>();

        private static object counterLock = new object();
        private static object writeLock = new object();

        // Creating the singleton of the NamedPipeHelper instance.
        public static NamedPipesHelper getPipeHelper()
        {
            lock (counterLock)
            {
                if (helper == null)
                {
                    helper = new NamedPipesHelper();
                }
            }
            return helper;
        }
        private NamedPipesHelper()
        {
            if (clientStream == null)
            {
                clientStream = new NamedPipeClientStream(".", "MyNamedPipe", PipeDirection.InOut, PipeOptions.Asynchronous);
            }
            // start read response Thread.
            Thread thread = new Thread(ReadResponses);
            thread.Start();

            try
            {
                Console.WriteLine("Connecting to the server <byte mode>...");
                Task init = clientStream.ConnectAsync();
                init.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void SendRequestByteModeAsync(Request request)
        {
            if (ClientStreamInitialized())
            {
                // Data to be sent as bytes
                string serializedRequest = request.getSerialized();
                byte[] messageBytes = Encoding.UTF8.GetBytes(serializedRequest);
                responseTimeMap[request.Id] = getId();
                // Write the message bytes to the named pipe asynchronously
                lock (writeLock)
                {
                    clientStream.Write(messageBytes, 0, messageBytes.Length);
                    clientStream.Flush();
                }
            }
        }

        private async Task<string> ReadResponseByteModeAsync()
        {
            string message = "";
            if (ClientStreamInitialized())
            {
                // Read the data from the named pipe as bytes asynchronously
                byte[] buffer = new byte[2 * 1024 * 1024];
                int bytesRead = await clientStream.ReadAsync(buffer, 0, buffer.Length);

                // Convert the received bytes to a string
                message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            return message;
        }
        private bool ClientStreamInitialized()
        {
            if (clientStream != null)
            {
                return clientStream.IsConnected;
            }
            return false;
        }

        public static ulong getId()
        {
            DateTime currentTime = DateTime.Now;

            // Convert time to ulong
            return (ulong)currentTime.Ticks;
        }

        public static ulong getUniqueId()
        {
            ulong id = getId();
            while (responseTimeMap.ContainsKey(id))
            {
                id = getId();
            }
            responseTimeMap.Add(id, 0);
            return id;
        }

        public Response SendRequestGetResponse(Request request)
        {
            SendRequestByteModeAsync(request);
            while(!responseCache.ContainsKey(request.Id))
            {
                Thread.Sleep(100); //As a heuristic, putting a sleep of 100 ms
            }
            Response response = responseCache[request.Id];
            responseCache.Remove(request.Id);
            return response;
        }
        private void ReadResponses()
        {
            while(true)
            {
                Task<string> readResponse = ReadResponseByteModeAsync();
                readResponse.Wait();
                string str = readResponse.Result;
                Response response = Response.getResponse(str);
                if (response != null)
                {
                    AddToCache(response);
                }
            }
        }
        private void AddToCache(Response data)
        {
            if (responseCache.ContainsKey(data.Id))
            {
                responseCache[data.Id] = data;
            }
            else
            {
                responseCache.Add(data.Id, data);
            }
        }
    }
}