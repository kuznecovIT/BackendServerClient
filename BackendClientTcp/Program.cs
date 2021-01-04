using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackendClientTcp
{
    class Program
    {       
        private const string ip = "127.0.0.1";
        private const int port = 8081;

        // точка подключения к серверу
        static private EndPoint tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

        static void Main()
        {
            Console.WriteLine("Введите путь до папки с текстовыми файлами, содержимое которых необходимо проверить на полиндром (Например: C:\\01\\texts)");
            string sourceDir = Console.ReadLine();

            // Записываем данные из файлов, путь к которым указали ранее в массив
            string[] sourcesArray = ReadSourcesFromDirectory(sourceDir);

            // Для каждого элемента массива делаем запрос на сервер и ожидаем ответа проверки на полиндром
            for (int fileIndex = 0; fileIndex < sourcesArray.Length; fileIndex++)
            {               
                ConnectToServerAndSendDataAsync(sourcesArray[fileIndex]);
            }

            Console.ReadLine();
        }

        /// <summary>
        /// Подключение к серверу и отправка данных
        /// </summary>
        /// <param name="dataString">Строковые данные, отправляемые на сервер</param>
        static void ConnectToServerAndSendData(object dataString)
        {
            try
            {
                // Создаём сокет, через который будет устанавливаться соединение
                var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Закодировали отправляемое сообщение
                var data = Encoding.UTF8.GetBytes((string)dataString);

                // Подключаемся к сокету
                tcpSocket.Connect(tcpEndPoint);

                // Отправляем данные
                tcpSocket.Send(data);

                var buffer = new byte[256];
                var answer = new StringBuilder();

                do
                {
                    var size = tcpSocket.Receive(buffer);
                    answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
                }
                while (tcpSocket.Available > 0);

                if (answer.ToString() == "Сервер перегружен, повторите запрос позже")
                {
                    Console.ResetColor();
                    Console.WriteLine($"В связи с перегрузкой сервера, значение : '{dataString}' для проверки на полиндром будет повторно отправлено через 5 секунд.");

                    Thread.Sleep(5000);
                    ConnectToServerAndSendData(dataString);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(answer);
                }

                tcpSocket.Shutdown(SocketShutdown.Both);
                tcpSocket.Close();
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Произошла ошибка при подключении к серверу и отправке данных на сервер");
            }
            
        }

        /// <summary>
        /// Подключение к серверу и отправка данных асинхронно
        /// </summary>
        /// <param name="dataString">Строковые данные, отправляемые на сервер</param>
        /// <returns></returns>
        static async Task ConnectToServerAndSendDataAsync(object dataString)
        {
            await Task.Run(() => ConnectToServerAndSendData(dataString));
        }

        /// <summary>
        /// Считывание содержимого файлов определенной директории в массив
        /// </summary>
        /// <param name="dirLocation">Путь к папке с файлами</param>
        /// <returns>Массив строк содержащий значения элементов в заданной папке</returns>
        static string[] ReadSourcesFromDirectory(string dirLocation)
        {
            if (Directory.Exists(dirLocation))
            {
                // определяем список содержимого текстовых файлов
                List<string> filesSource = new List<string>();

                // определяем список путей до файлов из директории dirLocation
                string[] files = Directory.GetFiles(dirLocation);       

                foreach (string s in files)
                {
                    using (FileStream fs = File.OpenRead(s))
                    {
                        // преобразуем считанную строку в байты
                        byte[] readBytesArr = new byte[fs.Length];

                        // считываем данные
                        fs.Read(readBytesArr, 0, readBytesArr.Length);

                        // декодируем байты в строку и добавляем в лист filesSource                       
                        filesSource.Add(Encoding.Default.GetString(readBytesArr));
                    }
                }

                return filesSource.ToArray();
            }
            else
            {
                throw new NotImplementedException(message: $"Путь до директории: {dirLocation} не обнаружен.");
            }
        }
    }
}
