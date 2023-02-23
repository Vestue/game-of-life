namespace DVA229_Lab4_AvaloniaElmish

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

    type Tile = { Cell: Cell; Coordinate: Position }
    type State = Cell[,]

    let cellToString (state: State) (coords: UserAction) =
        match coords with
        | ChangeCellState pos ->
            match state[pos.X, pos.Y] with
            | Dead -> " "
            | Alive -> "#"
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
            Array2D.init gridHeight gridWidth (fun x y ->
                if x = coordinates.X && y = coordinates.Y then
                    match state[coordinates.X, coordinates.Y] with
                    | Alive -> Cell.Dead
                    | Dead -> Cell.Alive
                else
                    state[x, y])

        newState

    let rec update (msg: UserAction) (state: State) : State =
        match msg with
        | Start -> state
        | Stop -> state
        | Reset -> init
        | ChangeCellState pos -> flipCellState pos state
        | _ -> state

    let view (state: State) dispatch =
        printfn $"{state}"
        let squareWidth = 25.0
        let squareHeight = 25.0
        let buttonsInColumn = 16.0

        let createButton (cellCoordinates: UserAction) =
            Button.create
                [ Button.width squareWidth
                  Button.height squareHeight
                  Button.content (cellToString state cellCoordinates)
                  Button.onClick (fun _ -> dispatch cellCoordinates)
                  ]

        DockPanel.create
            [ DockPanel.children
                  [ WrapPanel.create
                        [ WrapPanel.horizontalAlignment HorizontalAlignment.Center
                          WrapPanel.verticalAlignment VerticalAlignment.Center
                          WrapPanel.dock Dock.Bottom
                          WrapPanel.itemWidth squareWidth
                          WrapPanel.maxWidth (squareHeight * buttonsInColumn)

                          WrapPanel.children[for i = 0 to state.GetLength(0) - 1 do
                                                 for j = 0 to state.GetLength(1) - 1 do
                                                     createButton (ChangeCellState { Position.X = i; Position.Y = j })


                                             Button.create
                                                 [ Button.width 50.0; Button.height 25.0; Button.content "Start" ]

                                             Button.create
                                                 [ Button.margin 25.0
                                                   Button.width 50.0
                                                   Button.height 25.0
                                                   Button.content "Stop" ]

                                             Button.create
                                                 [ Button.margin 50.0
                                                   Button.width 50.0
                                                   Button.height 25.0
                                                   Button.content "Reset"
                                                   Button.onClick (fun _ -> dispatch UserAction.Reset) ]




                                             ] ]



                    ]

              ]
