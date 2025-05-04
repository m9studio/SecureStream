namespace M9Studio.SecureStream.Test
{
    internal class SecureTransportAdapter : ISecureTransportAdapter<int>
    {
        static Dictionary<int, SecureTransportAdapter> connect = new();
        private Dictionary<int, Queue<byte[]>> queue = new();

        static int count = 0;
        int id;
        public SecureTransportAdapter()
        {
            count++;
            id = count;
            connect.Add(id, this);
        }

        public event Action<int> OnConnected;
        public event Action<int> OnDisconnected;

        public byte[] ReceiveFrom(int address)
        {
            while (true)
            {
                if (queue.ContainsKey(address))
                {
                    if(queue[address] != null && queue[address].Count > 0)
                    {
                        return queue[address].Dequeue();
                    }
                }
                Thread.Sleep(10); // чтобы не жрать CPU
            }
        }

        public bool SendTo(byte[] buffer, int address)
        {
            if (connect.ContainsKey(address))
            {
                connect[address].AddCache(buffer, id);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Метод заглушка, чтобы симулировать передачу пакета
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="address"></param>
        public void AddCache(byte[] buffer, int address) {
            if (!queue.ContainsKey(address))
            {
                queue.Add(address, new Queue<byte[]>());
                OnConnected?.Invoke(address);
            }
            queue[address].Enqueue(buffer);
        }
    }
}
