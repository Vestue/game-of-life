namespace DVA229_Lab4_AvaloniaElmish
open System.Linq.Expressions
open Avalonia.Layout
open Microsoft.VisualBasic.CompilerServices

module Game =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Microsoft.FSharp.Collections
    
     type Cell =
          Dead of int*int
          |Alive of int*int
          
          
     type UserAction =
         ChangeCellState of Cell
         | Start
         | Stop
         | Save
         | Load
          
     type State = Cell list list
     let cellToString (cell: Cell) =
            match cell with
            | Dead (_, _) -> " "
            | Alive (_, _) -> "#"
     
    
        
    let isCellAlive (cell : Cell) =
        match cell with
        | Dead (x, y) -> false
        | Alive (x, y)-> true
        
    let gridHeight = 16
    let gridWidth = 16
    let init : State = List.init gridHeight (fun c -> List.init gridWidth (fun r -> Dead (c, r)))
    let rec update (msg: UserAction) (state : State) : State=
         match msg with
         | ChangeCellState cell (x, y) -> 
         | Start ->
         
         
    let view (state : State) dispatch =
        printfn $"{state}"
        let squareWidth = 25.0
        let squareHeight = 25.0
        let buttonsInColumn = 16.0
        
        
        let createButton (cell: Cell)=
            Button.create [
                Button.width(squareWidth)
                Button.height(squareHeight)
                Button.content(cellToString cell)
                Button.onClick(fun _ -> dispatch UserAction.ChangeCellState cell)
                
            ]
            
        DockPanel.create [
            DockPanel.children [
                WrapPanel.create [
                        WrapPanel.horizontalAlignment HorizontalAlignment.Center
                        WrapPanel.verticalAlignment VerticalAlignment.Center
                        WrapPanel.dock Dock.Bottom
                        WrapPanel.itemWidth squareWidth
                        WrapPanel.maxWidth (squareHeight * buttonsInColumn)
                        WrapPanel.children[
                          for row in state do
                              for cel in row do
                                createButton cel
                               
                    ]
             ]
    ]    ]