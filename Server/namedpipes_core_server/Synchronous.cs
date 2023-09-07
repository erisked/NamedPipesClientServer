using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace namedpipes_core_server
{
    class Synchronous
    {
        private const string PipeName = "MyNamedPipe";
        private static readonly object lockObject = new object();
        private static NamedPipeServerStream serverStream = null;
        private static StreamReader reader = null;
        private static StreamWriter writer = null;

        static void _Main()
        {
            Task t = Task.Run(() => ServerThreadMethod());
            Console.WriteLine("Server is listening for connections...");

            t.Wait();

            Console.ReadLine();
        }

        static void ServerThreadMethod()
        {
            serverStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut);
            reader = new StreamReader(serverStream);
            writer = new StreamWriter(serverStream);
            serverStream.WaitForConnection();
            Console.WriteLine("Waiting for a client connection...");

            while (true)
            {
                // Start a new thread to handle each client request
                ClientHandlerThreadMethod();
            }
        }

        static void ClientHandlerThreadMethod()
        {
            try
            {
                lock (lockObject) // Lock the critical section
                {
                    string message = reader.ReadLine();
                    Console.WriteLine($"Received message from client: {message}");
                    // Process the message if needed
                    string responseMessage = $"Hello from server! You said: {message}";
                    writer.WriteLine(responseMessage);
                    writer.Flush();
                }
            }
            catch (IOException ex) when (ex.InnerException is IOException innerException && innerException.HResult == -2146232800)
            {
                Console.WriteLine("Client disconnected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}