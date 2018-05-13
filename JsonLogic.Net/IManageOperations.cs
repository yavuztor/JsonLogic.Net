using System;
using Newtonsoft.Json.Linq;

namespace JsonLogic.Net {
    public interface IManageOperations 
    {
        void AddOperation(string name, Func<IProcessJsonLogic, JToken[], object, object> operation);

        Func<IProcessJsonLogic, JToken[], object, object> GetOperation(string name);

        void DeleteOperation(string name);
    }
}