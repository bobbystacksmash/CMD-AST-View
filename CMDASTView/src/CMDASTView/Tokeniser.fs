namespace CMDASTView.Tokeniser

open CMDASTView.DomainTypes
open System
open System.Text.RegularExpressions

module Tokeniser =

    let private (|CMD|CMP|UNKNOWN|) (input: string) =
        // TODO: hanlde redirects.
        let m = Regex.Match(input, "^Cmd: (.+)  Type: (\d+)(?: Args: `(.+)')?$")

        if m.Success then
            let (cmd, t) = (m.Groups.[1].Value,
                            m.Groups.[2].Value)

            let args =
                if m.Groups.Count = 4 then
                    m.Groups.[3].Value.Trim()
                else
                    ""

            match t with
            | "39" ->
                CMP (Special (IfTest (IfComparison (cmd, t, args))))
            | _ ->
                if String.IsNullOrEmpty args then
                    CMD (Command { Program = cmd; Args = None })
                else
                    CMD (Command { Program = cmd; Args = Some args })
        else
            UNKNOWN


    let private (|IF|UNKNOWN|) (input: string) =
        if Regex.IsMatch(input.Trim(), "^IF$") then
            IF
        else
            UNKNOWN


    let private (|ELSE|UNKNOWN|) (input: string) =
        if Regex.IsMatch(input.Trim(), "^else$")
        then
            ELSE
        else
            UNKNOWN


    let private (|FOR|UNKNOWN|) (input: string) =
        if Regex.IsMatch(input.Trim(), "^for ")
        then
            FOR (Regex.Replace(input.Trim(), "Do$", "").Trim())
        else
            UNKNOWN


    let private (|OPERATOR|UNKNOWN|) (input: string) =
        let trimmed = input.Trim()

        if trimmed = "&&" then
            OPERATOR (Special (Binop Success))
        elif trimmed = "&" then
            OPERATOR (Special (Binop Always))
        elif trimmed = "||" then
            OPERATOR (Special (Binop Or))
        elif trimmed = "|" then
            OPERATOR (Special (Binop Pipe))
        else
            UNKNOWN


    let private (|TOKEN|UNKNOWN|) (line: string) =
        match line with
        | CMD cmd     -> TOKEN cmd
        | CMP cmp     -> TOKEN cmp
        | IF          -> TOKEN (Special (If IfStatement))
        | ELSE        -> TOKEN (Special (Else IfElse))
        | FOR hdr     -> TOKEN (Special (For (ForLoop hdr)))
        | OPERATOR op -> TOKEN op
        | _           -> UNKNOWN


    let tokenise (line: string) =
        match line with
        | TOKEN token -> Ok token
        | UNKNOWN _   -> Error (sprintf "Unknown token: '%s'." line)
