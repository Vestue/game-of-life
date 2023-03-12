namespace DVA229_Proj_AvaloniaElmish

open System

type Position = { X: int; Y: int }

type Message =
    | ChangeCellState of Position
    | ChangeModelName of String
    | Start
    | Tick
    | Stop
    | Save
    | Load
    | Next
    | Reset
    | Increase
    | Decrease
    | ToggleInfinite

module Message =

    let toString (msg: Message) =
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
