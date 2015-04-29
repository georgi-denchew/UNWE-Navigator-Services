using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UNWE_Navigator_Services.Services
{
    public class LibFunct
    {
        public static void ResizeArray(ref string[,] Arr, int x)
        {
            string[,] _arr = new string[x, 5];
            int minRows = Math.Min(x, Arr.GetLength(0));
            int minCols = Math.Min(5, Arr.GetLength(1));
            for (int i = 0; i < minRows; i++)
                for (int j = 0; j < minCols; j++)
                    _arr[i, j] = Arr[i, j];
            Arr = _arr;
        }

        public static void ResizeArray3(ref string[,] Arr, int x)
        {
            string[,] _arr = new string[x, 3];
            int minRows = Math.Min(x, Arr.GetLength(0));
            int minCols = Math.Min(3, Arr.GetLength(1));
            for (int i = 0; i < minRows; i++)
                for (int j = 0; j < minCols; j++)
                    _arr[i, j] = Arr[i, j];
            Arr = _arr;
        }

        public static string GetRandom()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }
    }
}