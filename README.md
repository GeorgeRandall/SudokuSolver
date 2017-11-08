# SudokuSolver
Toy C# application in Visual Studio for automatically solving Sudoku puzzles.

This program is written as a personal project to solve Sudoku puzzles using only logical constraint algorithms.  The end result could very well end up being slower than a brute force approach and will almost certainly be slower than an optimized Exact Cover algorithm like Knuth's Algorithm X. This project remains a work in progress.

### Usage
The project can be compiled in Visual Studio 2010 (and I assume later versions) or downloaded as an executable release. To use it, simply type in a Sudoku puzzle (moving cells with mouse or arrow keys). The solver will update the puzzle as you type, and may finish solving it before you have finished input. Reset clears the puzzle, and Undo undoes typed inputs. Auto Solve can be disabled by unchecking it. The basic constraints are still applied, but no other logic is. Which numbers are know by the solver to still be possible for each square can be displayed by checking Possibilities.

### Debugging GUI
There are not yet separate debug and release configurations, so debug tools are accessed by the checkbox in the lower right-hand corner. The snapshot buttons save and restore the solver's exact current state to a stack. The Recheck button removes any cached optimizations (if any have been added) and reruns the solver; this *should* have no effect on the solver's state. 

The Test Case button causes the solver to load the hard-coded test case corresponding to the number in the adjacent numeric up/down. If Auto Solve is not checked, it does not solve. The time "Elapsed" during the solve is displayed in the debugging text box. Test case -1 is a special case and runs will run ALL programmed test cases 100 times each (ignoring Auto Solve), add the solving time (but not any setup time) all together and display the result in the debug text field. This may take a few minutes to run. This should make comparing efficiency when optimizing easier.

Copy Test and Paste Test are for convenience after manually entering a puzzle, a string corresponding to it can be copied to the clipboard with Copy Test. This string can be pasted into a new testCase in the Form1.cs hardcoded test case array. It can also be saved and pasted back into the solver with Paste Test. The format of the string is 3 lines of c# code initializing parallel arrays for X coordinate, Y coordinate, and number value of a number in the Sudoku grid. For examples, enter a Sudoku and press Copy Test.
