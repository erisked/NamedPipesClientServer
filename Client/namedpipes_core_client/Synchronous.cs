using System;
using System.IO;
using System.IO.Pipes;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace namedpipes_core_client
{
    class Synchronous
    {
        /*static void Main()
        {
            using (NamedPipeClientStream clientStream = new NamedPipeClientStream(".", "MyNamedPipe", PipeDirection.InOut))
            {
                Console.WriteLine("Connecting to server...");
                clientStream.Connect();

                using (StreamReader reader = new StreamReader(clientStream))
                using (StreamWriter writer = new StreamWriter(clientStream))
                {
                    string messageToSend = "Hello from client!";
                    writer.WriteLine(messageToSend);
                    writer.Flush();

                    string responseMessage = reader.ReadLine();
                    Console.WriteLine($"Received response: {responseMessage}");
                }
            }
        }*/
        private const string PipeName = "MyNamedPipe";
        private static readonly object lockObject = new object();
        private static object counterLock = new object();

        private static NamedPipeClientStream clientStream = null;
        private static StreamReader reader = null;
        private static StreamWriter writer = null;
        private static int counter = 0;


        static void _Main()
        {
            int numThreads = 20; // Number of client threads to create

            clientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
            reader = new StreamReader(clientStream);
            writer = new StreamWriter(clientStream);
            // Create an array to store the Task objects representing each thread
            Task[] tasks = new Task[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                // Create and start client threads
                tasks[i] = Task.Run(() => ClientThreadMethod(getID()));
            }

            // Wait for all threads to complete
            Task.WaitAll(tasks);

            clientStream.Close();
            Console.ReadLine();
        }

        static int getID()
        {
            int a = 0;
            lock(counterLock)
            {
                a = counter++;
            }
            return a;
        }
        static void ClientThreadMethod(object threadId)
        {
            int clientId = (int)threadId;


                lock (lockObject) // Lock the critical section
                {
                    try
                    {
                        Console.WriteLine($"Client {clientId} Connecting to server...");
                        if(!clientStream.IsConnected)
                            clientStream.Connect();

                        // Simulate client sending data to the server
                        string messageToSend = $"Hello from client {clientId}!";
                        writer.WriteLine(messageToSend);
                        writer.Flush();
                    
                        // Simulate client receiving response from the server
                        string responseMessage = reader.ReadLine();
                        Console.WriteLine($"Client {clientId} Received response: {responseMessage}");
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Client {clientId} Error: {ex.Message}");
                    }
                }
            }
        }
    }
