namespace DVA229_Proj_AvaloniaElmish

open System
open System.Linq.Expressions
open System.Timers
open Avalonia
open Avalonia.Controls
open Avalonia.Layout
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open Microsoft.VisualBasic.CompilerServices

module Game =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL

    open Microsoft.FSharp.Collections

    type Cell =
        | Dead
        | Alive

    type Position = { X: int; Y: int }

    type UserAction =
        | ChangeCellState of Position
        | Start
        | Run
        | Stop
        | Save
        | Load
        | Reset

    type GridState =
        | Running
        | Stopped
        
    type Grid = Cell[,]

    type State = { grid: Grid; timer: Timer}

    let cellToString (cell: Cell) =
        match cell with
        | Alive -> "■"
        | Dead -> " "

    let isCellAlive (cell: Cell) =
        match cell with
        | Dead -> false
        | Alive -> true

    let gridLength = 16
    
    let initTimer =
        let timer = new Timer(1000.0)
        timer.AutoReset <- true
        timer

    let init: State = {grid = Array2D.create gridLength gridLength Cell.Dead; timer = initTimer}

    let flipCellState (coordinates: Position) (state: State) : State =
        let newGrid: Cell[,] =
            Array2D.init gridLength gridLength (fun x y ->
                if x = coordinates.X && y = coordinates.Y then
                    match state.grid[coordinates.X, coordinates.Y] with
                    | Alive -> Cell.Dead
                    | Dead -> Cell.Alive
                else
                    state.grid[x, y])
        {grid = newGrid; timer = state.timer}

    let getNeighbours (grid: Grid) (xCoord: int) (yCoord: int) : Cell list =
        // Use min and max to prevent cells at the edges from attempting
        // to access indexes that are not in the array.
        let rowRange = { max 0 (xCoord - 1) .. min (gridLength - 1) (xCoord + 1) }
        let colRange = { max 0 (yCoord - 1) .. min (gridLength - 1) (yCoord + 1) }

        [ for row in rowRange do
              for col in colRange do
                  if row <> xCoord || col <> yCoord then
                      yield grid[row, col] ]

    let countAlive (cell: Cell) : int =
        match cell with
        | Alive -> 1
        | Dead -> 0
        
    let sumLivingNeighbours (grid: Grid) x y : int =
        getNeighbours grid x y
        |> List.fold (fun acc cell -> acc + countAlive cell) 0

    let generateNextGeneration (state: State) : State =
        let newGen: Cell[,] =
            Array2D.init gridLength gridLength (fun x y ->
                match sumLivingNeighbours state.grid x y, state.grid[x, y] with
                | x, Dead when x = 3 -> Alive
                | x, Alive when x = 3 || x = 2 -> Alive
                | _, _ -> Dead)
        {grid = newGen; timer = state.timer}
        
    let update (action: UserAction) (state: State) : State =
        match action with
        | Start ->
            state.timer.Start()
            state
        | Stop ->
            state.timer.Stop()
            state
        | Run -> generateNextGeneration state
        | Reset ->
            state.timer.Stop()
            init
        | ChangeCellState pos -> flipCellState pos state
        | _ -> state
        

    let view (state: State) dispatch =
        state.timer.Elapsed.Add (fun _ -> dispatch Run)

        // Sizes for UI
        let squareLength = 30.0
        let buttonsInColumn = 16.0
        let optionHeight = 30.0
        let optionWidth = 60.0
        
        let createCellButton (pos: Position) =
            Button.create
                [ Button.width squareLength
                  Button.height squareLength
                  Button.content (cellToString state.grid[pos.X, pos.Y])
                  Button.onClick (fun _ -> dispatch (ChangeCellState pos)) ]


        DockPanel.create
            [ DockPanel.children
                  [ WrapPanel.create
                        [ WrapPanel.horizontalAlignment HorizontalAlignment.Center
                          WrapPanel.verticalAlignment VerticalAlignment.Center
                          WrapPanel.dock Dock.Bottom
                          WrapPanel.itemWidth squareLength
                          WrapPanel.maxWidth (squareLength * buttonsInColumn)

                          WrapPanel.children[for i = 0 to gridLength - 1 do
                                                 for j = 0 to gridLength - 1 do
                                                     createCellButton { X = i; Y = j }

                                             Button.create
                                                 [ Button.width optionWidth
                                                   Button.height optionHeight
                                                   Button.content "Start"
                                                   Button.onClick (fun _ -> dispatch Start) ]

                                             Button.create
                                                 [ Button.margin 35
                                                   Button.width optionWidth
                                                   Button.height optionHeight
                                                   Button.content "Stop" 
                                                   Button.onClick (fun _ -> dispatch Stop) ]

                                             Button.create
                                                 [ Button.margin 70
                                                   Button.width optionWidth
                                                   Button.height optionHeight
                                                   Button.content "Reset"
                                                   Button.onClick (fun _ -> dispatch Reset) ]] ] ] ]
