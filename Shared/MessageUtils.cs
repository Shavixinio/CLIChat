using System.Net.Sockets;
using System.Text;

public static class MessageUtils
{
    public static async Task SendMessageAsync(NetworkStream stream, string message)
    {
        byte[] payload = Encoding.UTF8.GetBytes(message);
        byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);

        if (!BitConverter.IsLittleEndian)
            Array.Reverse(lengthPrefix);

        await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
        await stream.WriteAsync(payload, 0, payload.Length);
    }

    public static async Task<string?> ReadMessageAsync(NetworkStream stream)
    {
        byte[] lengthPrefix = new byte[4];
        int bytesRead = await stream.ReadAsync(lengthPrefix, 0, 4);
        if (bytesRead == 0)
            return null;

        if (!BitConverter.IsLittleEndian)
            Array.Reverse(lengthPrefix);

        int messageLength = BitConverter.ToInt32(lengthPrefix, 0);

        byte[] payload = new byte[messageLength];
        int offset = 0;
        while (offset < messageLength)
        {
            int chunk = await stream.ReadAsync(payload, offset, messageLength - offset);
            if (chunk == 0)
                throw new IOException("Connection closed unexpectedly");
            offset += chunk;
        }

        return Encoding.UTF8.GetString(payload);
    }
}
