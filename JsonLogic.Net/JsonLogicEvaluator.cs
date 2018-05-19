using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JsonLogic.Net
{
    public class JsonLogicEvaluator : IProcessJsonLogic
    {
        private IManageOperators operations;

        public JsonLogicEvaluator(IManageOperators operations)
        {
            this.operations = operations;
        }

        public object Apply(JToken rule, object data)
        {
            if (rule is null) return null;
            if (rule is JValue) return AdjustType((rule as JValue).Value);
            if (rule is JArray) return (rule as JArray).Select(r => Apply(r, data)).ToArray();

            var ruleObj = (JObject) rule;
            var p = ruleObj.Properties().First();
            var opName = p.Name;
            var opArgs = (p.Value is JArray) ? (p.Value as JArray).ToArray() : new JToken[] { p.Value };
            var op = operations.GetOperator(opName);
            return op(this, opArgs, data);
        }

        private object AdjustType(object value)
        {
            return value.IsNumeric() ? Convert.ToDouble(value) : value;
        }
    }
}
