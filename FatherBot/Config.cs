using FatherBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FatherBot
{
    class Config
    {
        public string BotToken { get; set; }

        public string CommandPrefix { get; set; }

        public string FilePath = ".";

        [JsonIgnore]
        public string ConfigFilePath => FilePath + "/config.json";

        public bool ImResponseEnabled { get; set; }


        public Config CreatConfig()
        {
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
