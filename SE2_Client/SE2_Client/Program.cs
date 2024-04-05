using System;
using System.IO;
using System.Net.Sockets;

class FileClient
{
    static readonly string SERVER_ADDRESS = "127.0.0.1";
    static readonly int PORT = 8888;

    static void Main()
    {
        try
        {
            TcpClient client = new TcpClient(SERVER_ADDRESS, PORT);
            Console.WriteLine("Connected to server!");

            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);
            
                Console.WriteLine("Enter action (1 - get a file, 2 - create a file, 3 - delete a file, exit - to quit): ");
                string action = Console.ReadLine();

                if (!int.TryParse(action, out int actionNumber)  || actionNumber < 1  || actionNumber > 3)
                {
                    Console.WriteLine("Invalid action!");
                }

                Console.WriteLine("Enter filename: ");
                string filename = Console.ReadLine();

                string content = "";

                if (actionNumber == 2)
                {
                    Console.WriteLine("Enter file content: ");
                    content = Console.ReadLine();
                }

                writer.WriteLine($"{actionNumber} {filename} {content}");
                writer.Flush();

                string response = reader.ReadLine();
                if (response != null)
                {
                    Console.WriteLine(response);
                }

            client.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}