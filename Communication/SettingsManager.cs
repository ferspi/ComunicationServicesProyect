using System.Configuration;

namespace Communication
{
    public class SettingsManager
    {

        public string ReadSettings(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                return appSettings[key] ?? string.Empty;
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error leyendo la configuracion");
                return string.Empty;
            }
        }
    }
}
