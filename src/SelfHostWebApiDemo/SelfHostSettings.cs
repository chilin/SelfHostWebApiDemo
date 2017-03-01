using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfHostWebApiDemo
{
    public class SelfHostSettings
    {
        private readonly CommandLineSettings commandLineSettings;

        private readonly string prefix;
        private readonly NameValueCollection settings;

        public SelfHostSettings(CommandLineSettings commandLineSettings)
        {
            this.commandLineSettings = commandLineSettings;
            this.prefix = "";
            this.settings = ConfigurationManager.AppSettings;
        }

        public int Port
        {
            get { return Convert.ToInt32(GetCmdSetting("port", "9000")); }
        }

        public IEnumerable<string> Urls
        {
            get
            {
                var urls = commandLineSettings.GetValues<string>("url").ToArray();
                if (urls.Any())
                {
                    return urls;
                }

                return GetAppSetting("url", "").Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s));
            }
        }

        public string BaseDirectory
        {
            get { return GetAppSetting("baseDirectory", DefaultBaseDirectory); }
        }

        public bool Interactive
        {
            get { return GetFlagFromAppSetting("interactive", false); }
        }

        protected string GetCmdSetting(string key, string defaultValue)
        {
            return commandLineSettings.GetValueOrDefault(key, GetAppSetting(key, defaultValue));
        }

        protected bool GetFlagFromAppSetting(string key, bool defaultValue)
        {
            var flag = GetAppSetting(key, String.Empty);

            bool result;
            return Boolean.TryParse(flag ?? String.Empty, out result) ? result : defaultValue;
        }

        protected virtual string MapPathFromAppSetting(string key, string defaultValue)
        {
            var path = GetAppSetting(key, defaultValue);

            if (path.StartsWith("~/"))
            {
                path = Path.Combine(Environment.CurrentDirectory, path.Replace("~/", ""));
            }

            return path.Replace('/', Path.DirectorySeparatorChar);
        }

        protected string GetAppSetting(string key, string defaultValue)
        {
            var value = settings[GetAppSettingKey(key)];
            return String.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private string GetAppSettingKey(string key)
        {
            return prefix + key;
        }

        private static readonly string DefaultBaseDirectory = ResolveBaseDirectory();

        private static string ResolveBaseDirectory()
        {
            var binPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase ?? Directory.GetCurrentDirectory();
            var index = binPath.LastIndexOf(Path.DirectorySeparatorChar + "bin", StringComparison.InvariantCultureIgnoreCase);
            if (index > 0)
            {
                binPath = binPath.Substring(0, index);
            }

            return binPath;
        }
    }
}
