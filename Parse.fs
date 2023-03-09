module DVA229_Proj_AvaloniaElmish.Parse

open System

let cellToString (cell: Cell) =
    match cell with
    | Alive -> "■"
    | Dead -> " "

let cellFromString (character: char) =
    match character with
    | '■' -> Ok Alive
    | ' ' -> Ok Dead
    | _ -> Error "Could not read cell from string"

let gridToString (grid: GameGrid) =
    grid
    |> Array2D.map cellToString
    |> Seq.cast<string>
    |> Seq.fold (fun acc n -> acc + n) ""

let gridFromString (string: String) (gridLength: int) =
    let grid (str: char list) =
        Array2D.init gridLength gridLength (fun x y ->
            let index = x * gridLength + y

            match cellFromString str[index] with
            | Ok cell -> cell
            | Error _ -> Dead)

    grid (Seq.toList string)

let msgToString (msg: Message) =
    match msg with
    | Start -> "Start"
    | Stop -> "Stop"
    | Save -> "Save"
    | Load -> "Load"
    | Next -> "Next"
    | Reset -> "Reset"
    | Increase -> "+"
    | Decrease -> "-"
    | ToggleInfinite -> "∞"
    | _ -> ""

let stepsToString (steps: Steps) =
    match steps with
    | Infinite -> "∞"
    | Amount x -> $"{x}"
