namespace DVA229_Proj_AvaloniaElmish

open System
open System.Linq.Expressions
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
        | Stop
        | Save
        | Load
        | Reset

    type GridState =
        | Running
        | Stopped

    type State = Cell[,]

    let cellToString (state: State) (coords: UserAction) =
        match coords with
        | ChangeCellState pos ->
            match state[pos.X, pos.Y] with
            | Dead -> " "
            | Alive -> "■"
        | _ -> ""

    let isCellAlive (cell: Cell) =
        match cell with
        | Dead -> false
        | Alive -> true

    let gridHeight = 16
    let gridWidth = 16

    let init: State = Array2D.create gridWidth gridHeight Cell.Dead

    let flipCellState (coordinates: Position) (state: State) : State =
        let newState: Cell[,] =
            Array2D.init gridWidth gridHeight (fun x y ->
                if x = coordinates.X && y = coordinates.Y then
                    match state[coordinates.X, coordinates.Y] with
                    | Alive -> Cell.Dead
                    | Dead -> Cell.Alive
                else
                    state[x, y])

        newState

    let getNeighbours (state: State) (xCoord: int) (yCoord: int) : Cell list =
        let rowRange = { max 0 (xCoord - 1) .. min (gridWidth - 1) (xCoord + 1) }
        let colRange = { max 0 (yCoord - 1) .. min (gridHeight - 1) (yCoord + 1) }

        [ for row in rowRange do
              for col in colRange do
                  if row <> xCoord || col <> yCoord then
                      yield state[row, col] ]

    let getLiveNeighbours (state: State) (xCoord: int) (yCoord: int) : int =
        let neighbours = getNeighbours state xCoord yCoord

        let rec loop neighbours amount =
            match neighbours with
            | [] -> amount
            | h :: t ->
                match h with
                | Alive -> loop t amount + 1
                | Dead -> loop t amount

        loop neighbours 0

    let generateNextGeneration (state: State) : State =
        let newGen: Cell[,] =
            Array2D.init gridWidth gridHeight (fun x y ->
                match getLiveNeighbours state x y, state[x, y] with
                | x, Dead when x = 3 -> Alive
                | x, Alive when x = 3 || x = 2 -> Alive
                | _, _ -> Dead)

        newGen

    let rec update (msg: UserAction) (state: State) : State =
        match msg with
        | Start -> generateNextGeneration state
        | Stop -> state
        | Reset -> init
        | ChangeCellState pos -> flipCellState pos state
        | _ -> state

    let view (state: State) dispatch =
        printfn $"{state}"

        // Sizes for UI
        let squareLength = 30.0
        let buttonsInColumn = 16.0
        let optionHeight = 30.0
        let optionWidth = 60.0
        
        let createButton (cellCoordinates: UserAction) =
            Button.create
                [ Button.width squareLength
                  Button.height squareLength
                  Button.content (cellToString state cellCoordinates)
                  Button.onClick (fun _ -> dispatch cellCoordinates) ]


        DockPanel.create
            [ DockPanel.children
                  [ WrapPanel.create
                        [ WrapPanel.horizontalAlignment HorizontalAlignment.Center
                          WrapPanel.verticalAlignment VerticalAlignment.Center
                          WrapPanel.dock Dock.Bottom
                          WrapPanel.itemWidth squareLength
                          WrapPanel.maxWidth (squareLength * buttonsInColumn)

                          WrapPanel.children[for i = 0 to state.GetLength(0) - 1 do
                                                 for j = 0 to state.GetLength(1) - 1 do
                                                     createButton (ChangeCellState { Position.X = i; Position.Y = j })

                                             Button.create
                                                 [ Button.width optionWidth
                                                   Button.height optionHeight
                                                   Button.content "Start"
                                                   Button.onClick (fun _ -> dispatch UserAction.Start) ]

                                             Button.create
                                                 [ Button.margin 35
                                                   Button.width optionWidth
                                                   Button.height optionHeight
                                                   Button.content "Stop" ]

                                             Button.create
                                                 [ Button.margin 70
                                                   Button.width optionWidth
                                                   Button.height optionHeight
                                                   Button.content "Reset"
                                                   Button.onClick (fun _ -> dispatch UserAction.Reset) ]] ] ] ]
