using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string firstChar;
            string rest;

            input = input.ToLower();
            firstChar = input.Substring(0, 1);
            rest = input.Substring(1, input.Length - 1);
            return string.Format("{0}{1}", firstChar.ToUpper(), rest);
        }
    }
}
