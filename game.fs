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

    type Msg =
        | ChangeCellState of Position
        | Start
        | Tick
        | Stop
        | Save
        | Load
        | Next
        | Reset
        
    type Grid = Cell[,]

    type State = { grid: Grid; isRunning: bool}

    let cellToString (cell: Cell) =
        match cell with
        | Alive -> "■"
        | Dead -> " "

    let isAlive (cell: Cell) =
        match cell with
        | Alive -> true
        | _ -> false

    let gridLength = 16
    
    let timer dispatch =
        let time = new Timer(1000.0)
        time.Elapsed.Add (fun _ -> dispatch Tick)
        time.Start()
        
    // Setup the timer and connect it to the elmish model
    //? 'model' has to be here.
    let subscribe model = [ timer ]
        
    let initGrid = Array2D.create gridLength gridLength Cell.Dead

    let init () = {grid = initGrid; isRunning = false}

    let flipCellState (coordinates: Position) (state: State) : State =
        let newGrid: Cell[,] =
            Array2D.init gridLength gridLength (fun x y ->
                if x = coordinates.X && y = coordinates.Y then
                    match state.grid[coordinates.X, coordinates.Y] with
                    | Alive -> Cell.Dead
                    | Dead -> Cell.Alive
                else
                    state.grid[x, y])
        { state with grid = newGrid }

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
                match sumLivingNeighbours state.grid x y with
                | 3 -> Alive
                | 2 when isAlive state.grid[x,y] -> Alive
                | _ -> Dead)
        { state with grid = newGen }
        
    let update (msg: Msg) (state: State) =
        match msg with
        | Start -> { state with isRunning = true }
        | Stop -> { state with isRunning = false }
        | Next -> generateNextGeneration state
        | Tick when state.isRunning -> generateNextGeneration state
        | Reset -> { state with grid = initGrid }
        | ChangeCellState pos -> flipCellState pos state
        | _ -> state
        
    let view state dispatch =
        
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
                                                   Button.margin (0.0, 10.0, 0.0, 0.0)
                                                   Button.content "Start"
                                                   Button.onClick (fun _ -> dispatch Start)]

                                             Button.create
                                                 [ Button.margin (35.0, 10.0, 0.0, 0.0)
                                                   Button.width optionWidth
                                                   Button.height optionHeight
                                                   Button.content "Stop" 
                                                   Button.onClick (fun _ -> dispatch Stop) ]

                                             Button.create
                                                 [ Button.margin (70.0, 10.0, 0.0, 0.0)
                                                   Button.width optionWidth
                                                   Button.height optionHeight
                                                   Button.content "Reset"
                                                   Button.onClick (fun _ -> dispatch Reset) ]
                                             
                                             Button.create
                                                 [ Button.margin (105.0, 10.0, 0.0, 0.0)
                                                   Button.width optionWidth
                                                   Button.height optionHeight
                                                   Button.content "Next"
                                                   Button.onClick (fun _ -> dispatch Next) ]] ] ] ]
