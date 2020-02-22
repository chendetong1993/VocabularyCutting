using System;
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace WPF
{
    public class Languages
    {
        private ResourceManager _rm;

        private string _currentLan;

        public Languages()
        {
            _currentLan = "zh-HK";

            _rm = new ResourceManager("Vocabulary Cutting.Sources.Languages", Assembly.GetExecutingAssembly());
        }

        public string[] ReturnLanguages()
        {
            return null;
        }

        public void ChangeLanguage(string Language)
        {
            _currentLan = Language;
        }

        public string GetText(string key)
        {
            string resourceValue = null;
            if (key != "")
            {
                resourceValue = _rm.GetString(key.Trim(), new CultureInfo(_currentLan, true));
            }
            return resourceValue;
        }
    }
}
