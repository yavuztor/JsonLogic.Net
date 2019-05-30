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
