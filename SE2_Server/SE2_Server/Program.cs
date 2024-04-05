using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

class FileServer
{
    static readonly string DATA_DIRECTORY = @"/Users/a1/RiderProjects/SE2_Server/SE2_Server";
    static readonly int PORT = 8888;

    static void Main(string[] args)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, PORT);
        listener.Start();
        Console.WriteLine("Server started!");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected!");

            HandleClientRequest(client);

            client.Close();
        }
    }

    static void HandleClientRequest(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);

            string request = reader.ReadLine();
            if (request != null)
            {
                string[] parts = request.Split(' ');
                string action = parts[0];
                string filename = parts[1];

                switch (action)
                {
                    case "1":
                        HandleGetRequest(filename, writer);
                        break;
                    case "2":
                        HandlePutRequest(filename, string.Join(" ", parts, 2, parts.Length - 2), writer);
                        break;
                    case "3":
                        HandleDeleteRequest(filename, writer);
                        break;
                    case "exit":
                        Environment.Exit(0);
                        break;
                    default:
                        writer.WriteLine("Invalid action!");
                        writer.Flush();
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error occurred: " + e.Message);
        }
    }

    static void HandleGetRequest(string filename, StreamWriter writer)
    {
        try
        {
            string filePath = Path.Combine(DATA_DIRECTORY, filename);
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                writer.WriteLine($"The content of the file is: {content}");
                writer.Flush();
            }
            else
            {
                writer.WriteLine("The response says that the file was not found!");
                writer.Flush();
            }
        }
        catch (Exception e)
        {
            writer.WriteLine($"Error occurred: {e.Message}");
            writer.Flush();
        }
    }

    static void HandlePutRequest(string filename, string content, StreamWriter writer)
    {
        try
        {
            string filePath = Path.Combine(DATA_DIRECTORY, filename);
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, content);
                writer.WriteLine("The response says that the file was created!");
                writer.Flush();
            }
            else
            {
                writer.WriteLine("The response says that creating the file was forbidden!");
                writer.Flush();
            }
        }
        catch (Exception e)
        {
            writer.WriteLine($"Error occurred: {e.Message}");
            writer.Flush();
        }
    }
    static void HandleDeleteRequest(string filename, StreamWriter writer)
    {
        try
        {
            string filePath = Path.Combine(DATA_DIRECTORY, filename);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                writer.WriteLine("The response says that the file was successfully deleted!");
                writer.Flush();
            }
            else
            {
                writer.WriteLine("The response says that the file was not found!");
                writer.Flush();
            }
        }
        catch (Exception e)
        {
            writer.WriteLine($"Error occurred: {e.Message}");
            writer.Flush();
        }
    }
}