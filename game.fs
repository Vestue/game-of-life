namespace DVA229_Proj_AvaloniaElmish

open System
open System.Linq.Expressions
open System.Net.Mime
open System.Timers
open Avalonia
open Avalonia.Controls
open Avalonia.Input
open Avalonia.Layout
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open Microsoft.VisualBasic.CompilerServices

module Game =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Controls.ApplicationLifetimes
    open Avalonia.Dialogs
    open System.IO

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
        | Increase
        | Decrease
        | ToggleInfinite
        
    type Grid = Cell[,]
    
    
    type Steps =
        | Infinite
        | Amount of int

    type State =
        | Stopped
        | Running 
    type Model = { grid: Grid; state: State; steps: Steps }
    
    let folderPath = __SOURCE_DIRECTORY__ + "/saves"
    
    let stepsToString (steps: Steps) =
        match steps with
        | Infinite -> "∞"
        | Amount x -> $"{x}"

    let cellToString (cell: Cell) =
        match cell with
        | Alive -> "■"
        | Dead -> " "
        
    let msgToString (msg: Msg) =
        match msg with
        | Start -> "Start"
        | Stop -> "Stop"
        | Save -> "Save"
        | Load -> "Load"
        | Next -> "Next"
        | Reset -> "Reset"
        | Increase -> "Increase"
        | Decrease -> "Decrease"
        | ToggleInfinite -> "∞"
        | _ -> ""
        
    //TODO: REPLACE failwith with Result error
    let cellFromString (character: char) =
        match character with
        | '■' -> Alive
        | ' ' -> Dead
        | _ -> failwith "Could not read cell from string"

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

    let init = {grid = initGrid; state = Stopped; steps = Infinite}

    let flipCellState (coordinates: Position) (model: Model) : Model =
        let newGrid: Cell[,] =
            Array2D.init gridLength gridLength (fun x y ->
                if x = coordinates.X && y = coordinates.Y then
                    match model.grid[coordinates.X, coordinates.Y] with
                    | Alive -> Cell.Dead
                    | Dead -> Cell.Alive
                else
                    model.grid[x, y])
        { model with grid = newGrid }

    let getNeighbours (grid: Grid) (xCoord: int) (yCoord: int) : Cell list =
        // Use min and max to prevent cells at the edges from attempting
        // to access indexes that are not in the array.
        let rowRange = { max 0 (xCoord - 1) .. min (gridLength - 1) (xCoord + 1) }
        let colRange = { max 0 (yCoord - 1) .. min (gridLength - 1) (yCoord + 1) }

        [ for row in rowRange do
              for col in colRange do
                  if row <> xCoord || col <> yCoord then
                      yield grid[row, col] ]
        
    let lowerRunningCountIfNeeded (model: Model) =
        match model.state, model.steps with
        | Running, Amount x when x <= 1 -> { grid = model.grid; state = Stopped; steps = Amount 0 }
        | Running, Amount x -> { model with steps = Amount (x - 1) }
        | _ -> model
        
    let countAlive (cell: Cell) : int =
        match cell with
        | Alive -> 1
        | Dead -> 0
        
    let sumLivingNeighbours (grid: Grid) x y : int =
        getNeighbours grid x y
        |> List.fold (fun acc cell -> acc + countAlive cell) 0

    let generateNextGeneration (model: Model) : Model =
        let newGen: Cell[,] =
            Array2D.init gridLength gridLength (fun x y ->
                match sumLivingNeighbours model.grid x y with
                | 3 -> Alive
                | 2 when isAlive model.grid[x,y] -> Alive
                | _ -> Dead)
        lowerRunningCountIfNeeded { model with grid = newGen }
        
        
    let GridToString (model: Model) =
        model.grid
        |> Array2D.map(fun cell -> cellToString cell)
        |> Seq.cast<string> |> Seq.fold (fun l n -> n :: l) []
        |> List.rev
        |> List.fold(+)""
        
    let saveFileToFolder stringToSave fileName folderPath =
        let filePath = Path.Combine(folderPath, fileName)
        File.WriteAllText(filePath, stringToSave)
    let getFileName ()=
        let nameStart = "Save"
        let rand = Random()
        let id = rand.Next(0, 10000)
        let fileName = nameStart + id.ToString()
        fileName
    
    let saveModel (model : Model) =
        let fileContent = GridToString model
        let filePath = Path.Combine(folderPath, getFileName())
        File.WriteAllText(filePath, fileContent)
        model
        
    let translateStringToGrid (str : char list)  =
        
        let loadedGrid =
            Array2D.init gridLength gridLength (fun x y ->
                let index = x * gridLength + y
                match cellFromString str[index] with
                | Alive -> Alive
                |Dead -> Dead)
        loadedGrid
        
    let loadModel() =
        let fileName = "Save9903"
        let filePath = Path.Combine(folderPath, fileName)
        let modelString = File.ReadAllLines(filePath) |> Seq.toList
        let str = modelString.Head
        let loadedGrid = translateStringToGrid (Seq.toList str)
        let model = {grid = loadedGrid; state = Stopped; steps = Infinite}
        model
        
    let toggleStepState (model: Model) =
        match model.steps with
        | Infinite -> { model with steps = Amount 0 }
        | _ -> { model with steps = Infinite }
        
    let isRunning (model: Model) =
        match model.state with
        | Running -> true
        | _ -> false
        
    let update (msg: Msg) (model: Model) =
        match msg with
        | Start -> { model with state = Running }
        | Stop -> { model with state = Stopped }
        | Next -> generateNextGeneration model
        | Tick when isRunning model -> generateNextGeneration model
        | Reset -> init
        | ChangeCellState pos -> flipCellState pos model
        | Load -> loadModel()
        | Save -> saveModel model
        | _ -> model
        
        
    let view model dispatch =
        
        // Sizes for UI
        let squareLength = 30.0
        let buttonsInColumn = 16.0
        let optionHeight = 30.0
        let optionWidth = 60.0
        let marginBase = 35.0
        
        let createCellButton (pos: Position) =
            Button.create
                [ Button.width squareLength
                  Button.height squareLength
                  Button.content (cellToString model.grid[pos.X, pos.Y])
                  Button.onClick (fun _ -> dispatch (ChangeCellState pos)) ]
                
        let getIndexOfMsg (toFind: Msg) (list: Msg list) =
                list
                |> List.findIndex toFind.Equals
                |> float

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

                                             let buttons = [ Start; Stop; Reset; Next; Save; Load ]
                                             for msg in buttons do
                                                 let margin = marginBase * getIndexOfMsg msg buttons
                                                 Button.create
                                                     [ Button.width optionWidth
                                                       Button.height optionHeight
                                                       Button.margin (margin, 10.0, 0.0, 0.0)
                                                       Button.content (msgToString msg)
                                                       Button.onClick (fun _ -> dispatch msg)]] ] ] ]