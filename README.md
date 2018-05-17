# JsonLogic.Net - JsonLogic implementation in .Net platform

JsonLogic.Net is an implementation of [JsonLogic](http://jsonlogic.com/) in .Net platform. Implementation includes [all supported operators documented on JsonLogic site](http://jsonlogic.com/operations.html).

## Dependencies

JsonLogic.Net has a hard dependency on [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/). It is a common library for json handling and very easy to use.

## Usage

```csharp
// The data that the rule will run against. 
object data = new {MyNumber = 8};

// Rule definition retrieved as JSON text
string jsonText="{\">\": [{\"var\": \"MyNumber\"}, 3]}";

// Parse json into hierarchical structure
var rule = JObject.Parse(jsonText);

// Create an evaluator with default operators.
var evaluator = new JsonLogicEvaluator(EvaluateOperators.Default);

// Apply the rule to the data.
object result = evaluator.Apply(rule, data);
```

The evaluator does not maintain any state and is thread-safe. Depending on your use case, you can add the evaluator as a singleton in your dependency injection container. You can also have multiple evaluators with different custom operations.
