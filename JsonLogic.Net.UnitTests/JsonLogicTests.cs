using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace JsonLogic.Net.UnitTests
{
    public class JsonLogicTests
    {
        public static object data = Dynamic(d => {
            d.name = "John Doe";
            d.address = Dynamic(a => {
                a.street = "123 Main";
                a.city = "Gotham";
                a.zip = "33333";
            });
            d.luckyNumbers= new int[]{ 3, 5, 7 };
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

        [InlineData("{`missing`:[`a`, `b`, `name`]}", new object[]{"a", "b"})]
        [InlineData("{`missing_some`:[2, [`a`, `b`, `name`]]}", new object[]{"a", "b"})]
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

        [InlineData("{`map`: [{`var`:`luckyNumbers`}, {`*`: [{`var`:``}, 3]} ]}", new object[]{9, 15, 21})]
        [InlineData("{`filter`: [{`var`:`luckyNumbers`}, {`>`: [{`var`:``}, 5]} ]}", new object[]{7})]
        [InlineData("{`filter`: [{`var`:`luckyNumbers`}, {`>=`: [{`var`:``}, 5]} ]}", new object[]{5, 7})]
        [InlineData("{`reduce`: [{`var`:`luckyNumbers`}, {`+`: [{`var`:`current`}, {`var`:`accumulator`}]}, 10 ]}", 25d)]
        [InlineData("{`all`: [{`var`:`luckyNumbers`}, {`>`: [{`var`:``}, 1]} ]}", true)]
        [InlineData("{`none`: [{`var`:`luckyNumbers`}, {`<=`: [{`var`:``}, 1]} ]}", true)]
        [InlineData("{`none`: [{`var`:`luckyNumbers`}, {`<=`: [{`var`:``}, 3]} ]}", false)]
        [InlineData("{`some`: [{`var`:`luckyNumbers`}, {`<=`: [{`var`:``}, 3]} ]}", true)]
        [InlineData("{`merge`:[ [1,2], [3,4] ]}", new object[]{1, 2, 3, 4})]
        [InlineData("{`merge`:[ 1,2, [3,4] ]}", new object[]{1, 2, 3, 4})]
        [InlineData("{`in`:[ 1, [3,4,1] ]}", true)]
        [InlineData("{`in`:[ 2, [3,4,1] ]}", false)]
        
        [InlineData("{`in`: [`Spring`, `Springfield`]}", true)]
        [InlineData("{`in`: [`Springs`, `Springfield`]}", false)]
        [InlineData("{`in`: [`spring`, `Springfield`]}", false)]
        [InlineData("{`cat`: [`spring`, `field`]}", "springfield")]
        [InlineData("{`substr`: [`springfield`, 6]}", "field")]
        [InlineData("{`substr`: [`springfield`, 6, 3]}", "fie")]
        [InlineData("{`substr`: [`springfield`, -3]}", "eld")]
        public void Apply(string argsJson, object expectedResult) 
        {
            // Arrange
            var rules = JsonFrom( argsJson );
            var jsonLogic = new JsonLogicEvaluator();
            
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
            }
            else {
                Assert.Equal(expectedResult, result);
            }
        }

        [Theory]
        [InlineData("{`-`: [2,`something`]}", typeof(FormatException))]
        public void ApplyThrowsException(string rulesJson, Type exceptionType)
        {
            // Arrange
            var rules = JsonFrom(rulesJson);
            var jsonLogic = new JsonLogicEvaluator();
            object result = null;
            
            // Act & Assert
            try {
                result = jsonLogic.Apply(rules, data);
            }
            catch (Exception e) {
                Assert.True(exceptionType.IsAssignableFrom(e.GetType()));
            }
            finally {
                Assert.Equal(null, result);
            }
        }
        public static JObject JsonFrom(string input) {
            return JObject.Parse(input.Replace('`', '"'));
        }

        public static object Dynamic(Action<dynamic> ctor) 
        {
            var value = new System.Dynamic.ExpandoObject();
            ctor(value);
            return value;
        }
    }
    
}
