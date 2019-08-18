namespace CMDASTView.Tests.DotTest

open System
open NUnit.Framework
open CMDASTView.DomainTypes
open CMDASTView.Dot

type TokenTest = { Input: string; Expected: string }

[<TestFixture>]
type TestClass () =

    [<Test>]
    member this.``Dot Test: Convert simple command to DOT`` () =

        let input = [
            """&&"""
            """  ("""
            """    for %a in (1 1 50) Do"""
            """      ("""
            """        &&"""
            """          Cmd: echo  Type: 0 Args: ` foo '"""
            """          Cmd: echo  Type: 0 Args: ` bar'"""
            """  Cmd: echo  Type: 0 Args: ` bazd'"""
        ]

        match (Dot.toDOT (input |> String.concat "\n")) with
        | Ok program ->
            printfn "%s" program
        | Error err ->
            printfn "Error converting AST to DOT: %A" err

        Assert.Fail()
