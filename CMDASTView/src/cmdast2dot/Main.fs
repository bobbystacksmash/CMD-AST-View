open System
open System.Diagnostics
open CMDASTView.Dot

[<EntryPoint>]
let main argv =

    let pipedInput = Console.In.ReadToEnd()

    (*let input = [
        """&&"""
        """  ("""
        """    for %a in (1 1 50) Do"""
        """      ("""
        """        &&"""
        """          Cmd: echo  Type: 0 Args: ` foo '"""
        """          Cmd: echo  Type: 0 Args: ` bar'"""
        """  Cmd: echo  Type: 0 Args: ` bazd'"""
    ]*)

    match Dot.toDOT pipedInput with
    | Ok program ->
        printfn "%s" program
    | Error err ->
        printfn "Error: %A" err




    0 // return an integer exit code
