using System;
using System.Text;

public static class MessageManager
{
    public static Header GetHeader(byte[] message)
    {
        return (Header)BitConverter.ToInt32(message, 0);
    }

    public static int GetBodyToInt(byte[] message, int startIndex = 4)
    {
        return BitConverter.ToInt32(message, startIndex);
    }

    public static string GetBodyToString(byte[] message, int startIndex = 4)
    {
        string msg = Encoding.UTF8.GetString(message, startIndex, message.Length - startIndex);
        msg = msg.Replace("\0", string.Empty).Trim();

        return msg;
    }

    public static byte[] MakeByteMessage(Header header, string message)
    {
        byte[] newByte = new byte[512];
        byte[] byteHeader = BitConverter.GetBytes((int)header);
        byte[] byteMessage = Encoding.UTF8.GetBytes(message);

        Array.Copy(byteHeader, newByte, byteHeader.Length);
        Array.Copy(byteMessage, 0, newByte, byteHeader.Length, byteMessage.Length);

        return newByte;
    }

    public static byte[] MakeByteMessage(Header header, int index, string message)
    {
        byte[] newByte = new byte[512];
        byte[] byteHeader = BitConverter.GetBytes((int)header);
        byte[] byteNumber = BitConverter.GetBytes(index);
        byte[] byteMessage = Encoding.UTF8.GetBytes(message);

        Array.Copy(byteHeader, newByte, byteHeader.Length);
        Array.Copy(byteNumber, 0, newByte, 4, byteNumber.Length);
        Array.Copy(byteMessage, 0, newByte, 8, byteMessage.Length);

        return newByte;
    }

    public static byte[] MakeByteMessage(Header header, params int []message)
    {
        int startIndex = 4;
        byte[] newByte = new byte[512];
        byte[] byteHeader = BitConverter.GetBytes((int)header);
        Array.Copy(byteHeader, newByte, byteHeader.Length);

        foreach (int msg in message)
        {
            byte[] byteMessage = BitConverter.GetBytes(msg);
            Array.Copy(byteMessage, 0, newByte, startIndex, byteMessage.Length);
            startIndex += 4;
        }

        return newByte;
    }
}