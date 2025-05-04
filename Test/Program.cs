namespace M9Studio.SecureStream.Test
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var serverAdapter = new SecureTransportAdapter(); // id == 1
            var clientAdapter = new SecureTransportAdapter(); // id == 2

            var server = new SecureChannelManager<int>(serverAdapter);
            server.OnSecureSessionEstablished += Server_OnSecureSessionEstablished;

            var client = new SecureChannelManager<int>(clientAdapter);
            client.OnSecureSessionEstablished += Client_OnSecureSessionEstablished;

            // Инициируем подключение: клиент должен послать "handshake"
            client.Connect(serverAdapter.id);

            Console.ReadLine();
        }


        private static void Server_OnSecureSessionEstablished(SecureSession<int> session)
        {
            //Console.WriteLine("server send:      " + BytesToHex([]) + " : " + session.Send([]));
            while (true)
            {
                //отправляем
                byte[] arr = GenerateRandomBytes(16);
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
                Thread.Sleep(1000);


                //отправляем
                byte[] arr = GenerateRandomBytes(16);
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
