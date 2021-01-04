using System;

namespace BackendServer
{
    class Program
    {
        static void Main()
        {
            int N;

            do
            {
                Console.WriteLine("Введите N - максимальное количество одновременно обрабатываемых запросов сервером");
            }
            while (!Int32.TryParse(Console.ReadLine(), out N));

            // Создаём наш сервер, по-умолчанию сразу начинается прослушивание эфира
            new ServerHandler("127.0.0.1", 8081, N);
        }
    }
}
