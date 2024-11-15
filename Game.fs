﻿namespace DVA229_Proj_AvaloniaElmish

open System
open System.Timers
open Avalonia.Controls
open Avalonia.Layout
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open Avalonia.FuncUI.DSL

module Game =

    // Create a timer that sends a Tick to update every second
    let timer dispatch =
        let time = new Timer(1000.0)
        time.Elapsed.Add(fun _ -> dispatch Tick)
        time.Start()

    // Setup the timer and connect it to the elmish model
    //? 'model' has to be here.
    let subscribe model = [ timer ]

    let init =
        { grid = GameGrid.init.Force()
          state = Stopped
          steps = Infinite
          name = FileManager.initialFileName }

    // Flip the state of a given cell from dead to alive or alive to dead
    let flipCellState (position: Position) (model: Model) : Model =
        let newGrid: Cell[,] =
            Array2D.init GameGrid.length GameGrid.length (fun x y ->
                match position with
                | pos when y = pos.Y && x = pos.X ->
                    match model.grid[pos.X, pos.Y] with
                    | Alive -> Cell.Dead
                    | Dead -> Cell.Alive
                | _ -> model.grid[x, y])

        { model with grid = newGrid }

    // Get a list of the neighbours of cell with specified position
    let getNeighbours (grid: GameGrid) (xCoord: int) (yCoord: int) =
        // Use min and max to prevent cells at the edges from attempting
        // to access indexes that are not in the array.
        let rowRange = { max 0 (xCoord - 1) .. min (GameGrid.length - 1) (xCoord + 1) }
        let colRange = { max 0 (yCoord - 1) .. min (GameGrid.length - 1) (yCoord + 1) }

        // This can be lazy evaluated as each cell will always have the
        // same set of neighbours.
        lazy [ for row in rowRange do
                  for col in colRange do
                      if row <> xCoord || col <> yCoord then
                          yield grid[row, col] ]

    let increaseSteps (model: Model) =
        match model.steps with
        | Amount x -> { model with steps = Amount(x + 1) }
        | Infinite -> { model with steps = Amount 1 }

    // Decrease steps by 1 until 0 if they are Steps Amount x
    let decreaseStepsIfNeeded (model: Model) =
        match model.steps with
        | Amount x when x <= 1 ->
            { grid = model.grid
              state = Stopped
              steps = Amount 0
              name = model.name }
        | Amount x -> { model with steps = Amount(x - 1) }
        | Infinite -> model

    // Used by functions counting amount of living cells
    let countAlive (cell: Cell) : int =
        match cell with
        | Alive -> 1
        | Dead -> 0

    let sumLivingNeighbours (grid: GameGrid) x y : int =
        (getNeighbours grid x y).Force()
        |> List.fold (fun acc cell -> acc + countAlive cell) 0

    let generateNextGeneration (model: Model) : Model =
        let newGen: Cell[,] =
            Array2D.init GameGrid.length GameGrid.length (fun x y ->
                match sumLivingNeighbours model.grid x y with
                | 3 -> Alive
                | 2 when Cell.isAlive model.grid[x, y] -> Alive
                | _ -> Dead)

        decreaseStepsIfNeeded { model with grid = newGen }

    // Toggles the state of steps between infinite and amount x
    let toggleStepState (model: Model) =
        match model.steps with
        | Infinite -> { model with steps = Amount 0 }
        | _ -> { model with steps = Infinite }

    let isRunning (model: Model) =
        match model.state with
        | Running -> true
        | _ -> false

    // Update the model using the given message
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
            | Stopped ->
                match FileManager.loadModel model with
                | Ok newModel -> newModel
                | Error _ -> model
            | _ -> model
        | Save ->
            match (FileManager.saveModel model) with
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
                  Button.content (Cell.toString model.grid[pos.X, pos.Y])
                  Button.onClick (fun _ -> dispatch (ChangeCellState pos)) ]

        let createBottomButton (content: String) (margin: float) handle =
            Button.create
                [ Button.width optionWidth
                  Button.height optionHeight
                  Button.margin (margin, 10.0, 0.0, 0.0)
                  Button.content content
                  Button.onClick handle ]

        // Get a index which will be multiplied with a margin base
        // to  create the margin for the button.
        // This is lazy evaluated as the position of a button will always be the same.
        let getFloatedIndexOfMsg (toFind: Message) (list: Message list) =
            lazy (list |> List.findIndex toFind.Equals |> float)

        DockPanel.create
            [ DockPanel.children
                  [ WrapPanel.create
                        [ WrapPanel.horizontalAlignment HorizontalAlignment.Center
                          WrapPanel.verticalAlignment VerticalAlignment.Center
                          WrapPanel.dock Dock.Bottom
                          WrapPanel.itemWidth squareLength
                          WrapPanel.maxWidth (squareLength * buttonsInColumn)

                          WrapPanel.children[for i = 0 to GameGrid.length - 1 do
                                                 for j = 0 to GameGrid.length - 1 do
                                                     createCellButton { X = i; Y = j }

                                             let buttons = [ Start; Stop; Reset; Next; Save ]

                                             for msg in buttons do
                                                 let margin = marginBase * (getFloatedIndexOfMsg msg buttons).Force()

                                                 createBottomButton (Message.toString msg) margin (fun _ ->
                                                     dispatch msg)

                                             TextBox.create
                                                 [ TextBox.margin ((marginBase * 6.0), 10.0, 0.0, 0.0)
                                                   TextBox.width optionWidth
                                                   TextBox.height optionHeight
                                                   TextBox.text model.name
                                                   TextBox.onTextChanged (ChangeModelName >> dispatch) ]

                                             createBottomButton (Message.toString Load) (marginBase * 6.0) (fun _ ->
                                                 dispatch Load)

                                             createBottomButton (Message.toString Decrease) (marginBase * 7.0) (fun _ ->
                                                 dispatch Decrease)

                                             createBottomButton
                                                 (Steps.toString model.steps)
                                                 (marginBase * 8.0)
                                                 (fun _ -> dispatch ToggleInfinite)

                                             createBottomButton (Message.toString Increase) (marginBase * 9.0) (fun _ ->
                                                 dispatch Increase)

                                             ] ] ] ]
