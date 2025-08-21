using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
class Server
{
    private static readonly ConcurrentDictionary<string, ClientInfo> clients = new();
    static async Task Main()
    {
        TcpListener? server = null;
        try
        {
            int port = 5000;
            IPAddress serverAddress = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(serverAddress, port);

            server.Start();
            Console.WriteLine("Server started on {0}:{1}", serverAddress, port);

            while (true)
            {
                var client = await server.AcceptTcpClientAsync();
                Console.WriteLine("A client has connected to the server");

                HandleClientAsync(client);
            }
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
        string? username = await MessageUtils.ReadMessageAsync(stream);
        Console.WriteLine($"{username} has joined the chat");

        var clientInfo = new ClientInfo { Client = client, Username = username ?? "Unknown" };
        clients[clientInfo.Username] = clientInfo;

        try
        {
            while (true)
            {
                string? message = await MessageUtils.ReadMessageAsync(stream);
                if (message == null)
                    break;

                Console.WriteLine($"Received from {clientInfo.Username}: {message}");
                string broadcast = $"{clientInfo.Username}: {message}";

                foreach (var kvp in clients)
                {
                    var otherClient = kvp.Value;
                    try
                    {
                        NetworkStream otherStream = otherClient.Client.GetStream();
                        await MessageUtils.SendMessageAsync(otherStream, broadcast);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Caught exception while handling client: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception: {e.Message}");
        }
        finally
        {
            clients.TryRemove(clientInfo.Username, out _);
            client.Close();
        }
    }
}