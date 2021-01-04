using System;
using System.Threading;

namespace BackendServer.Methods
{
    static class Palindrome
    {
        /// <summary>
        /// Метод проверки входных значений на полиндром <br/>
        /// Полиндромом является строка, которая одинаково читаются в обе стороны.
        /// </summary>
        /// <param name="source">Строка, проверяемая на полиндром</param>
        /// <returns>Булево значение определяющее являются ли входные данные полиндромом</returns>
        internal static bool IsPolyndrome(string source)
        {
            char[] reversedSource = source.ToCharArray();
            Array.Reverse(reversedSource);

            Thread.Sleep(5000); // симуляция долгих вычислений
            if (source == new string(reversedSource))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
