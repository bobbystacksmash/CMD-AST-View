namespace CMDASTView.Dot

open System.Web

open CMDASTView.DomainTypes
open CMDASTView.Parser
open CMDASTView.Common

type Port  = Port  of string

type Entity = {
    Label: string
    Src: string
}


type Edge =
    Edge of string * string * Port option


type DotConversionState =
    | NoError
    | ConversionError of string


type Digraph = {
    State: DotConversionState
    Entities: Entity list
    Edges:    Edge List
}

module Dot =

    let private encodeHTML str =
        HttpUtility.HtmlEncode str

    let mutable private idCounter = 0
    let private idGen label =
        idCounter <- idCounter + 1
        sprintf "%s%d" label idCounter


    let private dotifyCmd (cmd: Cmd) id =

        let getArgs cmd =
            match cmd.Args with
            | Some args -> args
            | None -> "[[NONE]]"

        System.String.Format(
                """{0} [shape=none, fontname="" label=<
                  <TABLE BGCOLOR="#282a38"
                       COLOR="#5e607f"
                       BORDER="0"
                       CELLBORDER="1"
                       CELLSPACING="0">
                    <TR><TD colspan="2"><font color="#70e093">{1}</font></TD></TR>
                    <TR>
                      <TD><font color="#efece4"><b>ARGS</b></font></TD>
                      <TD><font color="#ec82c4"><b>{2}</b></font></TD>
                    </TR>
                  </TABLE>>];""",
                id, (cmd.Program |> encodeHTML), (getArgs cmd |> encodeHTML))



    let private dotifyBinaryOperator (op: Operator) id =

        let opToString op =
            match op with
            | Success -> "&&"
            | Always  -> "&"
            | Or      -> "||"
            | Pipe    -> "|"

        System.String.Format(
            """{0} [shape=none, fontname="", label=<
                 <TABLE BGCOLOR="#282a38"
                        COLOR="#5e607f"
                        BORDER="0"
                        CELLPADDING="3"
                        CELLBORDER="1"
                        CELLSPACING="0">
                   <TR>
                     <TD colspan="2"><font color="#70e093">{1}</font></TD>
                   </TR>
                   <TR>
                     <TD port="p1"><font color="#efece4"><b>LEFT</b></font></TD>
                     <TD port="p2"><font color="#efece4"><b>RIGHT</b></font></TD>
                   </TR>
                 </TABLE>>];""",
            id, (opToString op) |> encodeHTML)


    let private dotifyFor (forLoop: ForLoop) id =
        match forLoop with
        | ForLoop hdr ->
            System.String.Format(
                """{0} [shape=none, fontname="", label=<
                  <TABLE BGCOLOR="#282a38"
                         COLOR="#5e607f"
                         BORDER="0"
                         CELLBORDER="1"
                         CELLSPACING="0">
                    <TR><TD><font color="#70e093">{1}</font></TD></TR>
                    <TR><TD port="p1"><font color="#efece4"><b>DO</b></font></TD></TR>
                  </TABLE>>];""",
                id, (hdr |> encodeHTML))



    let private dotifyIf (ifCmp: IfComparison) id =
        match ifCmp with
        | IfComparison (leftOp, cmpType, rightOp) ->
            System.String.Format(
                """{0} [shape=none, fontname="", label=<
                  <TABLE BGCOLOR="#282a38"
                      COLOR="#5e607f"
                      BORDER="0"
                      CELLPADDING="3"
                      CELLBORDER="1"
                      CELLSPACING="0">
                    <TR><TD colspan="2"><font color="#efece4"><b>IF</b></font></TD></TR>
                    <TR><TD colspan="2"><font color="#70e093">{1}</font></TD></TR>
                    <TR>
                      <TD port="p1"><font color="#efece4"><b>THEN</b></font></TD>
                      <TD port="p2"><font color="#efece4"><b>ELSE</b></font></TD>
                   </TR>
                  </TABLE>>];""",
                id, (sprintf "%s == %s" leftOp rightOp)) // TODO: update for different CMP types.


    let private astToDigraph (ast: ASTNode) =
        let rec walk' ast dag =
            match dag.State with
            | ConversionError _ -> dag
            | NoError ->
                match ast with
                | ElseNode elseNode ->
                    let err = ConversionError "Unexpected 'ElseNode' as top-level AST element."
                    { dag with State = err }


                | ForLoopNode (loopHdr, loopBody) ->
                    let forId  = idGen "loop"
                    let forEnt = { Src   = dotifyFor loopHdr forId
                                   Label = forId }

                    let forBodyDag = walk' loopBody dag
                    let edge = Edge (forId, forBodyDag.Entities.Head.Label, (Some (Port "p1")))

                    { State    = NoError
                      Entities = [forEnt] @ forBodyDag.Entities @ dag.Entities
                      Edges    = [edge]   @ forBodyDag.Edges @ dag.Edges }


                | BinaryOperatorNode (op, left, right) ->
                    let binopId  = idGen "binop"
                    let binopEnt = { Src   = dotifyBinaryOperator op binopId
                                     Label = binopId }

                    let leftDag  = walk' left  dag
                    let rightDag = walk' right dag

                    // Create an edge where our binopId points to its
                    // left and right children.
                    let edges = [
                        Edge (binopId, leftDag.Entities.Head.Label, (Some (Port "p1")))
                        Edge (binopId, rightDag.Entities.Head.Label, (Some (Port "p2")))
                    ]

                    { State    = NoError
                      Entities = [binopEnt] @ leftDag.Entities @ rightDag.Entities @ dag.Entities
                      Edges    = edges @ leftDag.Edges @ rightDag.Edges @ dag.Edges }

                | LeafNode cmd ->
                    let cmdId  = idGen "cmd"
                    let cmdEnt = { Src = (dotifyCmd cmd cmdId); Label = cmdId }
                    { dag with Entities = cmdEnt :: dag.Entities }

                | IfNode (ifCmp, ifBody, maybeElseBody) ->
                    let ifId  = idGen "ifstmt"
                    let ifEnt = { Src = dotifyIf ifCmp ifId; Label = ifId }

                    let ifBodyDag  = walk' ifBody dag
                    let ifBodyEdge = Edge (ifId, ifBodyDag.Entities.Head.Label,   (Some (Port "p1")))

                    match maybeElseBody with
                    | Some elseBody ->
                        let elseBodyDag = walk' elseBody dag

                        let edges = [
                            ifBodyEdge
                            Edge (ifId, elseBodyDag.Entities.Head.Label, (Some (Port "p2")))
                        ]

                        { State = NoError
                          Entities = [ifEnt] @ ifBodyDag.Entities @ elseBodyDag.Entities @ dag.Entities
                          Edges    = edges @ ifBodyDag.Edges @ elseBodyDag.Edges @ dag.Edges }

                    | None ->
                        { State = NoError
                          Entities = [ifEnt] @ ifBodyDag.Entities @ dag.Entities
                          Edges    = [ifBodyEdge] @ ifBodyDag.Edges @ dag.Edges }


        let output = walk' ast { State = NoError; Entities = []; Edges = [] }
        match output.State with
        | NoError -> Ok output
        | ConversionError msg -> Error msg


    let private edgeToString (edge: Edge) =
        match edge with
        | Edge (src, dest, maybeSrcPort) ->
            match maybeSrcPort with
            | Some p ->
                match p with
                | Port port ->
                    sprintf "%s:%s -> %s;" src port dest
            | None ->
                sprintf "%s -> %s;" src dest


    let private toSource (dag: Digraph) =

        let leftPad2 str = sprintf "  %s" str

        let entities =
            dag.Entities
            |> List.map (fun ent -> ent.Src)
            |> List.map leftPad2
            |> String.concat "\n"

        let edges =
            dag.Edges
            |> List.map edgeToString
            |> List.map leftPad2
            |> String.concat "\n"

        let output = [
            "digraph {"
            entities
            edges
            "}"
        ]

        Ok (output |> String.concat "\n")

    let toDOT (input: string) =
        input
        |> Parser.parse
        |> Result.bind astToDigraph
        |> Result.bind toSource
