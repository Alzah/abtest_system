using System;
using System.Linq;

namespace Classes.Systems.AbTests
{
    /// <summary>
    /// Когорта аб-теста со всеми своими значениями
    /// </summary>
    [Serializable]
    public class Option
    {
        public readonly string Id;
        public readonly Parameter[] Parameters;
        public int Weight;

        public Option(string id, Parameter[] parameters, int weight) 
        {
            Id = id;
            Parameters = parameters;
            Weight = weight;
        }

        public bool HasParameter(string name)
        {
            return Parameters.FirstOrDefault(x => x.Name.Equals(name)) != null;
        }

        public Parameter GetParameter(string name)
        {
            return Parameters.FirstOrDefault(x => x.Name.Equals(name));
        }

        public override string ToString()
        {
            return Id;
        }
    }

    [Serializable]
    public abstract class Parameter
    {
        public readonly string Name;

        protected Parameter(string name)
        {
            Name = name;
        }

        public abstract object GetValue();
    }

    [Serializable]
    public class Parameter<T> : Parameter
    {
        public readonly T Value;

        public Parameter(string name, T value) : base(name)
        {
            Value = value;
        }

        public override object GetValue()
        {
            return Value;
        }
    }
}
