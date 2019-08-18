namespace CMDASTView

module Common =

    let tryTake num stack =
        if num <= 0 then
            Error "Number cannot be less-than 1"
        else
            let rec take' num stack accum =
                match stack with
                | [] when num = 0 -> Ok (accum, [])
                | [] -> Error "not enough items"
                | _ when num = 0 -> Ok (accum, stack)
                | head :: rest ->
                    take' (num - 1) rest (head :: accum)
            take' num stack []
