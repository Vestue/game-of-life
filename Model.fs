namespace DVA229_Proj_AvaloniaElmish

open System

type State =
    | Stopped
    | Running

type Model =
    { grid: GameGrid
      state: State
      steps: Steps
      name: String }
