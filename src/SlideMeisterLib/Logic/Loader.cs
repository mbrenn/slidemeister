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

        /// <summary>
        /// Loads a machine from a string
        /// </summary>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        private Machine LoadMachineFromString(string jsonText)
        {
            var machineObject = JObject.Parse(jsonText);


            _machine = new Machine();
            LoadMachineValues(machineObject);
            LoadTypes(machineObject);
            LoadItems(machineObject);
            LoadTransitions(machineObject);
            LoadSequences(machineObject);


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

        /// <summary>
        /// Loads the types from the given machine object
        /// </summary>
        /// <param name="machineObject">The machine object</param>
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

        /// <summary>
        /// Loads the items from the given machine object
        /// </summary>
        /// <param name="machineObject">The machine object</param>
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
                        throw new InvalidOperationException($"Position not found: {itemTypeValue}");
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

        /// <summary>
        /// Loads the transition from the file
        /// </summary>
        /// <param name="machineObject">The machine object</param>
        private void LoadTransitions(JObject machineObject)
        {
            if (machineObject.TryGetValue("transitions", out JToken transitionToken))
            {
                foreach (var transitionSetItem in transitionToken.Children().OfType<JProperty>())
                {
                    var transitionSetName = transitionSetItem.Name;
                    var transitionSetValue = (JObject) transitionToken[transitionSetName];

                    var transitionSet = new TransitionSet(transitionSetName);

                    foreach (var transitionItem in transitionSetValue.Children().OfType<JProperty>())
                    {
                        var transitionName = transitionItem.Name;
                        var transitionValue = (JValue) transitionSetValue[transitionName];

                        var stateValue = transitionValue.ToString();

                        var foundItem = _machine.Items.FirstOrDefault(x => x.Name == transitionName);
                        if (foundItem == null)
                        {
                            throw new InvalidOperationException($"Object not found {transitionName}");
                        }

                        var foundState = foundItem.Type.States.FirstOrDefault(x => x.Name == stateValue);
                        if (foundState == null)
                        {
                            throw new InvalidOperationException($"Object not found {transitionName}");
                        }

                        var transition = new Transition(foundItem, foundState);
                        transitionSet.Transitions.Add(transition);
                    }

                    _machine.Transitions.Add(transitionSet);
                }
            }
        }

        /// <summary>
        /// Loads the sequences from the file
        /// </summary>
        /// <param name="machineObject">The machine object</param>
        private void LoadSequences(JObject machineObject)
        {
            if (machineObject.TryGetValue("sequences", out JToken sequenceToken))
            {
                foreach (var sequenceItem in sequenceToken.Children().OfType<JProperty>())
                {
                    var sequenceName = sequenceItem.Name;
                    var sequenceValue = (JObject) sequenceToken[sequenceName];

                    var sequence = new TransitionSequence(sequenceName);
                    var sequenceSteps = sequenceValue["steps"];

                    foreach (var sequenceStepItem in sequenceSteps.Children().OfType<JProperty>())
                    {
                        var sequenceStepName = sequenceStepItem.Name;
                        var sequenceStepValue = (JObject) sequenceSteps[sequenceStepName];

                        if (
                            !sequenceStepValue.TryGetValue("transition", out JToken transitionValue)  |
                            !sequenceStepValue.TryGetValue("duration", out JToken durationValue))
                        {
                            throw new InvalidOperationException($"Transition with value {sequenceStepName} does not have transition or duration");
                        }

                        var transition = _machine.Transitions.FirstOrDefault(x => x.Name == transitionValue.ToString());
                        var step = new TransitionSequenceStep(TimeSpan.FromSeconds(Convert.ToDouble(durationValue, CultureInfo.InvariantCulture)), transition);
                        sequence.Steps.Add(step);
                    }

                    _machine.Sequences.Add(sequence);
                }
            }
        }

    }
}