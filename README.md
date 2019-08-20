# CMDASTView: Abstract Syntax Tree Renderer

## About

In November 2018 FireEye published a [threat research article](https://www.fireeye.com/blog/threat-research/2018/11/cmd-and-conquer-de-dosfuscation-with-flare-qdb.html) that introduced a utility (De-Dosfuscator) which hooks `CMD.EXE`, dumping obfuscated instructions in-the-clear.  As part of their research, FireEye discovered a debug flag: `fDumpParse` which is a hidden debug mode for `CMD` that exports the Abstract Syntax Tree (AST) of the parsed command.

This project is a rough-and-ready F# parser designed to convert the AST exported from `fDumpParse` in to a GraphViz drawing, making CMD's AST far easier to read.

Given the following input:

```
  (for %a in (1 1 50) Do (echo foo && echo bar)) && echo baz
```
`fDumpParse` produces the AST:
```
  &&
    (
      for %a in (1 1 50) Do
        (
          &&
            Cmd: echo  Type: 0 Args: ` foo '
            Cmd: echo  Type: 0 Args: ` bar'
    Cmd: echo  Type: 0 Args: ` baz'
```
which `CMD-AST-View` then converts in to a [GraphViz](https://www.graphviz.org/) drawing, which hopefully presents the AST in an easier-to-read way:

![Example CMDASTView Output](https://github.com/bobbystacksmash/CMD-AST-View/blob/master/examples/images/ex2.ast.png)

## Building

`CMD-AST-View` is written in [F#](https://fsharp.org/), making it cross-platform thanks to [.Net Core](https://dotnet.microsoft.com/download).  Ensure you have the `dotnet` command line utility installed before continuing, then:

1. `git clone https://github.com/bobbystacksmash/CMD-AST-View.git`
2. `cd CMD-AST-View/CMDASTView`
3. `dotnet test`
4. `cd src/cmdast2dot`

From here you may build the `cmdast2dot` CLI for your platform of choice using `dotnet publish` and the ["runtime identifier"](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) (RID) for your target platform, for example:

| OS | Build CMD |
|-----|------------|
| `win7-x64` | `dotnet publish -c release -r win7-x64` |
| `win10-x64` | `dotnet publish -c release -r win10-x64` |
| `linux-x64` | `dotnet publish -c release -r linux-x64` |
| `linux-arm` | `dotnet publish -c release -r linux-arm` |
| `osx.10.14-x64` | `dotnet publish -c release -r osx.10.14-x64` |

Once you've built your binary, assuming you're still in `src/cmdast2dot`, the binaries can be found at:

 * `bin/release/netcoreapp2.2`
 
 ## Usage

Ensure the compiled binary is in your path, and then it should be possible to pipe a `CMD.EXE` AST directly in to `cmdast2dot`.  All being well, `cmdast2dot` should produce a valid GraphViz drawing, which can then be converted in to an image format.

For my workflow, I use De-Dosfuscator with `fDumpParse` to generate the AST, which I write to a text file.  I then pipe the text file in to `cmdast2dot`, piping the output of *that* in to DOT:

```
cat ast.txt | cmdast2dot | tee | dot -Tpng -o ast.png
```
Where the contents of `ast.txt` contains the following AST that was generated from the expression: `IF "this"=="that" (echo foo && echo bar) else (echo baz)`:
```cmd
IF
  Cmd: "this"  Type: 39 Args: `"that"'
  (
    &&
      Cmd: echo  Type: 0 Args: ` foo '
      Cmd: echo  Type: 0 Args: ` bar'
else
  (
    Cmd: echo  Type: 0 Args: ` baz'
```

A small amount of GraphViz fu using the DOT render `dot -Tpng -o ast.png` produces the following graphic:
![AST output](https://github.com/bobbystacksmash/CMD-AST-View/blob/master/examples/images/ex3.ast.png)
