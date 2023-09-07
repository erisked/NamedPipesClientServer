using namedpipes_core_client;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace namedpipes_core_server
{
    class NamedPipeServer
    {
        private static int requestIDCounter = 0;
        private static object requestIDLock = new object();
        private static object initializationLock = new object();
        private IRequestHandler requesthandler;
        private static NamedPipeServer server;

        public static NamedPipeServer getNamedPipeServer(IRequestHandler requestHandler)
        {
            lock (initializationLock)
            {
                if (server == null)
                {
                    server = new NamedPipeServer(requestHandler);
                }
            }
            return server;
        }

        private NamedPipeServer(IRequestHandler requestHandler)
        {
            if (requesthandler == null)
            {
                this.requesthandler = requestHandler;
            }
        }
        
        public void StartServer()
        {
            // Start server in a separate thread.
            Thread thread = new Thread(StartServerByteMode);
            thread.Start();
        }

        private void StartServerByteMode()
        {
            using (NamedPipeServerStream serverStream = new NamedPipeServerStream("MyNamedPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                Console.WriteLine("Server is waiting for connections <ByteMode>...");
                Task connectionTask = serverStream.WaitForConnectionAsync();
                connectionTask.Wait();
                Console.WriteLine("Pipe initialized");
                while (true)
                {
                    Task<int> requestIDTask = GetNextRequestIDAsync();
                    Task<string> requestTask = ReadRequestByteModeAsync(serverStream);

                    // Wait for both the request ID and the request message
                    Task.WhenAll(requestIDTask, requestTask).Wait();

                    int requestID = requestIDTask.Result;
                    string request = requestTask.Result;
                    Console.WriteLine("Message recieved - " + requestTask.Result.Length);
                    if (request == "exit")
                    {
                        // Handle exit request (client wants to disconnect)
                        Console.WriteLine($"Client with request ID {requestID} disconnected.");
                        break;
                    }

                    // Process the request and get the response
                    Response response = server.ProcessRequest(request);
                    Console.WriteLine("Sending response " + response.Id + "Len "+response.ResponseData.Length);
                    // Send the response back to the client with the corresponding request ID
                    WriteResponseByteMode(serverStream, requestID, Response.getSerialized(response));
                }

                serverStream.Disconnect();
            }
        }



        private async Task<int> GetNextRequestIDAsync()
        {
            int requestID;
            lock (requestIDLock)
            {
                requestID = requestIDCounter++;
            }
            return await Task.FromResult(requestID);
        }

        private async Task<string> ReadRequestAsync(NamedPipeServerStream pipeStream)
        {
            using (StreamReader reader = new StreamReader(pipeStream, Encoding.UTF8, true, 4096, leaveOpen: true))
            {
                return await reader.ReadLineAsync();
            }
        }

        private async Task<string> ReadRequestByteModeAsync(NamedPipeServerStream pipeStream)
        {
            // Read the data from the named pipe as bytes asynchronously
            byte[] buffer = new byte[2*1024*1024];
            int bytesRead = await pipeStream.ReadAsync(buffer, 0, buffer.Length);

            // Convert the received bytes to a string
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            return message;
        }

        private async Task WriteResponse(NamedPipeServerStream pipeStream, int requestID, string response)
        {
            StreamWriter writer = new StreamWriter(pipeStream, Encoding.UTF8, 4096, leaveOpen: true);
            await writer.WriteLineAsync(response);
            await writer.FlushAsync();
        }

        private async Task WriteResponseByteMode(NamedPipeServerStream pipeStream, int requestID, string response)
        {
            // Data to be sent as bytes
            byte[] messageBytes = Encoding.UTF8.GetBytes(response);

            // Write the message bytes to the named pipe asynchronously
            await pipeStream.WriteAsync(messageBytes, 0, messageBytes.Length);
            await pipeStream.FlushAsync();
        }

        private Response ProcessRequest(string request)
        {
            Request r = Request.getRequest(request);
            // Your logic to process the request and generate the response
            // In this example, we simply echo the request as the response.
            return requesthandler.ProcessRequest(r);
        }
    }

}
