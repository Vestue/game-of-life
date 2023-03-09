namespace DVA229_Proj_AvaloniaElmish

open System
open System.Timers
open Avalonia.Controls
open Avalonia.Layout
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open Avalonia.FuncUI.DSL
open System.IO

module Game =

    let stepsToString (steps: Steps) =
        match steps with
        | Infinite -> "∞"
        | Amount x -> $"{x}"

    let cellToString (cell: Cell) =
        match cell with
        | Alive -> "■"
        | Dead -> " "

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

    let cellFromString (character: char) =
        match character with
        | '■' -> Ok Alive
        | ' ' -> Ok Dead
        | _ -> Error "Could not read cell from string"

    let isAlive (cell: Cell) =
        match cell with
        | Alive -> true
        | _ -> false

    let gridLength = 16

    let timer dispatch =
        let time = new Timer(1000.0)
        time.Elapsed.Add(fun _ -> dispatch Tick)
        time.Start()

    // Setup the timer and connect it to the elmish model
    //? 'model' has to be here.
    let subscribe model = [ timer ]

    let initGrid = Array2D.create gridLength gridLength Cell.Dead

    let init =
        { grid = initGrid
          state = Stopped
          steps = Infinite
          name = "" }

    let flipCellState (position: Position) (model: Model) : Model =
        let newGrid: Cell[,] =
            Array2D.init gridLength gridLength (fun x y ->
                match position with
                | pos when y = pos.Y && x = pos.X ->
                    match model.grid[pos.X, pos.Y] with
                    | Alive -> Cell.Dead
                    | Dead -> Cell.Alive
                | _ -> model.grid[x, y])

        { model with grid = newGrid }

    let getNeighbours (grid: GameGrid) (xCoord: int) (yCoord: int) : Cell list =
        // Use min and max to prevent cells at the edges from attempting
        // to access indexes that are not in the array.
        let rowRange = { max 0 (xCoord - 1) .. min (gridLength - 1) (xCoord + 1) }
        let colRange = { max 0 (yCoord - 1) .. min (gridLength - 1) (yCoord + 1) }

        [ for row in rowRange do
              for col in colRange do
                  if row <> xCoord || col <> yCoord then
                      yield grid[row, col] ]

    let increaseSteps (model: Model) =
        match model.steps with
        | Amount x -> { model with steps = Amount(x + 1) }
        | _ -> { model with steps = Amount 1 }

    let decreaseStepsIfNeeded (model: Model) =
        match model.steps with
        | Amount x when x <= 1 ->
            { grid = model.grid
              state = Stopped
              steps = Amount 0
              name = model.name }
        | Amount x -> { model with steps = Amount(x - 1) }
        | _ -> model

    let countAlive (cell: Cell) : int =
        match cell with
        | Alive -> 1
        | Dead -> 0

    let sumLivingNeighbours (grid: GameGrid) x y : int =
        getNeighbours grid x y |> List.fold (fun acc cell -> acc + countAlive cell) 0

    let generateNextGeneration (model: Model) : Model =
        let newGen: Cell[,] =
            Array2D.init gridLength gridLength (fun x y ->
                match sumLivingNeighbours model.grid x y with
                | 3 -> Alive
                | 2 when isAlive model.grid[x, y] -> Alive
                | _ -> Dead)

        decreaseStepsIfNeeded { model with grid = newGen }


    let gridToString (model: Model) =
        model.grid
        |> Array2D.map cellToString
        |> Seq.cast<string>
        |> Seq.fold (fun acc n -> acc + n) ""

    let folderPath = __SOURCE_DIRECTORY__ + "/saves"

    let saveStringAsFile stringToSave fileName =
        let filePath = Path.Combine(folderPath, fileName)
        File.WriteAllText(filePath, stringToSave)

    let getFullFileName (name: String) = name + ".rog" // rog = Ragnar Oscar Grid

    let saveModel (model: Model) =
        match model.name with
        | "" -> Error "File needs to have a name"
        | _ ->
            saveStringAsFile (gridToString model) (getFullFileName model.name)
            Ok { model with name = "" } // Clear the filename to give feedback to the user that it has been saved

    let translateStringToGrid (string: String) =
        let grid (str: char list) =
            Array2D.init gridLength gridLength (fun x y ->
                let index = x * gridLength + y

                match cellFromString str[index] with
                | Ok cell -> cell
                | Error _ -> Dead)

        grid (Seq.toList string)

    let loadModel (model: Model) =
        let filePath = Path.Combine(folderPath, getFullFileName model.name)
        let modelString = File.ReadLines(filePath) |> Seq.head
        let loadedGrid = translateStringToGrid modelString
        { init with grid = loadedGrid }

    let toggleStepState (model: Model) =
        match model.steps with
        | Infinite -> { model with steps = Amount 0 }
        | _ -> { model with steps = Infinite }

    let isRunning (model: Model) =
        match model.state with
        | Running -> true
        | _ -> false

    let update (msg: Message) (model: Model) =
        match msg with
        | Start -> { model with state = Running }
        | Stop -> { model with state = Stopped }
        | Next -> generateNextGeneration model
        | Tick when isRunning model -> generateNextGeneration model
        | Reset -> init
        | ChangeCellState pos ->
            match model.state with
            | Stopped -> flipCellState pos model
            | _ -> model
        | Load ->
            match model.state with
            | Stopped -> loadModel model
            | _ -> model
        | Save ->
            match (saveModel model) with
            | Ok newModel -> newModel
            | Error _ -> model
        | ChangeModelName newString -> { model with name = newString }
        | Increase -> increaseSteps model
        | Decrease -> decreaseStepsIfNeeded model
        | ToggleInfinite -> toggleStepState model
        | _ -> model


    let view model dispatch =

        // Sizes for UI
        let squareLength = 30.0
        let buttonsInColumn = 16.0
        let optionHeight = 30.0
        let optionWidth = 55.0
        let marginBase = 30.0

        let createCellButton (pos: Position) =
            Button.create
                [ Button.width squareLength
                  Button.height squareLength
                  Button.content (cellToString model.grid[pos.X, pos.Y])
                  Button.onClick (fun _ -> dispatch (ChangeCellState pos)) ]

        let createBottomButton (content: String) (margin: float) handle =
            Button.create
                [ Button.width optionWidth
                  Button.height optionHeight
                  Button.margin (margin, 10.0, 0.0, 0.0)
                  Button.content content
                  Button.onClick handle ]

        let getFloatedIndexOfMsg (toFind: Message) (list: Message list) =
            list |> List.findIndex toFind.Equals |> float

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

                                             TextBox.create
                                                 [ TextBox.margin (-(marginBase), 10.0, 0.0, 0.0)
                                                   TextBox.width optionWidth
                                                   TextBox.height optionHeight
                                                   TextBox.text model.name
                                                   TextBox.onTextChanged (ChangeModelName >> dispatch) ]

                                             let buttons = [ Start; Stop; Reset; Next; Save; Load; Decrease ]

                                             for msg in buttons do
                                                 let margin = marginBase * getFloatedIndexOfMsg msg buttons
                                                 createBottomButton (msgToString msg) margin (fun _ -> dispatch msg)

                                             createBottomButton (stepsToString model.steps) (marginBase * 7.0) (fun _ ->
                                                 dispatch ToggleInfinite)

                                             createBottomButton (msgToString Increase) (marginBase * 8.0) (fun _ ->
                                                 dispatch Increase)

                                             ] ] ] ]
