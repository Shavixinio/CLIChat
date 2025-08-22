using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

class Client
{
    private const int port = 5000;
    static async Task Main()
    {
        try
        {
            Console.Write("Enter your username: ");
            string username = Console.ReadLine() ?? string.Empty;
            if (username == string.Empty)
                username = "User";

            using TcpClient client = new TcpClient("127.0.0.1", port);
            Console.WriteLine("Connected to the server");

            NetworkStream stream = client.GetStream();
            await MessageUtils.SendMessageAsync(stream, username);

            var receiveTask = Task.Run(async () =>
            {
                while (true)
                {
                    string? serverMessage = await MessageUtils.ReadMessageAsync(stream);
                    if (serverMessage == null)
                    {
                        Console.WriteLine("Server disconnected");
                        break;
                    }
                    Console.WriteLine(serverMessage);
                }
            });

            while (true)
            {
                string message = Console.ReadLine() ?? string.Empty;
                if (message != string.Empty)
                await MessageUtils.SendMessageAsync(stream, message);
            }
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
        {
            Console.WriteLine("The host server is not active or has refused your connection");
        }
        catch (Exception ex)
        {
            Console.Write("Failed to connect to the server: {0}", ex);
        }
    }
    // TODO: Implement 
    public async Task<long> PingServer(NetworkStream stream)
    {
        byte[] buffer = new byte[64];

        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(i % 256);
        }
        var stopwatch = Stopwatch.StartNew();

        await stream.WriteAsync(buffer, 0, buffer.Length);

        byte[] response = new byte[4];
        int read = await stream.ReadAsync(response, 0, response.Length);

        stopwatch.Stop();

        if (read == 64 && response.SequenceEqual(buffer))
            return stopwatch.ElapsedMilliseconds;
        else
        {
            throw new Exception("The server has closed. Ending program");
        }
    }
}