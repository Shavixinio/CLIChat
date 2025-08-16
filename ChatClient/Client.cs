using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

class Client
{
    private const int port = 5000;
    static void Main()
    {
        try
        {
            Console.Write("Enter your username: ");
            string username = Console.ReadLine() ?? "User";
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);

            using TcpClient client = new TcpClient("127.0.0.1", port);
            Console.WriteLine("Connected to the server");

            NetworkStream stream = client.GetStream();
            stream.Write(usernameBytes);

            // var clientInstance = new Client();
            // long ping = clientInstance.PingServer(stream).Result;

            while (true)
            {

                Console.Write("Enter a message: ");
                string message = Console.ReadLine() ?? string.Empty;

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data);

                byte[] response = new byte[256];
                int byteRead = stream.Read(response, 0, response.Length);
                string serverMessage = Encoding.UTF8.GetString(response, 0, byteRead);
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
    public async Task<long> PingServer(NetworkStream stream)
    {
        // Ping payload
        byte[] buffer = new byte[64];

        // Fill the buffer with actual bytes 
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(i % 256);
        }
        var stopwatch = Stopwatch.StartNew();

        await stream.WriteAsync(buffer, 0, buffer.Length);

        byte[] response = new byte[4];
        int read = await stream.ReadAsync(response, 0, response.Length);

        stopwatch.Stop();

        if (read == 4 && response.SequenceEqual(buffer))
            return stopwatch.ElapsedMilliseconds;
        else
        {
            throw new Exception("The server has closed. Ending program");
        }
    }
}