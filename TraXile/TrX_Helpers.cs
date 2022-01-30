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
    }
}
