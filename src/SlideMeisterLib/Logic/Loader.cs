using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlideMeisterLib.Model;

namespace SlideMeisterLib.Logic
{
    public class Loader
    {
        private readonly string _relativePath;
        private List<OverlayType> _types;
        private Machine _machine;

        private Loader(string relativePath)
        {
            _relativePath = relativePath;
        }

        public static Machine LoadMachine(Stream stream, string relativePath)
        {
            using (var reader = new StreamReader(stream))
            {
                var loader = new Loader(relativePath);
                return loader.LoadMachineFromString(reader.ReadToEnd());
            }
        }

        public static Machine LoadMachine(string filePath)
        {
            var jsonText = File.ReadAllText(filePath);
            var loader = new Loader(Path.GetDirectoryName(filePath));
            return loader.LoadMachineFromString(jsonText);
        }

        private Machine LoadMachineFromString(string jsonText)
        {
            var machineObject = JObject.Parse(jsonText);


            _machine = new Machine();
            LoadMachineValues(machineObject);
            LoadTypes(machineObject);
            LoadItems(machineObject);

            return _machine;
        }

        private void LoadMachineValues(JObject machineObject)
        {
            if (machineObject.TryGetValue("name", out JToken nameValue))
            {
                _machine.Name = nameValue.ToString();
            }

            if (machineObject.TryGetValue("version", out JToken versionValue))
            {
                _machine.Version = versionValue.ToString();
            }

            if (machineObject.TryGetValue("backgroundImage", out JToken backgroundImageValue))
            {
                _machine.BackgroundImageUrl =
                    Path.Combine(
                        _relativePath,
                        backgroundImageValue.ToString());
            }
        }

        private void LoadTypes(JObject machineObject)
        {
            // Loads the types
            _types = new List<OverlayType>();

            if (machineObject.TryGetValue("types", out JToken typeValue))
            {
                foreach (var childProperty in typeValue.Children().OfType<JProperty>())
                {
                    var name = childProperty.Name;
                    var overlayType = new OverlayType(name);

                    var childItem = (JObject) typeValue[name];


                    if (childItem.TryGetValue("states", out JToken statesToken))
                    {
                        foreach (var state in statesToken.Children().OfType<JProperty>())
                        {
                            var stateName = state.Name;
                            var overlayState = new OverlayState(stateName);


                            var stateValue = (JObject) statesToken[stateName];
                            if (stateValue.TryGetValue("image", out JToken imageValue))
                            {
                                overlayState.ImageUrl =
                                    Path.Combine(
                                        _relativePath,
                                        imageValue.ToString());
                            }

                            overlayType.States.Add(overlayState);
                        }
                    }

                    _types.Add(overlayType);
                }
            }
        }

        private void LoadItems(JObject machineObject)
        {
            if (machineObject.TryGetValue("items", out JToken itemsToken))
            {
                foreach (var itemProperty in itemsToken.Children().OfType<JProperty>())
                {
                    var itemName = itemProperty.Name;
                    var itemValue = (JObject)itemsToken[itemName];
                    if (!itemValue.TryGetValue("type", out JToken itemTypeValue))
                    {
                        throw new NotImplementedException($"Type not found for {itemName}");
                    }

                    var foundType = _types.FirstOrDefault(x => x.Name == itemTypeValue.ToString());
                    if (foundType == null)
                    {
                        throw new NotImplementedException($"Type not found: {itemTypeValue}");
                    }


                    var item = new OverlayItem(foundType) { Name = itemName };
                    var hasPosition =
                        itemValue.TryGetValue("x", out JToken xValue) &
                        itemValue.TryGetValue("y", out JToken yValue) &
                        itemValue.TryGetValue("width", out JToken widthValue) &
                        itemValue.TryGetValue("height", out JToken heightValue);
                    if (!hasPosition || xValue == null || yValue == null || widthValue == null || heightValue == null)
                    {
                        throw new NotImplementedException($"Position not found: {itemTypeValue}");
                    }

                    item.Position =
                        new Rectangle(
                            Convert.ToDouble(xValue, CultureInfo.InvariantCulture),
                            Convert.ToDouble(yValue, CultureInfo.InvariantCulture),
                            Convert.ToDouble(widthValue, CultureInfo.InvariantCulture),
                            Convert.ToDouble(heightValue, CultureInfo.InvariantCulture));
                    if (itemValue.TryGetValue("defaultState", out JToken defaultStateValue))
                    {
                        item.CurrentState = item.Type.States.First(x => x.Name == defaultStateValue.ToString());
                    }



                    _machine.AddItem(item);
                }
            }
        }
    }
}