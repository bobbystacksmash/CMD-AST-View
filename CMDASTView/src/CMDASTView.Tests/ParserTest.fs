namespace CMDASTView.Tests.ParserTest

open System
open NUnit.Framework
open CMDASTView.DomainTypes
open CMDASTView.Parser

type TokenTest = { Input: string; Expected: ASTNode }

[<TestFixture>]
type TestClass () =

    let compare test =
        match Parser.parse test.Input with
        | Ok output -> Assert.AreEqual(output, test.Expected)
        | Error err -> Assert.Fail()


    [<Test>]
    member this.``Parser Test: Command Conversion`` () =

        let tests = [
            { Input    = "Cmd: echo  Type: 0 Args: ` foobar '";
              Expected = LeafNode ({ Program = "echo"; Args = "foobar" }) }
        ]
        tests |> List.iter compare


    [<Test>]
    member this.``Parser Test: Parse Operator: '&'`` () =

        let input = [
            """&"""
            """  Cmd: echo  Type: 0 Args: ` foo '"""
            """  Cmd: echo  Type: 0 Args: ` bar'"""
        ]

        let expected = BinaryOperatorNode (Always,
                                           LeafNode { Program = "echo"; Args = "foo" },
                                           LeafNode { Program = "echo"; Args = "bar" })

        match Parser.parse (input |> String.concat "\n") with
        | Ok ast -> Assert.AreEqual(ast, expected)
        | Error err ->
            printfn "ERROR: %A" err
            Assert.Fail()

    [<Test>]
    member this.``Parser Test: Parse Operator: '&&'`` () =

        let input = [
            """&&"""
            """  Cmd: echo  Type: 0 Args: ` foo '"""
            """  Cmd: echo  Type: 0 Args: ` bar'"""
        ]

        let expected = BinaryOperatorNode (Success,
                                           LeafNode { Program = "echo"; Args = "foo" },
                                           LeafNode { Program = "echo"; Args = "bar" })

        match Parser.parse (input |> String.concat "\n") with
        | Ok ast -> Assert.AreEqual(ast, expected)
        | Error err ->
            printfn "ERROR: %A" err
            Assert.Fail()

    [<Test>]
    member this.``Parser Test: Parse Operator: '||'`` () =

        let input = [
            """||"""
            """  Cmd: echo  Type: 0 Args: ` foo '"""
            """  Cmd: echo  Type: 0 Args: ` bar'"""
        ]

        let expected = BinaryOperatorNode (Or,
                                           LeafNode { Program = "echo"; Args = "foo" },
                                           LeafNode { Program = "echo"; Args = "bar" })

        match Parser.parse (input |> String.concat "\n") with
        | Ok ast -> Assert.AreEqual(ast, expected)
        | Error err ->
            printfn "ERROR: %A" err
            Assert.Fail()

    member this.``Parser Test: Parse Operator: '|'`` () =

        let input = [
            """|"""
            """  Cmd: echo  Type: 0 Args: ` foo '"""
            """  Cmd: echo  Type: 0 Args: ` bar'"""
        ]

        let expected = BinaryOperatorNode (Pipe,
                                           LeafNode { Program = "echo"; Args = "foo" },
                                           LeafNode { Program = "echo"; Args = "bar" })

        match Parser.parse (input |> String.concat "\n") with
        | Ok ast -> Assert.AreEqual(ast, expected)
        | Error err ->
            printfn "ERROR: %A" err
            Assert.Fail()


    [<Test>]
    member this.``Parser Test: Parse IF ELSE`` () =

        let input = [
            """IF"""
            """Cmd: red  Type: 39 Args: `blue'"""
            """("""
            """Cmd: echo  Type: 0 Args: ` foo'"""
            """else"""
            """("""
            """Cmd: echo  Type: 0 Args: ` bar'"""
        ]

        let expected =
            IfNode (IfComparison ("red", "39", "blue"),
                        LeafNode { Program = "echo"; Args = "foo" },
                        Some (LeafNode { Program = "echo"; Args = "bar" }))


        match Parser.parse (input |> String.concat "\n") with
        | Ok ast -> Assert.AreEqual(ast, expected)
        | Error err ->
            printfn "ERROR: %A" err
            Assert.Fail()


    [<Test>]
    member this.``Parser Test: Parse complex cmdline`` () =
        let input = [
            """&&"""
            """  ("""
            """    for %a in (1 1 50) Do"""
            """      ("""
            """        &&"""
            """          Cmd: echo  Type: 0 Args: ` foo '"""
            """          Cmd: echo  Type: 0 Args: ` bar'"""
            """  Cmd: echo  Type: 0 Args: ` baz'"""
        ]

        let expected =
            BinaryOperatorNode (Success,
                                ForLoopNode (ForLoop "for %a in (1 1 50)",
                                             BinaryOperatorNode (Success,
                                                                 LeafNode { Program = "echo"; Args = "foo" },
                                                                 LeafNode { Program = "echo"; Args = "bar" })),
                                LeafNode { Program = "echo"; Args = "baz" })

        match Parser.parse (input |> String.concat "\n") with
        | Ok ast -> Assert.AreEqual(ast, expected)
        | Error err ->
            printfn "ERROR -> %A" err
            Assert.Fail()
