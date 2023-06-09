# Conway's Game of Life
Implemented in F# as part of a course in functional programming (DVA229).

## User Manual

### Running the program
- Install F# for dotnet.
- Clone the repository into any directory of your choice.
- Navigate into the repository and run “dotnet run”

### Initial setup
An empty grid is loaded on startup. Each tile on the grid can be clicked in order to switch
between a living or dead state of the cell in the tile.
In the bottom right corner are buttons which control how many turns will be taken. The amount
of turns will be infinite by default. This can be changed by either clicking the “+” or “-” buttons or
clicking on the “∞” symbol itself, which represents the current amount of turns left.

### Controlling the game
The game can be started by pressing “Start”. This will run a turn each second until the amount
of turns runs out (unless it’s infinite). Pressing “Stop” stops the game from running. The game
can then be continued if “Start” is pressed again. When the game is stopped the state of the
cells can be changed by clicking on the tiles in the grid.
Pressing “Restart” clears the grid from any living cells and resets the amount of chosen turns.
The “Next” button can be clicked to move the game to the next step with the next generation of
cells. This can be done whether the game is running or not.

### Saving/loading the gamestate
In order to save/load the gamestate the user needs to write the filename into the textbox below
the grid. The user can then choose to save or load a file with the written filename. If a file with
the same name exists when trying to save said file will get overwritten. A file can only be loaded
if the game is not running.
