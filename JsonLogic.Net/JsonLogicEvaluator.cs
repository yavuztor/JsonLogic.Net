using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JsonLogic.Net
{
    public class JsonLogicEvaluator : IProcessJsonLogic, IManageOperations
    {
        private Dictionary<string, Func<IProcessJsonLogic, JToken[], object, object>> registry;

        public JsonLogicEvaluator()
        {
            registry = new Dictionary<string, Func<IProcessJsonLogic, JToken[], object, object>>();
            AddDefaultOperations();
        }

        public void AddOperation(string name, Func<IProcessJsonLogic, JToken[], object, object> operation)
        {
            registry[name] = operation;
        }

        public object Apply(JToken rule, object data)
        {
            if (rule is null) return null;
            if (rule is JValue) return (rule as JValue).Value;

            var ruleObj = (JObject) rule;
            var p = ruleObj.Properties().First();
            var opName = p.Name;
            var opArgs = (p.Value is JArray) ? (p.Value as JArray).ToArray() : new JToken[] { p.Value };
            var op = GetOperation(opName);
            return op(this, opArgs, data);
        }

        public void DeleteOperation(string name)
        {
            registry.Remove(name);
        }

        public Func<IProcessJsonLogic, JToken[], object, object> GetOperation(string name)
        {
            return registry[name];
        }

        private bool IsAny<T>(params object[] subjects) 
        {
            return subjects.Any(x => x != null && x is T);
        }

        private void AddDefaultOperations()
        {
            AddOperation("==", (p, args, data) => p.Apply(args[0], data).Equals(p.Apply(args[1], data)));
            
            AddOperation("===", (p, args, data) => p.Apply(args[0], data).Equals(p.Apply(args[1], data)));

            AddOperation("!==", (p, args, data) => !p.Apply(args[0], data).Equals(p.Apply(args[1], data)));

            AddOperation("!=", (p, args, data) => !p.Apply(args[0], data).Equals(p.Apply(args[1], data)));

            AddOperation("+", (p, args, data) => Min2From(args.Select(a => p.Apply(a, data))).Aggregate((prev, next) =>
            {
                if (IsAny<string>(next, prev))
                    return (prev ?? string.Empty).ToString() + next.ToString();

                return Convert.ToDouble(prev ?? 0d) + Convert.ToDouble(next);
            }));

            AddOperation("-", ReduceDoubleArgs((prev, next) => prev - next));

            AddOperation("/", ReduceDoubleArgs((prev, next) => prev / next));

            AddOperation("*", ReduceDoubleArgs((prev, next) => prev * next));

            AddOperation("%", ReduceDoubleArgs((prev, next) => prev % next));

            AddOperation("max", ReduceDoubleArgs((prev, next) => (prev > next) ? prev : next));

            AddOperation("min", ReduceDoubleArgs((prev, next) => (prev < next) ? prev : next));

            AddOperation("<", DoubleArgsSatisfy((prev, next) => prev < next));

            AddOperation("<=", DoubleArgsSatisfy((prev, next) => prev <= next));
            
            AddOperation(">", DoubleArgsSatisfy((prev, next) => prev > next));

            AddOperation(">=", DoubleArgsSatisfy((prev, next) => prev >= next));

            AddOperation("var", (p, args, data) => {
                var names = p.Apply(args.First(), data).ToString();
                try 
                {
                    return GetValueByName(data, names);
                }
                catch (Exception e) 
                {
                    object defaultValue = (args.Count() == 2) ? p.Apply(args.Last(), data) : null;
                    return defaultValue;
                }
            });

            AddOperation("and", (p, args, data) => {
                bool value = Convert.ToBoolean(p.Apply(args[0], data));
                for (var i = 1; i < args.Length && value; i++) 
                {
                    value = Convert.ToBoolean(p.Apply(args[i], data));
                }
                return value;
            });

            AddOperation("or", (p, args, data) => {
                bool value = Convert.ToBoolean(p.Apply(args[0], data));
                for (var i = 1; i < args.Length && !value; i++) 
                {
                    value = Convert.ToBoolean(p.Apply(args[i], data));
                }
                return value;
            });

            AddOperation("if", (p, args, data) => {
                for (var i = 0; i < args.Length - 1; i += 2) 
                {
                    if (Convert.ToBoolean(p.Apply(args[i], data))) return p.Apply(args[i+1], data);
                }
                return p.Apply(args[args.Length - 1], data);
            });

            AddOperation("missing", (p, args, data) => args.Select(a => p.Apply(a, data).ToString()).Where(n => {
                try 
                {
                    GetValueByName(data, n);
                    return false;
                }
                catch
                {
                    return true;
                }
            }));

            AddOperation("missing_some", (p, args, data) => {
                var minRequired = Convert.ToDouble(p.Apply(args[0], data));
                var keys = (args[1] as JArray).ToArray();
                var missingKeys = GetOperation("missing").Invoke(p, keys, data) as IEnumerable<object>;
                var validKeyCount = keys.Length - missingKeys.Count();
                return (validKeyCount >= minRequired) ? new object[0] : missingKeys;
            });
        }

        private object GetValueByName(object data, string namePath)
        {
            string[] names = namePath.Split('.');
            object d = data;
            foreach (string name in names) 
            {
                if (d == null) return null;
                if (d.GetType().IsArray) 
                {
                    d = (d as Array).GetValue(int.Parse(name));
                }
                else if (typeof(IEnumerable<object>).IsAssignableFrom(d.GetType())) 
                {
                    d = (d as IEnumerable<object>).Skip(int.Parse(name)).First();
                }
                else if (typeof(IDictionary<string, object>).IsAssignableFrom(d.GetType()))
                {
                    var dict = (d as IDictionary<string, object>);
                    if (!dict.ContainsKey(name)) throw new Exception();
                    d = dict[name];
                }
                else 
                {
                    var property = d.GetType().GetProperty(name, BindingFlags.Public);
                    if (property == null) throw new Exception();
                    d = property.GetValue(d);
                }
            }
            return d;
        }

        private Func<IProcessJsonLogic, JToken[], object, object> DoubleArgsSatisfy(Func<double, double, bool> criteria)
        {
            return (p, args, data) => {
                var values = args.Select(a => a == null ? 0d : Convert.ToDouble(p.Apply(a, data))).ToArray();
                for (int i = 1; i < values.Length; i++) {
                    if (!criteria(values[i-1], values[i])) return false;
                }
                return true;
            };
        }

        private static bool IsEnumerable(object d)
        {
            return d.GetType().IsArray || (d as IEnumerable<object>) != null;
        }

        private static Func<IProcessJsonLogic, JToken[], object, object> ReduceDoubleArgs(Func<double, double, double> reducer)
        {
            return (p, args, data) => Min2From(args.Select(a => p.Apply(a, data))).Select(a => a == null ? 0d : Convert.ToDouble(a)).Aggregate(reducer);
        }

        private static IEnumerable<object> Min2From(IEnumerable<object> source) 
        {
            var count = source.Count();
            if (count >= 2) return source;

            return new object[]{ null, count == 0 ? null : source.First() };
        }
    }
}
