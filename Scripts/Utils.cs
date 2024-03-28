using System.Collections.Generic;

namespace SpacetimeDB
{
    public static class Utils
    {
        /// Clips input to maxLength. If we clipped anything,
        /// we'll replace the last 3 characters with "..."
        public static string ClipString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || maxLength <= 0)
            {
                return string.Empty;
            }

            if (input.Length > maxLength)
            {
                return input[..(maxLength - 3)] + "...";
            }

            return input;
        }
        
        public static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1 == null || a2 == null)
                return a1 == a2;

            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;

            return true;
        }
    }

    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            return Utils.ByteArrayCompare(x, y);
        }

        public int GetHashCode(byte[] obj)
        {
            if (obj == null)
                return 0;
            int sum = 0;
            for (int i = 0; i < obj.Length; i++)
                sum += obj[i];
            return sum;
        }
    }
}
