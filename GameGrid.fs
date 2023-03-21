namespace DVA229_Proj_AvaloniaElmish

open System

type GameGrid = Cell[,]

module GameGrid =

    let length = 16

    let init = lazy (Array2D.create length length Cell.Dead)

    let toString (grid: GameGrid) =
        grid
        |> Array2D.map Cell.toString
        |> Seq.cast<string>
        |> Seq.fold (fun acc n -> acc + n) ""

    let fromString (string: String) =
        let grid (str: char list) =
            Array2D.init length length (fun x y ->
                let index = x * length + y

                // Errors in single characters are handled as dead cells
                // to ensure that a full grid is returned from the function.
                match Cell.fromString str[index] with
                | Ok cell -> cell
                | Error _ -> Dead) 

        grid (Seq.toList string)
