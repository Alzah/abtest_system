using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Classes.Systems.AbTests
{
    /// <summary>
    /// Конвертер для List<AbTest> из кастомного конфига
    /// Пример конфига: Assets/StreamingAssets/abtest_config.json
    /// </summary>
    public class JsonAbTestsConverter : JsonConverter<List<AbTest>>
    {
        public override void WriteJson(JsonWriter writer, List<AbTest> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
            writer.WriteValue(value.ToString());
        }

        public override List<AbTest> ReadJson(JsonReader reader, Type objectType, List<AbTest> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            JToken abtests = null;

            try
            {
                abtests = jObject["ab_tests"];
            }
            catch (Exception e)
            {
                Debug.LogError($"[AbTestsConverter] {e}");
                Debug.LogError($"[AbTestsConverter] Json config have not ab_tests[] block! Check config and try again.");
            }

            var list = new List<AbTest>();
            if (abtests == null) 
            {
                return list;
            }

            foreach (var abtest in abtests)
            {
                try
                {
                    var id = (string)abtest["Id"];
                    var version = (int)abtest["Version"];
                    var optionsCount = (int)abtest["OptionsCount"];
                    var priority = (int)abtest["Priority"];
                    var distributionId = (int)abtest["DistributionId"];
                    var options = abtest["Options"]
                        .Select(opt => ParseOption(opt))
                        .ToArray();
                    var startConditions = abtest["StartConditions"]
                        .Select(сondition => ParseCondition(сondition))
                        .ToArray();
                    var endConditions = abtest["EndConditions"]
                        .Select(сondition => ParseCondition(сondition))
                        .ToArray();

                    list.Add(new AbTest(id, version, optionsCount, options, priority, distributionId, startConditions, endConditions));
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AbTestsConverter] Skip parsing {abtest["Id"]} with exception: {e}");
                }
            }

            return list;
        }

        private Option ParseOption(JToken token) 
        {
            return new Option
            (
                (string)token["Id"],
                token["Parameters"].Select(x => ParseOptionParameter(x)).ToArray(),
                (int)token["Weight"]
            );
        }

        private Parameter ParseOptionParameter(JToken token)
        {
            switch (token["Value"].Type)
            {
                case JTokenType.Integer:
                    return new Parameter<int>((string)token["Name"], (int)token["Value"]);
                case JTokenType.Float:
                    return new Parameter<float>((string)token["Name"], (float)token["Value"]);
                case JTokenType.String:
                    return new Parameter<string>((string)token["Name"], (string)token["Value"]);
                default:
                    throw new NotImplementedException($"[AbTestsConverter] Unknown type for field {token["Name"]}");
            }
        }

        private BaseCondition ParseCondition(JToken token)
        {
            var type = (string)token["TypeCondition"];
            switch (type)
            {
                case "Time":
                    return ParseDateTimeCondition(token);
                case "Value":
                    return ParseValueCondition(token);
                default:
                    throw new NotImplementedException($"[AbTestsConverter] Unknown type:{type} for ParseCondition");
            }
        }

        private BaseCondition ParseValueCondition(JToken token)
        {
            switch (token["Value"].Type)
            {
                case JTokenType.Integer:
                    return new ValueCondition<int>((string)token["Key"], ParseEnum(token["Condition"]), (int)token["Value"]);
                case JTokenType.Float:
                    return new ValueCondition<float>((string)token["Key"], ParseEnum(token["Condition"]), (float)token["Value"]);
                case JTokenType.String:
                    return new ValueCondition<string>((string)token["Key"], ParseEnum(token["Condition"]), (string)token["Value"]);
                default:
                    throw new NotImplementedException($"[AbTestsConverter] Unknown type for field {token["Key"]}");
            }
        }

        private BaseCondition ParseDateTimeCondition(JToken token)
        {
            var time = (DateTime)token["Time"];
            var isGlobal = (bool)token["IsGlobal"];
            var condition = ParseEnum(token["Condition"]);

            return new DateTimeCondition(condition, time, isGlobal);
        }

        private ECondition ParseEnum(JToken token) => (ECondition)Enum.Parse(typeof(ECondition), (string)token, true);
    }
}
