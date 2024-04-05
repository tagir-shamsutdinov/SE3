using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    // Путь к папке с данными на сервере
    static string dataFolderPath = @"/Users/a1/RiderProjects/SE3_Server/SE3_Server";

    // Переменная для присвоения уникальных идентификаторов файлам
    static int nextFileId = 1;

    // Объект-блокировка для обеспечения синхронизации доступа к общим данным
    static object fileIdLock = new object();

    // Словарь для хранения соответствия идентификатора файла и его пути на сервере
    static Dictionary<int, string> fileIdToPathMap = new Dictionary<int, string>();

    // Переменная для контроля завершения работы сервера
    public static int err = 1;

    static void Main(string[] args)
    {
        Console.WriteLine("Server started!");

        // Инициализация TcpListener для прослушивания подключений
        TcpListener server = null;

        try
        {
            Int32 port = 8888;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            
            server = new TcpListener(localAddr, port);

            server.Start();

            while (err == 1)
            {
                // Ожидание подключения клиента
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Waiting for a connection...");
                
                Console.WriteLine("Connected!");

                // Создание нового потока для обработки запроса клиента
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(client);
            }
        }
        catch (SocketException e)
        {
            // Вывод сообщения об ошибке сокета
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            server.Stop();
        }
    }

    // Метод обработки запроса клиента
    static void HandleClient(object obj)
    {
        // Извлечение клиентского сокета из объекта
        TcpClient client = (TcpClient)obj;

        // Получение сетевого потока для обмена данными с клиентом
        NetworkStream stream = client.GetStream();

        StreamReader reader = new StreamReader(stream);

        StreamWriter writer = new StreamWriter(stream);

        try
        {
            string request = reader.ReadLine();

            if (request!= null)
            {
                string[] requestData = request.Split(' ');

                if (requestData[0] == "PUT")
                {
                    string filename = requestData[1];
                    string fileContent = requestData[2];

                    int fileId = SaveFile(filename, fileContent);
                    if (fileId!= -1)
                    {
                        writer.WriteLine("Response says that file is saved! ID = " + fileId);
                    }
                    else
                    {
                        writer.WriteLine("403");
                    }
                }
                else if (requestData[0] == "GET")
                {
                    int fileId = int.Parse(requestData[1]);
                    if (fileIdToPathMap.ContainsKey(fileId))
                    {
                        string filePath = fileIdToPathMap[fileId];
                        if (File.Exists(filePath))
                        {
                            string fileContent = File.ReadAllText(filePath);
                            writer.WriteLine("The file was downloaded! Specify a name for it:" + fileContent);
                        }
                        else
                        {
                            writer.WriteLine("404");
                        }
                    }
                    else
                    {
                        writer.WriteLine("404");
                    }
                }
                else if (requestData[0] == "GET1")
                {
                    string filename = requestData[1];
                    string filePath = FindFileByName(filename);
                    if (filePath!= null)
                    {
                        string fileContent = File.ReadAllText(filePath);
                        writer.WriteLine("The file was downloaded! Specify a name for it:" + fileContent);
                    }
                    else
                    {
                        writer.WriteLine("404");
                    }
                }
                else if (requestData[0] == "DELETE")
                {
                    int fileId = int.Parse(requestData[1]);
                    if (DeleteFileById(fileId))//функ внизу
                    {
                        writer.WriteLine("The response says that this file was deleted successfully!"); // Возвращаем успешный статус удаления
                    }
                    else
                    {
                        writer.WriteLine("404"); // Возвращаем статус, указывающий на то, что файл не найден или удаление не удалось
                    }
               }
                else if (requestData[0] == "DELETE1")
                {
                    string filename = requestData[1];
                    string filePath = Path.Combine(dataFolderPath, filename);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        writer.WriteLine("The response says that this file was deleted successfully!");
                    }
                    else
                    {
                        writer.WriteLine("404");
                    }
                }
                else if (requestData[0] == "1")
                {
                    Console.WriteLine("Server is shutting down...");
                    err = 0;
                    return;
                }
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("IOException: {0}", e);
        }
        finally
        {
            writer.Close();
            reader.Close();
            stream.Close();
            client.Close();
        }
    }

    static string FindFileByName(string filename)
    {
        string filePath = Path.Combine(dataFolderPath, filename);
        if (File.Exists(filePath))
        {
            return filePath;
        }
        else
        {
            return null;
        }
    }

    static int SaveFile(string filename, string fileContent)
    {
        lock (fileIdLock)
        {
            string filePath = Path.Combine(dataFolderPath, filename);
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, fileContent);
                int fileId = nextFileId++;
                fileIdToPathMap[fileId] = filePath;
                return fileId;
            }
        }
        return -1;
    }

    static bool DeleteFileById(int fileId)
    {
        lock (fileIdLock)
        {
            if (fileIdToPathMap.ContainsKey(fileId))
            {
                string filePath = fileIdToPathMap[fileId];
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    fileIdToPathMap.Remove(fileId);
                    return true;
                }
            }
        }
        return false;
    }
}