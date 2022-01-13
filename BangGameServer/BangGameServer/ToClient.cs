namespace BangGameServer
{
    public static class ToClient
    {
        public delegate void SendToClients(byte[] message);
        public static SendToClients sendToClients;

        public static void SendToAll(Header header, string message)
        {
            sendToClients(MessageManager.MakeByteMessage(header, message));
        }

        public static void SendToAll(Header header, int index, string message)
        {
            sendToClients(MessageManager.MakeByteMessage(header, index, message));
        }

        public static void SendToAll(Header header, params int[] message)
        {
            sendToClients(MessageManager.MakeByteMessage(header, message));
        }
    }
}
