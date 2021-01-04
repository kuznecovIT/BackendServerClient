using BackendServer.Methods;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BackendServer
{
    class ServerHandler
    {
        string ServerIp { get; set; }
        int ServerPort {get; set;}
        IPEndPoint TcpEndPoint { get; set; }
        Socket TcpSocket { get; set; }
        int WorkingRequestsMaximum { get; set; }
        int WorkingRequestsCounter { get; set; }

        ServerHandler() { }

        /// <summary>
        /// Определяет TCP сервер на Socket соединениях
        /// </summary>
        /// <param name="serverIp">IP адрес сервера</param>
        /// <param name="serverPort">Порт сервера</param>
        /// <param name="workingRequestsMaximum">Максимальное количество обрабатываемых запросов сервером</param>
        internal ServerHandler(string serverIp, int serverPort, int workingRequestsMaximum)
        {
            this.ServerIp = serverIp;
            this.ServerPort = serverPort;
            this.WorkingRequestsMaximum = workingRequestsMaximum;

            // Определение точки подключения к серверу
            TcpEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

            // Определение сокета, через который будет устанавливаться соединение
            TcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Указываем где сокету слушать
            TcpSocket.Bind(TcpEndPoint);

            // Начинаем приём запросов сервером
            StartListening();
        }

        /// <summary>
        /// Начать принимать запросы от клиентов
        /// </summary>
        internal void StartListening()
        {
            WorkingRequestsCounter = 0;

            // Начинаем слушать эфир с очередью в 5 подключений 
            TcpSocket.Listen(5);

            while (true)
            {
                Socket listenen = TcpSocket.Accept();

                var buffer = new byte[256];
                var size = 0;
                var data = new StringBuilder();

                do
                {
                    // Считываем количество полученных байт
                    size = listenen.Receive(buffer);

                    // Записываем считанные данные в data
                    data.Append(Encoding.UTF8.GetString(buffer, 0, size));
                }
                while (listenen.Available > 0);

                Console.WriteLine($"На сервер поступили данные для анализа на полиндром : '{data}'");

                // Запускаем задачу обработки данных, в нашем случае проверку на полиндром
                new Task(() => PolyndromCheck(listenen, data)).Start();
            }
        }

        /// <summary>
        /// Проверка строки, переданной входящим подключением, на полиндром <br/>
        /// Результат проверки будет отправлен клиенту
        /// </summary>
        /// <param name="listenen">Сокет входящего запроса</param>
        /// <param name="data">Строка для проверки</param>
        void PolyndromCheck(Socket listenen, StringBuilder data)
        {
            try 
            {
                if (++WorkingRequestsCounter > WorkingRequestsMaximum)
                {
                    Console.WriteLine("Превышение, количества одновременных обработок, отправка таймаута клиенту");
                    listenen.Send(Encoding.UTF8.GetBytes("Сервер перегружен, повторите запрос позже"));
                }
                else if (Palindrome.IsPolyndrome(data.ToString()))
                {
                    listenen.Send(Encoding.UTF8.GetBytes($"Ваше сообщение '{data}' является полиндромом"));
                }
                else
                {
                    listenen.Send(Encoding.UTF8.GetBytes($"Ваше сообщение '{data}' НЕ является полиндромом"));
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine($"При обработке запроса от клиента произошла ошибка: {e.Message}");
            }
            finally
            {
                listenen.Shutdown(SocketShutdown.Both);
                listenen.Close();

                // После обработки уменьшаем счетчик количества обрабатываемых запросов
                WorkingRequestsCounter--;
            }                  
        }
    }
}
