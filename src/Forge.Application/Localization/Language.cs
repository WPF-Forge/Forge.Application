namespace Forge.Application.Localization
{
    using System.Globalization;

    public class Language
    {
        public Language(string name, string dictionaryUri, CultureInfo cultureInfo)
        {
            this.Name = name;
            this.DictionaryUri = dictionaryUri;
            this.CultureInfo = cultureInfo;
        }

        public string Name { get; }

        public string DictionaryUri { get; }

        public CultureInfo CultureInfo { get; }
    }
}
