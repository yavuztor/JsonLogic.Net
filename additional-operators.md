# Additional operators

JsonLogic operators cover only the essentials, but there might be a need to provide some extra operators out of the box, to make life easier for developers. 

## within

The number comparisons might not lend themselves to strict equality check, due to floating point arithmetic, which could result in a really close but not exactly same number as you might think. This operator helps in these cases. it takes 3 arguments. First two are the numbers to compare and the last argument is the precision. 

This operator will return true if the first two arguments are closer than the precision value. So, it will return true for this case:

```json
{"within": [
    58.849,
    58.849000000000004,
    0.000001 
]}
```

It will return false for this case:
```json
{"within": [
    58.849,
    58.849100000000004,
    0.000001 
]}
```

## local

Complex rules can make it difficul to sequentially operate on results of previous rules. In particular, lack of scope for **var** can be restrictive, since under most conditions (except several array operations) it only operates on the contents of the entire data object. Example could be filtering the array with **filter**, and then accessing a specific field form the first valid match from the resultant object. This can be done with a complex combination of **filter**, **reduce**, **var** and conditions. 

Instead, to simplify processing, a new operator **local** is introduced. 
**local** accepts two positional arguments:
* **source** - logic applied to the entire data block as normal that retrieves the data for **logic** argument to operate on. This evaluates as a regular rule and its results are chained into the second argument
* **logic** - logic applied to the results of the first, **source** argument only. **var** evaluations inside will be scoped to the results of **source** rather than the entire data object

This permits the following logic constructs:

Given data object:
```json
{
    "orchards": [
        {
            "name": "Sunny Fruits Orchard", 
            "apple": 12,
            "pear": 20
        },
        {
            "name": "Nature's Garden Orchard", 
            "cherry": 5,
            "pear": 25
        }
    ]
}
```

If we want to find out the name of the first orchard that has more than 20 pear trees, we can now do:
```json
{
    "local": [
        {
            "filter": [
                { "var": [ "orchards" ] },
                { ">": [
                    { "var": "pear" },
                    20
                ]}
            ]
        },
        {
            "var": "0.name"
        }
    ]
}
```

Without local, to achieve the same we'd need to use this:
```json
{
    "reduce": [
        {
            "filter": [
                { "var": [ "orchards" ] },
                { ">": [
                    { "var": "pear" },
                    20
                ]}
            ]
        },
        {
            "if": [
                {"var": "accumulator"},
                {"var": "accumulator"},
                {"var": "current.name"}
            ]
        },
        null
    ]
}
```
Logic quickly becomes complicated and nested array accessors (imagine if trees in each orchard were inside their own arrays?) become difficult to follow and interpret. Imagine if we wanted to ask the question about second orchard that satisfies the condition? Easy to do with **local** but starting to require accumulation of state in **reduce**.