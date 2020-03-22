using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace TestProject.WebAPI
{
    public class CustomStringLocalizer : IStringLocalizer
    {
        Dictionary<string, Dictionary<string, string>> resources;

        const string Key1 = "DramaKey";
        const string Key2 = "HorrorKey";
        const string Key3 = "ComedyKey";

        public CustomStringLocalizer()
        {

            Dictionary<string, string> enDict = new Dictionary<string, string>
            {
                {Key1, "Drama" },
                {Key2, "Horror" },
                {Key3, "Comedy" }
            };

            Dictionary<string, string> ruDict = new Dictionary<string, string>
            {
                {Key1, "Драма" },
                {Key2, "Ужасы" },
                {Key3, "Комедия" }
            };

            Dictionary<string, string> itDict = new Dictionary<string, string>
            {
                {Key1, "Dramma" },
                {Key2, "Orrore" },
                {Key3, "Сommedia" }
            };

            resources = new Dictionary<string, Dictionary<string, string>>
            {
                {"en", enDict },
                {"ru", ruDict },
                {"it", itDict }
            };
        }

        public LocalizedString this[string name]
        {
            get
            {
                var currentCulture = CultureInfo.CurrentUICulture;
                string val = "";
                if (resources.ContainsKey(currentCulture.Name))
                {
                    if (resources[currentCulture.Name].ContainsKey(name))
                    {
                        val = resources[currentCulture.Name][name];
                    }
                }
                return new LocalizedString(name, val);
            }
        }

        public LocalizedString this[string name, params object[] arguments] => throw new NotImplementedException();

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return this;
        }
    }
}
