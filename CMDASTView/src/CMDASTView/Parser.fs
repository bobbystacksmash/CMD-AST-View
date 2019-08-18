namespace CMDASTView.Parser

open System
open System.Text.RegularExpressions

open CMDASTView.Common
open CMDASTView.DomainTypes
open CMDASTView.Tokeniser

module Parser =

    let private toLines (str: string) =
        str.Split([|"\n"|], System.StringSplitOptions.None)

    let private trim (str: string) = str.Trim()
    let private isEmpty (str: string) = str.Length = 0
    let private notEmpty str = (isEmpty str = false)
    let private ignoreParen str = Regex.IsMatch(str, "^\s*\(\s*$") = false

    let private validateTokens tokens =
        let rec validate' tokens accum =
            match tokens with
            | [] -> Ok accum
            | head:: rest ->
                match head with
                | Ok token  -> validate' rest (token :: accum)
                | Error err -> Error err
        validate' tokens []


    //
    // S P E C I A L  T O K E N   H A N D L E R S
    // ==========================================
    //
    // FOR LOOP
    // ~~~~~~~~
    let private handleFor (forLoop: ForLoop) astNodes =
        match tryTake 1 astNodes with
        | Ok (taken, remaining) ->
            let node = ForLoopNode (forLoop, taken.Head)
            Ok (node :: remaining)
        | Error err -> Error err
    //
    // ELSE
    // ~~~~
    let private handleElse (els: IfElse) astNodes =
        match tryTake 1 astNodes with
        | Ok (taken, remaining) ->
            let node = ElseNode (taken.Head)
            Ok (node :: remaining)
        | Error err -> Error err
    //
    // IF COMPARISON
    // ~~~~~~~~~~~~~
    let private handleIfCmp (cmp: IfComparison) astNodes =
        match tryTake 2 astNodes with
        | Ok (taken, remaining) ->
            match taken with
            | [ElseNode elseNode;  ifBody] ->
                // We know the last token is an ElseNode -- just need
                // to validate that the `ifBody' token is a valid
                // token type for the first branch of an If statement.
                match ifBody with
                | ElseNode _ ->
                    Error "IF cannot have ELSE has first branch."
                | _ ->
                    printfn "----> %A" ifBody
                    let node = IfNode (cmp, ifBody, Some elseNode)
                    Ok (node :: remaining)

            | [ifBody; putMeBack] ->
                let node = IfNode (cmp, ifBody, None)
                Ok (node :: putMeBack :: remaining)

            | _ ->
                Error "Failed to parse IF statement."

        | Error err -> Error err
    //
    // IF STATEMENT
    // ~~~~~~~~~~~~
    let private handleIf (ifs: IfStatement) (astNodes: ASTNode list) =
        match astNodes with
        | [] -> Error "Unable to parse IF statement."
        | head :: rest ->
            match head with
            | IfNode _ -> Ok astNodes
            | _ -> Error "Cannot parse IF statement: top-of-stack is not an IF comparison."


    let private handleBinaryOperator (binop: Operator) astNodes =
        match tryTake 2 astNodes with
        | Ok (taken, remaining) ->
            let node = (BinaryOperatorNode (binop, taken.[1], taken.[0]))
            Ok (node :: remaining)
        | Error err -> Error err


    let private specialToASTNode special astNodes =
        match special with
        | Binop binop  -> handleBinaryOperator binop astNodes
        | For forLoop  -> handleFor forLoop astNodes
        | Else ifElse  -> handleElse ifElse astNodes
        | IfTest ifCmp -> handleIfCmp ifCmp astNodes
        | If ifs       -> handleIf ifs astNodes


    let private commandToASTNode (cmd: Cmd) astNodes =
        Ok ((LeafNode cmd) :: astNodes)


    let private toAST tokens =

        let rec toAST' tokens (astNodes: ASTNode list) =
            match tokens with
            | [] -> Ok astNodes
            | head :: rest ->
                match head with
                | Special special ->
                    match specialToASTNode special astNodes with
                    | Ok newAstNodes -> toAST' rest newAstNodes
                    | Error err      -> Error err

                | Command cmd ->
                    // CMD doesn't need a special handler, because
                    // it's just wrapped in a LeafNode.
                    toAST' rest ((LeafNode cmd) :: astNodes)

        match toAST' tokens [] with
        | Ok ast ->
            if ast.Length = 1 then
                Ok ast.Head
            else
                printfn "DANGLING REFERENCE STACK"
                printfn "========================"
                List.iter (fun x -> printfn ">> %A" x) ast
                Error "AST contains at least one dangling reference."
        | Error err -> Error err


    let private tokensToAST tokens =
        (Ok tokens)
        |> Result.bind validateTokens
        |> Result.bind toAST


    let parse (input: string) =
        input
        |> toLines
        |> List.ofSeq
        |> List.map trim
        |> List.filter notEmpty
        |> List.filter ignoreParen
        |> List.map Tokeniser.tokenise
        |> tokensToAST
