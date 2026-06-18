using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace smallTV
{
    public class Config
    {
        public Key HotkeyResetFinishKey { get; set; } = Key.RightShift;
        public List<Key> TopLeftBKeys { get; set; } = [Key.RightShift, Key.C, Key.LeftShift];
        public List<Key> BottomRightBKeys { get; set; } = [Key.RightShift, Key.V, Key.LeftShift];
        public List<Key> ToggleKeys { get; set; } = [Key.RightShift, Key.B, Key.LeftShift];
        


        public static Config Instance { get; private set; }

        private static readonly string configFilename = "Config.data";

        // Called once
        static Config()
        {
            Load();
        }

        // to prevent new() call
        [JsonConstructor]
        private Config() {

        }

        public static void Load()
        {
            if (File.Exists(configFilename))
            {
                try
                {
                    string json = File.ReadAllText(configFilename);
                    Instance = JsonSerializer.Deserialize<Config>(json) ?? new Config();
                }
                catch (Exception e)
                {
                    Instance = new Config();
                    Instance.Save();
                }
            }
            else
            {
                Instance = new Config();
                Instance.Save();
            }
        }

        public void Save()
        {
            string jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText(configFilename, jsonString);
        }
    }
}
