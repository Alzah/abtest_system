using Classes.Managers.NetworkTime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Classes.Systems.AbTests
{
    public enum ECondition
    {
        Less,
        LessOrEqual,
        Equal,
        NonEqual,
        MoreOrEqual,
        More,
        Any,
        Never
    }

    /// <summary>
    /// Условия запуска и завершения аб-тестов
    /// </summary>
    [Serializable]
    public abstract class BaseCondition
    {
        public readonly ECondition Condition;

        protected BaseCondition(ECondition condition)
        {
            Condition = condition;
        }

        public abstract bool Check(Dictionary<string, object> userProperties);

        protected bool Comparison<T>(T targetValue, T value) where T : IComparable<T>, IEquatable<T>
        {
            switch (Condition)
            {
                case ECondition.Any:
                    return true;
                case ECondition.Less:
                    return value.CompareTo(targetValue) < 0;
                case ECondition.LessOrEqual:
                    return value.CompareTo(targetValue) <= 0;
                case ECondition.Equal:
                    return value.Equals(targetValue);
                case ECondition.NonEqual:
                    return !value.Equals(targetValue);
                case ECondition.MoreOrEqual:
                    return value.CompareTo(targetValue) >= 0;
                case ECondition.More:
                    return value.CompareTo(targetValue) > 0;
                default:
                    throw new NotImplementedException($"Comparison for {Condition} non implemented!");
            }
        }
    }

    /// <summary>
    /// Условие дата-время
    /// </summary>
    [Serializable]
    public class DateTimeCondition : BaseCondition
    {
        public readonly DateTime Time;
        public readonly bool IsGlobal;

        public DateTimeCondition(ECondition condition, DateTime time, bool isGlobal) : base(condition)
        {
            Time = time;
            IsGlobal = isGlobal;
        }

        public override bool Check(Dictionary<string, object> userProperties)
        {
            try
            {
                var time = (DateTime) userProperties[NetworkTimeManager.TimeKey];
                var isTimeValid = (bool) userProperties[NetworkTimeManager.IsGlobalKey];
                if (IsGlobal && isTimeValid || !IsGlobal)
                {
                    return Comparison(Time, time);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"DateTimeCondition throw: {e}");
                throw e;
            }
            return false;
        }
    }

    /// <summary>
    /// Условие проверки значения
    /// </summary>
    /// <typeparam name="T">Доступен для парсинга int, double, bool, string</typeparam>
    [Serializable]
    public class ValueCondition<T> : BaseCondition where T : IComparable<T>, IEquatable<T>
    {
        public readonly string Key;
        public readonly T Value;

        public ValueCondition(string key, ECondition condition, T value) : base(condition)
        {
            Key = key;
            Value = value;
        }

        public override bool Check(Dictionary<string, object> userProperties)
        {
            try
            {
                var userProperty = (T)userProperties[Key];
                return Comparison(Value, userProperty);
            }
            catch (Exception e)
            {
                Debug.LogError($"ValueCondition<{typeof(T)}> with key: {Key} throw: {e}");
                throw e;
            }
        }
    }
}
