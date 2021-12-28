# MathCode Example

## Do calculations with wolfram engine:

### simple formula

```math
2*3*7
```

Define some variables:

```math
a = 3;
b = 7;
F[x_,y_]:=Sin[x] + Cos[y];
```

### hide the code

Result of 2*a*b is:

```math0
2*a*b
```

Use "\`\`\`math0" instead of "\`\`\`math" to hide the code.

### hide the result

I don't want to tell you the result of

```math/null
c = 2*a*b
```

Unless you really want to know ...

```math
c
```

## Multimedia output

### make a plot

```math
Plot[x,{x,-1,1}]
```

### or sound:

```math
Sound[SoundNote["G",1,"Violin"]]
```


### or even animation:

```math
Table[Plot[a Sin[x] Cos[t], {x, -2 Pi, 2 Pi}, PlotRange -> {-a, a}], {t, -Pi, Pi, 0.1}]
```

## result format

### Try mathml for formulae

```math/mathml
Exp[c*\[Mu]+\[CapitalDelta]]
```

### and svg for general output:

```math/svg
Table[Plot[a Sin[x] Cos[t], {x, -2 Pi, 2 Pi}, PlotRange -> {-a, a}], {t, -Pi, Pi, 0.5}]
```

You can also try png/jpg/...

## Use the raw output

### Use raw for numbers

```math/raw
c+1
```

### or make html by your self

```math/raw
head = "<img src=\"data:image/gif;base64,";
content = ExportString[ExportString[Plot[Sin[x],{x,-Pi,Pi}],"png"],"Base64"];
tail = "\"/>";
StringJoin[head,content,tail]
```




Have fun~
