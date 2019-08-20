# CMDASTView: Abstract Syntax Tree Renderer

## About

In November 2018 FireEye published a [threat research article](https://www.fireeye.com/blog/threat-research/2018/11/cmd-and-conquer-de-dosfuscation-with-flare-qdb.html) that introduced a utility (De-Dosfuscator) which hooks ~CMD.EXE~, dumping obfuscated instructions in-the-clear.  As part of their research, FireEye discovered a debug flag: `fDumpParse` which is a hidden debug mode for `CMD` that exports the Abstract Syntax Tree (AST) of the parsed command.

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

![Example CMDASTView Output](https://github.com/bobbystacksmash/CMD-AST-View/blob/master/examples/images/ex3.ast.png)

## Building

`CMD-AST-View` is written in [F#](https://fsharp.org/), making it cross-platform thanks to [.Net Core](https://dotnet.microsoft.com/download).  Ensure you have the `dotnet` command line utility installed before continuing, then:

1. `git clone https://github.com/bobbystacksmash/CMD-AST-View.git`
2. `cd CMD-AST-View/CMDASTView`
3. `dotnet test`
4. `cd src/cmdast2dot`

From here you may build the `cmdast2dot` CLI for your platform of choice, using `dotnet publish` and the ["runtime identifier"](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) (RID) for your target platform, for example:

| OS | Build CMD |
|-----|------------|
| `win7-x64` | `dotnet publish -c release -r win7-x64` |
| `win10-x64` | `dotnet publish -c release -r win7-x64` |
| `linux-x64` | `dotnet publish -c release -r linux-x64` |
| `linux-arm` | `dotnet publish -c release -r linux-arm` |
| `osx.10.14-x64` | `dotnet publish -c release -r osx.10.14-x64` |

Once you've build your binary, assuming you're still in `src/cmdast2dot`, the binaries can be found at:

 * `bin/release/netcoreapp2.2`
