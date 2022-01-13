public static class ToServer
{
    public delegate void Send (byte[] message);
    public static Send sendMessage;

    public static void SendToServer(Header header, params int[] message)
    {
        sendMessage(MessageManager.MakeByteMessage(header, message));
    }

    public static void SendToServer(Header header, string message)
    {
        sendMessage(MessageManager.MakeByteMessage(header, message));
    }
}
