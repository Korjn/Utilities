namespace Korkn.Utilities;

/// <summary>
/// Генератор уникальных ID длиной 22 символа в base32, с уникальностью между машинами, процессами и вызовами.
/// </summary>
public class IdGenerator
{
    // Алфавит base32 для кодирования (5 бит на символ)
    private static readonly char[] encodeChars = "0123456789ABCDEFGHIJKLMNOPQRSTUV".ToCharArray();

    // Уникальный ID машины (хеш названия хоста, 4 байта / 32 бита)
    private static readonly int machineId = GenerateMachineId();

    // Идентификатор процесса (2 байта / 16 бит)
    private static readonly ushort pid = (ushort)Environment.ProcessId;

    // Начальное значение счётчика increment — случайное крипто-стойкое число (3 байта / 24 бита)
    private static int increment = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1 << 24);

    // Случайный байт (8 бит) при запуске процесса, чтобы добавить уникальности
    private static readonly byte randomExtra = (byte)new Random().Next(0, 256);

    /// <summary>
    /// Генерация ID машины на основе MD5-хэша hostname (берём первые 4 байта)
    /// </summary>
    private static int GenerateMachineId()
    {
        var hash = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(Environment.MachineName));
        return BitConverter.ToInt32(hash, 0); // 4 байта (32 бита)
    }

    /// <summary>
    /// Генерация нового уникального ID
    /// </summary>
    public static string NewId()
    {
        // timestamp: текущее время в секундах с начала эпохи (4 байта / 32 бита)
        uint timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Увеличиваем счётчик atomарно (Interlocked гарантирует потокобезопасность)
        // Ограничиваем значением до 24 бит (0..16 777 215)
        int inc = Interlocked.Increment(ref increment) & 0xFFFFFF;

        // Собираем все данные в одно большое 112-битное число:
        // timestamp (32) + machineId (32) + pid (16) + increment (24) + randomExtra (8)
        ulong high = ((ulong)timestamp << 80) // timestamp сдвигаем в старшие биты (слева)
                   | ((ulong)(uint)machineId << 48) // machineId сразу после timestamp
                   | ((ulong)pid << 32)             // pid после machineId
                   | ((ulong)inc << 8)              // increment после pid
                   | randomExtra;                   // randomExtra в младшие 8 бит

        // Буфер для символов ID (22 символа base32)
        Span<char> buffer = stackalloc char[22]; // 112 бит / 5 бит = 22 символа

        // Преобразуем число high в строку base32
        for (int i = 21; i >= 0; i--)
        {
            buffer[i] = encodeChars[(int)(high & 31)]; // берём последние 5 бит → символ
            high >>= 5; // сдвигаем вправо на 5 бит
        }

        // Возвращаем ID как строку
        return new string(buffer);
    }
}