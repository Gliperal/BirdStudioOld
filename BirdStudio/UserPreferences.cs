using System;
using System.Collections.Generic;

namespace BirdStudio
{
    class UserPreferences
    {
        private const string PREFERENCE_FILE = "./preferences.txt";
        private static bool loaded = false;
        private static IDictionary<string, string> data;

        private UserPreferences() {}

        private static void loadFile()
        {
            if (loaded)
                return;
            try
            {
                data = new Dictionary<string, string>();
                // TODO Read user preferences from file
                data["dark mode"] = "false";
                data["show help"] = "true";
                loaded = true;
            }
            catch (Exception e) {}
        }

        private static void saveFile()
        {
            // TODO
        }

        public static string get(string key, string fallback)
        {
            loadFile();
            if (!loaded)
                return fallback;
            if (data.ContainsKey(key))
                return data[key];
            else
                return fallback;
        }

        public static void set(string key, string value)
        {
            loadFile();
            if (!loaded)
                return;
            if (data.ContainsKey(key) && data[key] == value)
                return;
            data[key] = value;
            saveFile();
        }
    }
}
