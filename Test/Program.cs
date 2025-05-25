namespace M9Studio.SecureStream.Test
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var serverAdapter = new SecureTransportAdapter(); // id == 1
            var clientAdapter = new SecureTransportAdapter(); // id == 2

            var server = new SecureChannelManager<int>(serverAdapter);
            server.OnConnected += Server_OnSecureSessionEstablished;

            var client = new SecureChannelManager<int>(clientAdapter);

            // Инициируем подключение: клиент должен послать "handshake"
            Client_OnSecureSessionEstablished(client.Connect(serverAdapter.id));

            Console.ReadLine();
        }
        static Random Random = new Random();

        private static void Server_OnSecureSessionEstablished(SecureSession<int> session)
        {
            /*byte[] arr2 = GenerateRandomBytes(16);
            Console.WriteLine("server send:      " + BytesToHex(arr2) + " : " + session.Send(arr2));*/
            while (true)
            {
                //отправляем
                byte[] arr = GenerateRandomBytes(Random.Next(65)/*16*/);
                Console.WriteLine("server send:      " + BytesToHex(arr) + " : " + session.Send(arr));

                //ждем  ответа от клиента
                Console.WriteLine("client -> server: " + BytesToHex(session.Receive()));
            }
        }

        private static void Client_OnSecureSessionEstablished(SecureSession<int> session)
        {
            while (true)
            {
                //ждем сообщения от сервера
                Console.WriteLine("server -> client: " + BytesToHex(session.Receive()));

                //задержка
                Thread.Sleep(500);


                //отправляем
                byte[] arr = GenerateRandomBytes(Random.Next(65));
                Console.WriteLine("client send:      " + BytesToHex(arr) + " : " + session.Send(arr));
            }
        }















        public static byte[] GenerateRandomBytes(int length = 64)
        {
            byte[] bytes = new byte[length];
            Random random = new Random();
            random.NextBytes(bytes);
            return bytes;
        }
        public static string BytesToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
