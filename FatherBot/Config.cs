using FatherBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;

namespace FatherBot
{
    class Config
    {
        public string BotToken { get; set; }

        public string CommandPrefix { get; set; }

        public string RootFilePath = ConfigurationManager.AppSettings["RootDir"];

        [JsonIgnore]
        public string ConfigFilePath => RootFilePath + "config.json";

        public bool ImResponseEnabled { get; set; }

        public BotActivity BotActivity {get; set;}


        public Config CreateConfig()
        {
            if (!Directory.Exists(RootFilePath))
                Directory.CreateDirectory(RootFilePath);

            if (!File.Exists(ConfigFilePath))
            {
                using (StreamWriter sw = File.CreateText(ConfigFilePath))
                {
                    sw.Write("{\n  \"BotToken\": \"\",\n  \"BotActivity\": {\n    \"Activity\": \"footy\",\n    \"ActivityType\": \"Watching\"\n  },\n  \"ImResponseEnabled\": true,\n  \"CommandPrefix\": \"\"\n}");
                }
                throw new Exception("Config not found new config created please fill.");
            }
            else
            {
                using (StreamReader sr = File.OpenText(ConfigFilePath))
                {
                    var _config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());

                    return _config;
                }
            }
        }
    }
}
