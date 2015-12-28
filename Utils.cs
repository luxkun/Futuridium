using System;
using System.Linq;

namespace Futuridium
{
    public static class Utils
    {
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // lame workaround, why???
        public static int FixBoxValue(int value)
        {
            return (int)(value * 1.33);
        }
    }
}