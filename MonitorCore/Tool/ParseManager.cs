using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorCore
{
    public static class ParseManager
    {
        const string GameSettingPath = "GameSettings";

        public static T LoadJson<T> () where T : new()
        {
            T data = default;

            var path = Path.Combine (FileUtility.CurrentDriectory, GameSettingPath, $"{typeof (T).Name}.json");

            if (File.Exists (path))
            {
                var json = File.ReadAllText (path);

                if (string.IsNullOrEmpty (json) == false)
                {
                    data = JsonUtility.FromJson<T> (json);
                }
                else
                {
                    data = new T ();
                }
            }
            else
            {
                data = new T ();
            }

            return data;
        }

        public static void SaveJson<T> (T data) where T : new()
        {
            var path = Path.Combine (FileUtility.CurrentDriectory, GameSettingPath, $"{typeof (T).Name}.json");

            var json = JsonUtility.ToJson (data);

            File.WriteAllText (path, json);
        }
    }
}
