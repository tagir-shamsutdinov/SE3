using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main(string[] args)
    {
        int err = 1;

        while (err == 1)
        {
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 8888);

                NetworkStream stream = client.GetStream();

                StreamReader reader = new StreamReader(stream);

                StreamWriter writer = new StreamWriter(stream);

                Console.WriteLine("Enter action (1 - get a file, 2 - save a file, 3 - delete a file):");
                string action = Console.ReadLine();

                if (action == "exit")
                {
                    writer.WriteLine("1");
                    writer.Flush();
                    err = 0;
                    Console.ReadLine();
                    return;
                }

                if (int.Parse(action) == 1)
                {
                    Console.WriteLine("Do you want to get the file by name or by id (1 - name, 2 - id):");
                    int choice = int.Parse(Console.ReadLine());

                    if (choice == 1)
                    {
                        Console.WriteLine("Enter name:");
                        string name = Console.ReadLine();
                        writer.WriteLine("GET1 " + name);
                    }
                    else if (choice == 2)
                    {
                        Console.WriteLine("Enter id:");
                        int id = int.Parse(Console.ReadLine());
                        writer.WriteLine("GET " + id);
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice!");
                        return;
                }
                }

                else if (int.Parse(action) == 2)
                {
                    Console.WriteLine("Enter name of the file:");
                    string filename = Console.ReadLine();
                    string filePath = Path.Combine(@"/Users/a1/RiderProjects/SE3_Client/SE3_Client", filename);

                    if (File.Exists(filePath))
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        long fileSize = fileInfo.Length;
                        byte[] fileBytes = File.ReadAllBytes(filePath);

                        writer.WriteLine("PUT " + filename + " " + fileSize);
                        writer.Flush();

                        stream.Write(fileBytes, 0, fileBytes.Length);
                    }
                    else
                    {
                        Console.WriteLine("File not found!");
                    }
                }

                else if (int.Parse(action) == 3)
                {
                    Console.WriteLine("Do you want to delete the file by name or by id (1 - name, 2 - id):");
                    int choice = int.Parse(Console.ReadLine());

                    if (choice == 1)
                    {
                        Console.WriteLine("Enter name:");
                        string name = Console.ReadLine();
                        writer.WriteLine("DELETE1 " + name);
                    }
                    else if (choice == 2)
                    {
                        Console.WriteLine("Enter id:");
                        int id = int.Parse(Console.ReadLine());
                        writer.WriteLine("DELETE " + id);
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice!");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid action!");
                    return;
                }

                writer.Flush();

                string response = reader.ReadLine();

                Console.WriteLine("The request was sent.");
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}