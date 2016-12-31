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
            string _filePath = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            _filePath = System.IO.Directory.GetParent(System.IO.Directory.GetParent(_filePath).FullName).FullName + "\\Config\\";
            System.IO.File.WriteAllText(_filePath + FileName + ".json", jsonString);

            return jsonString;

        }

        public static Setting LoadSetting(string filename)
        {
            string _filePath = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            _filePath = System.IO.Directory.GetParent(System.IO.Directory.GetParent(_filePath).FullName).FullName + "\\Config\\";
            string jsonText = System.IO.File.ReadAllText(_filePath + filename + ".json");
            //return JsonConvert.DeserializeObject<Setting>(jsonText);
            return JsonConvert.DeserializeObject<Setting>(jsonText);            
        }

        internal static Config LoadConfig(string filename)
        {
            string jsonText = "";
            if(filename.Contains("C:\\"))
                jsonText = System.IO.File.ReadAllText(filename);
            else
                jsonText = System.IO.File.ReadAllText("config//" + filename + ".json");
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

        internal static List<FrameBufferItem> GetFrameBufferList(string stringJson)
        {
            return JsonConvert.DeserializeObject<List<FrameBufferItem>>(stringJson);
        }

        internal static string GetString(string stringJson)
        {
            return JsonConvert.DeserializeObject<string>(stringJson);
        }

        internal static List<DecompositionAssignment> GetDecompositionAssignment(string stringJson) 
        {
            return JsonConvert.DeserializeObject<List<DecompositionAssignment>>(stringJson); 
        }

        internal static List<List<ComputationData>> GetComputationData(string stringJson)
        {
            return JsonConvert.DeserializeObject<List<List<ComputationData>>>(stringJson); 
        }
    }
}
