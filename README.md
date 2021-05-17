# SharpRPN

SharpRPN is a stack machine with an [RPN](https://en.wikipedia.org/wiki/Reverse_Polish_notation) programming language,
inspired by [RPL](https://en.wikipedia.org/wiki/RPL_(programming_language)) on HP calculators.

![SharpRPN image](assets/demo.svg)

Use as a calculator:
```
Input: 1
                     
  # │ Value │ Type   
 ───┼───────┼─────── 
  1 │ 1     │ Int32  
                     
Input: 2
                     
  # │ Value │ Type   
 ───┼───────┼─────── 
  2 │ 1     │ Int32  
  1 │ 2     │ Int32  
                     
Input: +
                     
  # │ Value │ Type   
 ───┼───────┼─────── 
  1 │ 3     │ Int32  
                     
Input: 3 4 + *
                     
  # │ Value │ Type   
 ───┼───────┼─────── 
  1 │ 21    │ Int32  
                     
Input: 1 2 3 * 4 + +
                     
  # │ Value │ Type   
 ───┼───────┼─────── 
  2 │ 21    │ Int32  
  1 │ 11    │ Int32  
```

Drop an item from stack or clear all items:

```
  # │ Value │ Type   
 ───┼───────┼─────── 
  2 │ 21    │ Int32  
  1 │ 11    │ Int32  
                     
Input: drop
                     
  # │ Value │ Type   
 ───┼───────┼─────── 
  1 │ 21    │ Int32  
                     
Input: clear
Stack empty
```

Set value to variable:
```
Input: 1 'foo' sto
Stack empty
Input: foo
                     
  # │ Value │ Type   
 ───┼───────┼─────── 
  1 │ 1     │ Int32  
```

Codeblocks:
```
Input: { 'hello world!' 1 2 + }
                                            
  # │ Value                    │ Type       
 ───┼──────────────────────────┼─────────── 
  1 │ { 'hello world!' 1 2 + } │ CodeBlock  
                                            
Input: eval
                             
  # │ Value        │ Type    
 ───┼──────────────┼──────── 
  2 │ hello world! │ String  
  1 │ 3            │ Int32   
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

### Run in Docker [![Docker Image Size (tag)](https://img.shields.io/docker/image-size/yuuza/sharprpn/latest?label=yuuza%2Fsharprpn%3Alatest)](https://hub.docker.com/r/yuuza/sharprpn)

```shell
docker run -it yuuza/sharprpn
```
