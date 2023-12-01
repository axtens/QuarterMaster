using System;
using System.Collections.Generic;
using System.Configuration;

namespace QuarterMaster.Configuration
{
    public static class Configuration
    {
        public static Tuple<int, Dictionary<string, string>> ReadAllSettings()
        {
            int count = 0;
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {

                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        keyValues[key] = appSettings[key];
                        count++;
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
            }
            return new Tuple<int, Dictionary<string, string>>(count, keyValues);
        }

        public static string ReadSetting(string key)
        {
            string result = string.Empty;
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                result = appSettings[key] ?? string.Empty;
            }
            catch (ConfigurationErrorsException)
            {
            }
            return result;
        }

        public static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }
    }
}
