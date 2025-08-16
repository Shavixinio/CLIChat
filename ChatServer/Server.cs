using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

class Server
{
    private static readonly List<ClientInfo> clients = new();
    private static readonly object clientsLock = new();
    static async Task Main()
    {
        TcpListener? server = null;
        try
        {
            // Define the server
            int port = 5000;
            IPAddress serverAddress = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(serverAddress, port);

            server.Start();
            Console.WriteLine("Server started on {0}:{1}", serverAddress, port);

            var client = await server.AcceptTcpClientAsync();
            Console.WriteLine("A client has connected to the server");

            HandleClientAsync(client);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Server exception: {0}", ex);
        }
        finally
        {
            server?.Stop();
        }
    }

    private static async void HandleClientAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[256];

        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        string username = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"{username} has joined the chat");

        clients.Add(new ClientInfo
        {
            Client = client,
            Username = username
        });
        try
            {
                // Process incoming messages
                int count;
                while ((count = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string data = Encoding.UTF8.GetString(buffer, 0, count);
                    Console.WriteLine($"Received from {username}: {data}");

                    string messageToBroadcast = $"{username}: {data}";
                    byte[] msgBytes = Encoding.UTF8.GetBytes(messageToBroadcast);

                    // Send the message to all clients
                    foreach (var c in clients.ToList())
                    {
                        try
                        {
                            NetworkStream clientStream = c.Client.GetStream();
                            await clientStream.WriteAsync(msgBytes, 0, msgBytes.Length);
                        }
                        catch
                        {
                            Console.WriteLine($"Removing disconnected client: {c.Username}");
                            clients.Remove(c);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while handling {username}: {e.Message}");
            }
            finally
            {
                Console.WriteLine($"{username} disconnected");
                client.Close();
            }
    }
}