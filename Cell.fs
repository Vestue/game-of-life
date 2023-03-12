namespace DVA229_Proj_AvaloniaElmish

type Cell =
    | Dead
    | Alive

module Cell =

    let toString (cell: Cell) =
        match cell with
        | Alive -> "■"
        | Dead -> " "

    let fromString (character: char) =
        match character with
        | '■' -> Ok Alive
        | ' ' -> Ok Dead
        | _ -> Error "Could not read cell from string"

    let isAlive (cell: Cell) =
        match cell with
        | Alive -> true
        | _ -> false
