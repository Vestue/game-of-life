namespace DVA229_Proj_AvaloniaElmish

open System

type Cell =
    | Dead
    | Alive

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

type GameGrid = Cell[,]

type Steps =
    | Infinite
    | Amount of int

type State =
    | Stopped
    | Running

type Model =
    { grid: GameGrid
      state: State
      steps: Steps
      name: String }
