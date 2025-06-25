namespace DocAnalyzer.Domain.Utils
{
    public static class StringExtension
    {
        /// <summary>
        /// Method for converting language code to language name
        /// </summary>
        /// <param name="languageCode"></param>
        /// <returns></returns>
        public static string ConvertLanguageCodeToName(this string languageCode)
        {
            switch (languageCode.ToLower())
            {
                case "es":
                    return "spanish";
                case "en":
                    return "english";
                default:
                    return "portuguese";
            }
        }
    }
}
