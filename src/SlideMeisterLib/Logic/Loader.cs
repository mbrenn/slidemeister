using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlideMeisterLib.Model;

namespace SlideMeisterLib.Logic
{
    public class Loader
    {
        public static Machine LoadMachine(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return LoadMachineFromString(reader.ReadToEnd());
            }
        }

        public static Machine LoadMachine(string filePath)
        {
            var jsonText = File.ReadAllText(filePath);
            return LoadMachineFromString(jsonText);
        }

        private static Machine LoadMachineFromString(string jsonText)
        {
            JObject.Parse(jsonText);


            var newMachine = new Machine();


            return newMachine;
        }
    }
}