using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        // Генерация случайного байтового массива
        byte[] randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Преобразование в строку
        string secretKey = Convert.ToBase64String(randomBytes);

        Console.WriteLine("Сгенерированный секретный ключ: " + secretKey);
    }
}
