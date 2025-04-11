namespace ProjectM
{
    public static class ExtensionString
    {
        public static string Locale(this string str)
        {
            return LocaleManager.GetLocale(str);
        }
        
        public static string Locale(this string str, params object[] args)
        {
            return LocaleManager.GetLocale(str, args);
        }
    }
}