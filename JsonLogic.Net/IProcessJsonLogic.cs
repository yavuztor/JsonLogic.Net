using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonLogic.Net
{
    public interface IProcessJsonLogic 
    {
        object Apply(JToken rule, object data);
    }
}
