using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorCore
{
    public static class JsonUtility
    {
        public static string ToJson (object obj, bool isPrettyPrint = true)
        {
            Formatting formatting = isPrettyPrint ? Formatting.Indented : Formatting.None;

            return JsonConvert.SerializeObject (obj, formatting);
        }

        public static T FromJson<T> (string jsonStr) where T : new()
        {
            try
            {
                return JsonConvert.DeserializeObject<T> (jsonStr);
            }
            catch (Exception e)
            {
                return new T ();
            }
        }
    }
}
