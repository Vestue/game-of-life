namespace DVA229_Proj_AvaloniaElmish


type Steps =
    | Infinite
    | Amount of int

module Steps =

    let toString (steps: Steps) =
        match steps with
        | Infinite -> "∞"
        | Amount x -> $"{x}"
