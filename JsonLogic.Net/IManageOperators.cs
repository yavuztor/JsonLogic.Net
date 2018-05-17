using System;
using Newtonsoft.Json.Linq;

namespace JsonLogic.Net {
    public interface IManageOperators 
    {
        void AddOperator(string name, Func<IProcessJsonLogic, JToken[], object, object> operation);

        Func<IProcessJsonLogic, JToken[], object, object> GetOperator(string name);

        void DeleteOperator(string name);
    }
}