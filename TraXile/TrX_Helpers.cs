using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TraXile
{
    public static class TrX_Helpers
    {
        /// <summary>
        /// Convert a string, making the first letter uppercase
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CapitalFirstLetter(string input)
        {
            input = input.ToLower();
            return string.Format("{0}{1}", input[0].ToString().ToUpper(), input.Substring(1, input.Length - 1));
        }

        /// <summary>
        /// Get line count from Client.txt. Used for progress calculation
        /// </summary>
        /// <returns></returns>
        public static int GetLogFileLineCount(string path)
        {
            int iCount = 0;
            FileStream fs1 = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            TextReader reader1 = new StreamReader(fs1);
            while ((reader1.ReadLine()) != null)
            {
                iCount++;
            }
            reader1.Close();
            return iCount;
        }

        /// <summary>
        /// Compute hash from raw data
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        /// <summary>
        /// Get the last line of the given file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetLastLineOfFile(string path)
        {
            return File.ReadLines(path).Last();
        }

        public static string GetRegistryValue(string key, string value, string defaultVal = "")
        {
            return (string)Registry.GetValue(key, value, defaultVal);
        }
    }
}
