using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Classes.Systems.AbTests
{
    [Serializable]
    public class AbTest 
    {
        public string Id;
        public int Version = 1;

        public int OptionsCount;
        public Option[] Options = { };
        public int SelectedOption = -1;
        public int Priority;
        public int DistributionId = 1;

        public BaseCondition[] StartConditions;
        public BaseCondition[] EndConditions;

        public AbTest(
            string id,
            int version,
            int optionsCount,
            Option[] options,
            int priority,
            int distributionId,
            BaseCondition[] startConditions,
            BaseCondition[] endConditions)
        {
            Id = id;
            Version = version;

            OptionsCount = optionsCount;
            Options = options;
            Priority = priority;
            DistributionId = distributionId;

            StartConditions = startConditions;
            EndConditions = endConditions;

            if (Options.Length != OptionsCount)
            {
                throw new ArgumentException($"[AbTest] {Id} Options.Length no validated for OptionsCount");
            }

            if (EndConditions == null || EndConditions.Length == 0)
            {
                throw new ArgumentException($"[AbTest] {Id} EndConditions no found");
            }

            var weightSum = Options.Sum(x => x.Weight);
            if (weightSum != 100)
            {
                var currentSum = 0;
                for(int i = 0; i < OptionsCount - 1; i++) 
                {
                    Options[i].Weight = Options[i].Weight / weightSum * 100;
                    currentSum += Options[i].Weight;
                }
                Options[OptionsCount - 1].Weight = 100 - currentSum;

                Debug.Log($"[AbTest] {Id} Sum option's weights != 100. Weights are given to 100.");
            }
        }

        public bool NeedStart(Dictionary<string, object> userProperties)
        {
            try
            {
                return StartConditions.All(x => x.Check(userProperties));
            }
            catch (Exception e)
            { 
                Debug.LogError($"[AbTest] Skip start {Id} abtest, reason: {e}");
                return false; 
            }
        }

        public bool NeedEnd(Dictionary<string, object> userProperties)
        {
            try
            {
                return EndConditions.All(x => x.Check(userProperties));
            }
            catch (Exception e)
            {
                Debug.LogError($"[AbTest] Force end {Id} abtest, reason: {e}");
                return true;
            }
        }

        public string GetId()
        {
            return Id;
        }

        public Option GetOption()
        {
            if (SelectedOption < 0)
            {           
                throw new InvalidOperationException($"[AbTest] Ab test {Id} no running!");
            }
            return Options[SelectedOption];
        }

        public bool HasParameter(string name)
        {
            return GetOption().HasParameter(name);
        }

        public Parameter GetParameter(string name) 
        {
            return GetOption().GetParameter(name);
        }

        public int GetDistributionId()
        {
            return DistributionId;
        }

        public bool EqualsId(AbTest other)
        {
            return Id.Equals(other.GetId());
        }

        public int GetVersion() 
        {
            return Version;
        }

        public override string ToString()
        {
            var option = (SelectedOption < 0) ? "?" : Options[SelectedOption].ToString();
            return $"{Id}, Option:{option} Version {GetVersion()}, Priority {Priority}";
        }
    }
}
