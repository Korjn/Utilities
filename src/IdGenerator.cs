using System.Numerics;

namespace Korkn.Utilities;

/// <summary>
/// IdGenerator generates a unique 22-character base32 string identifier.
/// Ensures uniqueness across machines, processes, and threads.
/// </summary>
public class IdGenerator
{
    // Base32 character set (maps 5-bit values to characters)
    private static readonly char[] encodeChars = "0123456789ABCDEFGHIJKLMNOPQRSTUV".ToCharArray();

    // Unique machine identifier (first 4 bytes of MD5 hash of machine name)
    private static readonly int machineId = GenerateMachineId();

    // Process ID (16 bits)
    private static readonly ushort pid = (ushort)Environment.ProcessId;

    // Increment counter initialized with a cryptographically secure random number (24 bits)
    private static int increment = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1 << 24);

    // A random byte generated once per process to increase uniqueness
    private static readonly byte randomExtra = (byte)new Random().Next(0, 256);

    /// <summary>
    /// Generates a 32-bit machine identifier by hashing the machine name.
    /// </summary>
    private static int GenerateMachineId()
    {
        var hash = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(Environment.MachineName));
        return BitConverter.ToInt32(hash, 0); // Take the first 4 bytes (32 bits)
    }

    /// <summary>
    /// Generates a unique 22-character base32 identifier.
    /// </summary>
    public static string NewId()
    {
        // Current Unix timestamp in seconds (32 bits)
        uint timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Atomically increment the counter; limit to 24 bits (0..16,777,215)
        int inc = Interlocked.Increment(ref increment) & 0xFFFFFF;

        // Build a 112-bit integer composed of:
        // timestamp (32 bits) + machineId (32 bits) + pid (16 bits) + increment (24 bits) + randomExtra (8 bits)

        var value = new BigInteger(0);
      
        value |= (BigInteger)timestamp << 80;        // shift timestamp to highest 32 bits
        value |= (BigInteger)(uint)machineId << 48;  // shift machineId below timestamp
        value |= (BigInteger)pid << 32;              // shift pid below machineId
        value |= (BigInteger)inc << 8;               // shift increment below pid
        value |= randomExtra;                        // add randomExtra in the lowest 8 bits

        /*ulong high = ((ulong)timestamp << 80)        // shift timestamp to highest 32 bits
                   | ((ulong)(uint)machineId << 48)  // shift machineId below timestamp
                   | ((ulong)pid << 32)              // shift pid below machineId
                   | ((ulong)inc << 8)               // shift increment below pid
                   | randomExtra;                    // add randomExtra in the lowest 8 bits*/

        // Allocate buffer for 22 base32 characters (112 bits / 5 = 22.4 → 22 characters)
        Span<char> buffer = stackalloc char[22];

        // Convert the 112-bit integer into a base32 string
        for (int i = 21; i >= 0; i--)
        {
            buffer[i] = encodeChars[(int)(value & 31)];  // take the last 5 bits as index
            value >>= 5;                                 // shift right by 5 bits
        }

        // Return the encoded string
        return new string(buffer);
    }
}