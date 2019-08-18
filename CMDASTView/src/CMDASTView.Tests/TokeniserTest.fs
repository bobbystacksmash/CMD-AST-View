namespace CMDASTView.Tests.TokeniserTest

open System
open NUnit.Framework
open CMDASTView.DomainTypes
open CMDASTView.Tokeniser

type TokenTest = { Input: string; Expected: Token }

[<TestFixture>]
type TestClass () =

    let compare test =
        match Tokeniser.tokenise test.Input with
        | Ok output -> Assert.AreEqual(output, test.Expected)
        | Error _ -> Assert.Fail((sprintf "Error parsing token: %s" test.Input))


    let errors test =
        match Tokeniser.tokenise test.Input with
        | Error msg -> Assert.Pass(msg)
        | _ -> Assert.Fail((sprintf "Expected to throw errow when parsing: %s" test.Input))



    [<Test>]
    member this.``Tokeniser Test: Operator Token Identification`` () =

        let tests = [
            { Input = "&&"; Expected = (Special (Binop  Success)) }
            { Input = "&";  Expected = (Special (Binop  Always))  }
            { Input = "||"; Expected = (Special (Binop  Or))      }
            { Input = "|";  Expected = (Special (Binop  Pipe))    }
        ]
        tests |> List.iter compare


    [<Test>]
    member this.``Tokeniser Test: Special Token Identification`` () =

        let tests = [
            { Input = "IF"; Expected = (Special (If IfStatement)) }
            { Input = "else"; Expected = (Special (Else IfElse))  }
            { Input = "for %a in (1 1 50) Do"; Expected = (Special (For (ForLoop "for %a in (1 1 50)"))) }
            { Input = "Cmd: red  Type: 39 Args: `blue'"; Expected = (Special (IfTest (IfComparison ("red", "39", "blue")))) }
        ]
        tests |> List.iter compare


    [<Test>]
    member this.``Tokeniser Test: Command Token Identification`` () =

        let tests = [
            { Input = "Cmd: echo  Type: 0 Args: ` abc '"
              Expected = (Command { Program = "echo"; Args = Some "abc" }) }

            { Input = "Cmd: calc  Type: 0"
              Expected = (Command { Program = "calc"; Args = None }) }
        ]
        tests |> List.iter compare


    [<Test>]
    member this.``Tokeniser Test: Bad Token Identification`` () =

        let invalidInputs = [
            "foobar"
        ]

        invalidInputs |> List.iter (fun input ->
                                    match Tokeniser.tokenise input with
                                    | Ok _ -> Assert.Fail()
                                    | Error _ -> Assert.Pass())
