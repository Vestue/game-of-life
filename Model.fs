namespace DVA229_Proj_AvaloniaElmish

open System

type State =
    | Stopped
    | Running

type Model =
    { grid: GameGrid  // Grid of cells
      state: State    // Current running state of the game
      steps: Steps    // Amount of steps until the game should stop
      name: String }  // File name to be used for saving and loading
