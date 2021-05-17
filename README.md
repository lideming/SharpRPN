# SharpRPN

SharpRPN is a stack machine with an [RPN](https://en.wikipedia.org/wiki/Reverse_Polish_notation) programming language,
inspired by [RPL](https://en.wikipedia.org/wiki/RPL_(programming_language)) on HP calculators.

![SharpRPN image](https://user-images.githubusercontent.com/14901890/114175312-76962080-996c-11eb-9dd5-0a2e17d1b7dc.png)

Use as a calculator:
```
Input: 1 2 + 3 4 + *
===StackBegin===
1:      21      Int32
====StackEnd====
Input: 1 2 3 * 4 + +
===StackBegin===
1:      11      Int32
2:      21      Int32
====StackEnd====
```

Drop an item from stack or clear all items:

```
===StackBegin===
1:      11      Int32
2:      21      Int32
====StackEnd====
Input: drop
===StackBegin===
1:      21      Int32
====StackEnd====
Input: clear
===StackBegin===
====StackEnd====
```

Set value to variable:
```
Input: 1 'foo' sto
===StackBegin===
====StackEnd====
Input: foo
===StackBegin===
1:      1       Int32
====StackEnd====
```

Codeblocks:
```
Input: { 'hello world!' 1 2 + }
===StackBegin===
1:      { 'hello world!' 1 2 + }        CodeBlock
====StackEnd====
Input: eval
===StackBegin===
1:      3       Int32
2:      hello world!    String
====StackEnd====
```

Input and condition:
```
Input: { 'Correct\n' print } { 'Wrong\n' print } "What's the answer? " print input '42' == evalifelse
What's the answer? 42
Correct
```


## Try it now

### Build and run from source

(.NET Core 3.1 SDK required)

```shell
git clone https://github.com/lideming/SharpRPN.git
cd SharpRPN
dotnet run
```

### Run prebuilt binaries

(.NET Core 3.1 Runtime required)

Download the latest build from [Github Actions](https://github.com/lideming/SharpRPN/actions).
