using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace JsonLogic.Net.UnitTests {
    public class JsonLogicTests {
        private readonly ITestOutputHelper _output;

        public JsonLogicTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        public static object data = Dynamic(d => {
            d.name = "John Doe";
            d.address = Dynamic(a => {
                a.street = "123 Main";
                a.city = "Gotham";
                a.zip = "33333";
            });
            d.luckyNumbers = new int[] { 3, 5, 7 };
            d.items = new object[0];
        });

        [Theory]
        [InlineData("{`==`: [1, 1]}", true)]
        [InlineData("{`==`: [{`var`: `name`}, `John Doe`]}", true)]
        [InlineData("{`===`: [1, 1]}", true)]
        [InlineData("{`===`: [{`var`: `name`}, `John Doe`]}", true)]

        [InlineData("{`!=`: [1, 1]}", false)]
        [InlineData("{`!=`: [{`var`: `name`}, `John Doe`]}", false)]
        [InlineData("{`!==`: [1, 1]}", false)]
        [InlineData("{`!==`: [{`var`: `name`}, `John Doe`]}", false)]

        [InlineData("{`+`: [0,0]}", 0d)]
        [InlineData("{`+`: [1,1]}", 2d)]
        [InlineData("{`+`: [-1, 1]}", 0d)]
        [InlineData("{`+`: [0.5, 0.5, 0.3]}", 1.3d)]
        [InlineData("{`+`: [1, 1.2]}", 2.2d)]
        [InlineData("{`+`: [3, `musketeers`]}", "3musketeers")]
        [InlineData("{`+`: [`musketeers`, 3]}", "musketeers3")]
        [InlineData("{`+`: 2}", 2d)]
        [InlineData("{`+`: -2}", -2d)]

        [InlineData("{`-`: [0,0]}", 0d)]
        [InlineData("{`-`: [0,1]}", -1d)]
        [InlineData("{`-`: [0,1,2]}", -3d)]
        [InlineData("{`-`: [1.5, 0.3]}", 1.2d)]
        [InlineData("{`-`: 2}", -2d)]
        [InlineData("{`-`: -2}", 2d)]

        [InlineData("{`*`: [0,0]}", 0d)]
        [InlineData("{`*`: [-1,1]}", -1d)]
        [InlineData("{`*`: [-2,1.5]}", -3d)]
        [InlineData("{`*`: [-2,-1.5]}", 3d)]

        [InlineData("{`/`: [-3,-1.5]}", 2d)]
        [InlineData("{`/`: [-3,1.5]}", -2d)]
        [InlineData("{`/`: [2,0]}", double.PositiveInfinity)]
        [InlineData("{`/`: [-2,0]}", double.NegativeInfinity)]

        [InlineData("{`%`: [3, 2]}", 1d)]
        [InlineData("{`%`: [3.3, 2]}", 3.3 % 2)]
        [InlineData("{`%`: [3.8, 2]}", 3.8 % 2)]
        [InlineData("{`%`: [3, 0]}", double.NaN)]

        [InlineData("{`max`: [1, 2, 3]}", 3d)]
        [InlineData("{`min`: [1, 2, 3]}", 1d)]

        [InlineData("{`<`: [1, 2, 3]}", true)]
        [InlineData("{`<`: [1, 1, 3]}", false)]
        [InlineData("{`<`: [1, 4, 3]}", false)]
        [InlineData("{`<=`: [1, 2, 3]}", true)]
        [InlineData("{`<=`: [1, 1, 3]}", true)]
        [InlineData("{`<=`: [1, 4, 3]}", false)]
        [InlineData("{`>`: [3, 2, 1]}", true)]
        [InlineData("{`>`: [3, 1, 1]}", false)]
        [InlineData("{`>`: [3, 4, 1]}", false)]
        [InlineData("{`>=`: [3, 2, 1]}", true)]
        [InlineData("{`>=`: [3, 1, 1]}", true)]
        [InlineData("{`>=`: [3, 4, 1]}", false)]

        [InlineData("{`var`: `name`}", "John Doe")]
        [InlineData("{`var`: `address.zip`}", "33333")]
        [InlineData("{`var`: [`nonexistent`, `default-value`]}", "default-value")]
        [InlineData("{`var`: `luckyNumbers.1`}", 5)]

        [InlineData("{`missing`:[`a`, `b`, `name`]}", new object[] { "a", "b" })]
        [InlineData("{`missing_some`:[2, [`a`, `b`, `name`]]}", new object[] { "a", "b" })]
        [InlineData("{`missing_some`:[1, [`a`, `b`, `name`]]}", new object[0])]

        [InlineData("{`and`: [true, true]}", true)]
        [InlineData("{`and`: [true, false]}", false)]
        [InlineData("{`and`: [false, true]}", false)]
        [InlineData("{`and`: [false, false]}", false)]
        [InlineData("{`and`: [{`==`: [5,5]}, {`<`: [3,5]}]}", true)]

        [InlineData("{`or`: [true, true]}", true)]
        [InlineData("{`or`: [false, true]}", true)]
        [InlineData("{`or`: [true, false]}", true)]
        [InlineData("{`or`: [false, false]}", false)]

        [InlineData("{`if`: [true, `yes`, `no`]}", "yes")]
        [InlineData("{`if`: [false, `yes`, `no`]}", "no")]
        [InlineData("{`if`: [false, `yes`, false, `maybe`, `no`]}", "no")]
        [InlineData("{`if`: [false, `yes`, true, `maybe`, `no`]}", "maybe")]
        [InlineData("{`if`: [true, `yes`, true, `maybe`, `no`]}", "yes")]

        [InlineData("{`map`: [{`var`:`luckyNumbers`}, {`*`: [{`var`:``}, 3]} ]}", new object[] { 9, 15, 21 })]
        [InlineData("{`filter`: [{`var`:`luckyNumbers`}, {`>`: [{`var`:``}, 5]} ]}", new object[] { 7 })]
        [InlineData("{`filter`: [{`var`:`luckyNumbers`}, {`>=`: [{`var`:``}, 5]} ]}", new object[] { 5, 7 })]
        [InlineData("{`reduce`: [{`var`:`luckyNumbers`}, {`+`: [{`var`:`current`}, {`var`:`accumulator`}]}, 10 ]}", 25d)]
        [InlineData("{`all`: [{`var`:`luckyNumbers`}, {`>`: [{`var`:``}, 1]} ]}", true)]
        [InlineData("{`none`: [{`var`:`luckyNumbers`}, {`<=`: [{`var`:``}, 1]} ]}", true)]
        [InlineData("{`none`: [{`var`:`luckyNumbers`}, {`<=`: [{`var`:``}, 3]} ]}", false)]
        [InlineData("{`some`: [{`var`:`luckyNumbers`}, {`<=`: [{`var`:``}, 3]} ]}", true)]
        [InlineData("{`some`:[{`var`:`luckyNumbers`},{`==`:[{`var`:``},3]}]}", true)]
        [InlineData("{`merge`:[ [1,2], [3,4] ]}", new object[] { 1, 2, 3, 4 })]
        [InlineData("{`merge`:[ 1,2, [3,4] ]}", new object[] { 1, 2, 3, 4 })]
        [InlineData("{`in`:[ 1, [3,4,1] ]}", true)]
        [InlineData("{`in`:[ 2, [3,4,1] ]}", false)]

        [InlineData("{`in`: [`Spring`, `Springfield`]}", true)]
        [InlineData("{`in`: [`Springs`, `Springfield`]}", false)]
        [InlineData("{`in`: [`spring`, `Springfield`]}", false)]
        [InlineData("{`cat`: [`spring`, `field`]}", "springfield")]
        [InlineData("{`substr`: [`springfield`, 6]}", "field")]
        [InlineData("{`substr`: [`springfield`, 6, 3]}", "fie")]
        [InlineData("{`substr`: [`springfield`, -3]}", "eld")]
        [InlineData("{`log`: `apple`}", "apple")]

        [InlineData("{`all`:[{`var`:`items`},{`>=`:[{`var`:`qty`},1]}]}", false)]
        public void Apply(string argsJson, object expectedResult) {
            // Arrange
            var rules = JsonFrom(argsJson);
            var jsonLogic = new JsonLogicEvaluator(EvaluateOperators.Default);

            _output.WriteLine($"{MethodBase.GetCurrentMethod().Name}() Testing {rules} against {data}");

            // Act
            var result = jsonLogic.Apply(rules, data);

            // Assert
            if (expectedResult is Array) {
                string[] expectedResultArr = (expectedResult as Array).Cast<object>().Select(i => i.ToString()).ToArray();
                string[] resultArr;

                if (result is Array)
                    resultArr = (result as Array).Cast<object>().Select(i => i.ToString()).ToArray();
                else if (result is IEnumerable<object>)
                    resultArr = (result as IEnumerable<object>).Select(i => i.ToString()).ToArray();
                else
                    throw new Exception("Cannot cast resultArr");

                Assert.Equal(expectedResultArr, resultArr);
            } else {
                Assert.Equal(expectedResult, result);
            }
        }

        [Theory]
        [InlineData("{`-`: [2,`something`]}", typeof(FormatException))]
        public void ApplyThrowsException(string rulesJson, Type exceptionType) {
            // Arrange
            var rules = JsonFrom(rulesJson);
            var jsonLogic = new JsonLogicEvaluator(EvaluateOperators.Default);
            object result = null;

            _output.WriteLine($"{MethodBase.GetCurrentMethod().Name}() Testing {rules} against {data}");

            // Act & Assert
            try
            {
                result = jsonLogic.Apply(rules, data);
            } catch (Exception e) {
                Assert.Equal(exceptionType, e.GetType());
            } finally {
                Assert.Null(result);
            }
        }

        [Fact]
        public void SimpleUseCase() {
            // Arrange
            string jsonText = "{\">\": [{\"var\": \"MyNumber\"}, 3]}";
            var rule = JObject.Parse(jsonText);
            object localData = new { MyNumber = 8 };
            var evaluator = new JsonLogicEvaluator(EvaluateOperators.Default);

            _output.WriteLine($"{MethodBase.GetCurrentMethod().Name}() Testing {rule} against {localData}");

            // Act
            var result = evaluator.Apply(rule, localData);

            // Assert
            Assert.True((bool) result);
        }

        [Fact]
        public void Issue2_Var_In_With_JObject()
        {
            // Arrange
            string ruleJson = "{`in`:[{`var`:`marital_status`},[`Single`,`Married`,`Divorced`,`Widowed`,`Separated`]]}".Replace('`', '"');
            string dataJson = "{`marital_status`: `Divorced`}".Replace('`', '"');
            var rule = JObject.Parse(ruleJson);
            var localData = JObject.Parse(dataJson);
            var evaluator = new JsonLogicEvaluator(EvaluateOperators.Default);

            _output.WriteLine($"{MethodBase.GetCurrentMethod().Name}() Testing {rule} against {localData}");

            // Act
            var result = evaluator.Apply(rule, localData);

            // Assert
            Assert.True((bool) result);
        }

        [Fact]
        public void ConjunctExpression() {
            // Arrange
            string dataJson = "{ `temp` : 100, `pie` : { `filling` : `apple` } }".Replace('`', '"');
            string ruleJson = "{ `and` : [  {`<` : [ { `var` : `temp` }, 110 ]},  {`==` : [ { `var` : `pie.filling` }, `apple` ] }] }".Replace('`', '"');
            var evaluator = new JsonLogicEvaluator(EvaluateOperators.Default);
            var rule = JObject.Parse(ruleJson);
            var localData = JObject.Parse(dataJson);

            _output.WriteLine($"{MethodBase.GetCurrentMethod().Name}() Testing {rule} against {localData}");

            // Act
            var result = evaluator.Apply(rule, localData);

            // Assert
            Assert.True((bool) result);
        }

        [Fact]
        public void NestedFilterVariableAccess()
        {
            // Arrange
            string dataJson = "{`parentArray`:[{`childArray`:[1,2,3,4,5],`childItem`:`a`},{`childArray`:[4,5],`childItem`:`b`},{`childArray`:[5,6,7,8],`childItem`:`c`}]}".Replace('`', '"');
            string ruleJson = "{`filter`:[{`var`:`parentArray`},{`and`:[{`===`:[{`var`:`childItem`},`c`]},{`filter`:[{`var`:`childArray`},{`===`:[{`var`:``},5]}]}]}]}".Replace('`', '"');
            string expectedJson = "[{`childArray`:[5,6,7,8],`childItem`:`c`}]".Replace('`', '"');
            var evaluator = new JsonLogicEvaluator(EvaluateOperators.Default);
            var rule = JsonFrom(ruleJson);
            var localData = JsonFrom(dataJson);
            var expectedResult = JsonFrom(expectedJson);

            _output.WriteLine($"{MethodBase.GetCurrentMethod().Name}() Testing {rule} against {localData}");

            // Act
            var result = evaluator.Apply(rule, localData);

            // Assert
            Assert.Equal(expectedResult, result);
        }


        [Theory]
        [InlineData("{`==`: [1,`1`]}", true)]
        [InlineData("{`===`: [1,`1`]}", false)]
        [InlineData("{`==`: [1,1]}", true)]
        [InlineData("{`===`: [1,1]}", true)]
        [InlineData("{`==`: [1,2]}", false)]
        [InlineData("{`===`: [1,2]}", false)]
        [InlineData("{`==`: [1,null]}", false)]
        [InlineData("{`===`: [1,null]}", false)]
        [InlineData("{`==`: [null,null]}", true)]
        [InlineData("{`===`: [null,null]}", true)]
        [InlineData("{`!=`: [1,`1`]}", false)]
        [InlineData("{`!==`: [1,`1`]}", true)]
        [InlineData("{`!=`: [1,1]}", false)]
        [InlineData("{`!==`: [1,1]}", false)]
        [InlineData("{`!=`: [1,2]}", true)]
        [InlineData("{`!==`: [1,2]}", true)]
        [InlineData("{`!=`: [1,null]}", true)]
        [InlineData("{`!==`: [1,null]}", true)]
        [InlineData("{`!=`: [null,null]}", false)]
        [InlineData("{`!==`: [null,null]}", false)]
        public void EqualityAndInequalityHandling(string ruleJson, object expectedResult)
        {
            var rule = JsonFrom(ruleJson);
            var jsonLogic = new JsonLogicEvaluator(EvaluateOperators.Default);

            _output.WriteLine($"{MethodBase.GetCurrentMethod().Name}() Testing {rule}");

            // Act
            var actualResult = jsonLogic.Apply(rule, data);
            Assert.Equal(expectedResult, actualResult);
        }


        [Theory]
        [InlineData("{`var`:`string`}", "{`string`:`a`}", "a")]
        [InlineData("{`var`:[`string`]}", "{`string`:`a`}", "a")]
        [InlineData("{`var`:`number`}", "{`number`:10.3}", 10.3)]
        [InlineData("{`var`:[`number`]}", "{`number`:10.3}", 10.3)]
        [InlineData("{`var`:`boolean`}", "{`boolean`:true}", true)]
        [InlineData("{`var`:[`boolean`]}", "{`boolean`:true}", true)]
        [InlineData("{`var`:`nullref`}", "{`nullref`:null}", null)]
        [InlineData("{`var`:[`nullref`]}", "{`nullref`:null}", null)]
        [InlineData("{`var`:[`nullrefWithDefaultString`,`a`]}", "{}", "a")]
        [InlineData("{`var`:[`nullrefWithDefaultNumeric`,5.2]}", "{}", 5.2)]
        [InlineData("{`var`:[`nullrefWithDefaultBoolean`,true]}", "{}", true)]
        [InlineData("{`var`:`nested.variable`}", "{`nested`:{ `variable`: `alpha` }}", "alpha")]
        [InlineData("{`var`:[`nested.variable`]}", "{`nested`:{ `variable`: `alpha` }}", "alpha")]
        [InlineData("{`var`:3}", "[`a`,`b`,`c`,`d`]", "d")]
        [InlineData("{`var`:`arrayByNumericIndexNested.2`}", "{`arrayByNumericIndexNested`: [`a`,`b`,`c`,`d`]}", "c")]
        [InlineData("{`var`:[`nullValueWithDefault`,100]}", "{`nullValueWithDefault`:null}", null)]
        [InlineData("{`var`:[100,`defaultValue`]}", "[`a`,`b`,`c`,`d`]", "defaultValue")]
        [InlineData("{`var`:100}", "[`a`,`b`,`c`,`d`]", null)]
        public void VariableHandling(string ruleJson, string dataJson, object expectedResult)
        {
            var rule = JsonFrom(ruleJson);
            var localData = JsonFrom(dataJson);
            var jsonLogic = new JsonLogicEvaluator(EvaluateOperators.Default);

            _output.WriteLine($"{MethodBase.GetCurrentMethod().Name}() Testing {rule} against {localData}");
            // Act
            var actualResult = jsonLogic.Apply(rule, localData);
            Assert.Equal(expectedResult, actualResult);
        }



        [Fact]
        public void PassesJsonLogicTests() {
            // Arrange
            var tests = JArray.Parse(System.IO.File.ReadAllText("tests.json"));
            var evaluator = new JsonLogicEvaluator(EvaluateOperators.Default);

            var results = tests.Where(t => t is JArray).Select(t => {
                var test = t as JArray;
                var rule = test[0];
                object data = GetDataObject(test[1]);
                var expectedResult = GetDataObject(test[2]);
                object result = null;
                
                try {
                    result = evaluator.Apply(rule, data);
                    Assert.Equal(expectedResult, result);
                    return new TestDef(null, null, null);
                } catch (Exception e) {
                    return new TestDef(test, result, e);
                }
            });

            // Act
            var failures = results.Where(t => t.Test != null);

            // Assert
            _output.WriteLine("Failures:\n\t" + string.Join("\n\n\t", failures.Select(f => f.Test.ToString(Formatting.None) + " -> " + f.Expected.ToString())));
            Assert.Empty(failures);
        }

        [Fact]
        public void Issue3_FilterBehaviorTest()
        {
            var localData = JsonConvert.DeserializeObject(@"[
                {
                    `Prop1`: {
                    `PropA`: 5
                    },
                    `Prop2`: {
                    `PropB`: 18
                    }
                },
                {
                    `Prop1`: {
                    `PropA`: 1
                    },
                    `Prop2`: {
                    `PropB`: 35
                    }
                }
            ]".Replace('`', '"'));

            var rules = JObject.Parse(@"{
                `filter`: [
                    { `var`: `` },
                    {
                    `and`: [
                        {
                        `>=`: [
                            { `var`: `Prop1.PropA` },
                        1
                        ]
                        },
                        {
                        `>=`: [
                            { `var`: `Prop2.PropB` },
                            19
                        ]
                        }
                    ]
                    }
                ]
                }".Replace('`', '"'));
            var evaluator = new JsonLogicEvaluator(EvaluateOperators.Default);

            _output.WriteLine($"{MethodBase.GetCurrentMethod().Name}() Testing {rules} against {localData}");

            // Act
            var result = evaluator.Apply(rules, localData);

            // Assert
            Assert.Equal((result as object[]).Length, 1);
            

        }

        private object GetDataObject(JToken token)
        {
            if (token is JValue) return CastPrimitive((token as JValue).Value);
            if (token is JArray) return (token as JArray).Select(t => GetDataObject(t)).ToArray();
            if (token is JObject) return (token as JObject).Properties().Aggregate(new Dictionary<string, object>(), (d, p) => { 
                d.Add(p.Name, GetDataObject(p.Value)); 
                return d;
            });
            throw new Exception("GetDataObject cannot handle token " + token.ToString());
        }

        private object CastPrimitive(object value) 
        {
            if (value is int || value is short || value is long || value is double || value is float || value is decimal) return Convert.ToDouble(value);
            return value;
        }

        private object GetValue(JToken token) {
            if (token is JValue) return (token as JValue).Value;
            if (token is JArray) return (token as JArray).ToArray();
            if (token is JObject) return (token as JObject).ToObject<dynamic>();
            throw new Exception("Cannot get value of this token: " + token.ToString(Formatting.None));
        }

        public static JToken JsonFrom(string input) {
            return JToken.Parse(input.Replace('`', '"'));
        }

        public static object Dynamic(Action<dynamic> ctor) {
            var value = new System.Dynamic.ExpandoObject();
            ctor(value);
            return value;
        }
    }

    internal class TestDef 
    {
        public TestDef(JToken test, object result, object expected)
        {
            this.Test = test;
            this.Result = result;
            this.Expected = expected;
        }

        public object Expected { get; private set; }

        public object Result { get; private set; }

        public JToken Test { get; private set; }
    }

}