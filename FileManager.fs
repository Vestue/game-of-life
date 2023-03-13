module DVA229_Proj_AvaloniaElmish.FileManager

open System
open System.IO

let initialFileName = "..."

let folderPath = __SOURCE_DIRECTORY__ + "/saves"

let saveStringAsFile stringToSave fileName =
    let filePath = Path.Combine(folderPath, fileName)
    File.WriteAllText(filePath, stringToSave)

let getFullFileName (name: String) = name + ".rog" // rog = Ragnar Oscar Grid

let saveModel (model: Model) =
    match model.name with
    | "" -> Error "File needs to have a name"
    | name when name = initialFileName -> Error "File has no given name"
    | _ ->
        saveStringAsFile (GameGrid.toString model.grid) (getFullFileName model.name)
        Ok { model with name = "" } // Clear the filename to give feedback to the user that it has been saved


let loadModel (model: Model) =
    match model.name with
    | name when name = initialFileName -> Error "No given file name to load"
    | _ ->
        let filePath = Path.Combine(folderPath, getFullFileName model.name)
        let modelString = File.ReadLines(filePath) |> Seq.head
        let loadedGrid = GameGrid.fromString modelString

        Ok
            { grid = loadedGrid
              state = Stopped
              steps = Infinite
              name = "" }
