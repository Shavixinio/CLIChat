using System.Net.Sockets;

class ClientInfo
{
    public required TcpClient Client { get; set; }
    public required string Username { get; set; }
}