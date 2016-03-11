using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class JsonManager
    {
        // create a file json
        public static string SaveSetting(object obj, string FileName)
        {
            // serialize object in Json
            string jsonString = JsonConvert.SerializeObject(obj);

            // write string into file
            System.IO.File.WriteAllText("config\\"+ FileName + ".json", jsonString);

            return jsonString;

        }

        public static Setting LoadSetting(string filename)
        {
            string jsonText = System.IO.File.ReadAllText("config//"+ filename + ".json");
            //return JsonConvert.DeserializeObject<Setting>(jsonText);
            return JsonConvert.DeserializeObject<Setting>(jsonText);            
        }

        internal static Config LoadConfig(string filename)
        {
            string jsonText = System.IO.File.ReadAllText("config//" + filename + ".json");
            return JsonConvert.DeserializeObject<Config>(jsonText);            
        }

        // create a file json
        public static string CreateJson(object obj)
        {
            // serialize object in Json
            string jsonString = JsonConvert.SerializeObject(obj);

            return jsonString;
        }


        public static List<string> GetList(string stringJson)
        {
            return JsonConvert.DeserializeObject<List<string>>(stringJson);
        }

        public static int GetInt(string stringJson)
        {
            return JsonConvert.DeserializeObject<int>(stringJson);
        }

        public static float GetFloat(string stringJson)
        {
            return JsonConvert.DeserializeObject<float>(stringJson);
        }

        internal static List<Bone> GetBoneList(string stringJson)
        {
            return JsonConvert.DeserializeObject<List<Bone>>(stringJson);
        }

        internal static string GetString(string stringJson)
        {
            return JsonConvert.DeserializeObject<string>(stringJson);
        }
    }
}
