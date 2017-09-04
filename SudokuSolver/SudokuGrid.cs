using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver
{
	public class SudokuGrid
	{
		public enum SolveType
		{
			Unsolved,
			Entered,
			PlaceElimination, //red
			PossibilityElimination, //blue
			Invalid //red background
		}

		//helper class
		//@TODO: make private when done pulling solve logic from Form1
		//@TODO: clean up
		public class gridSquare
		{
			int x; //these might not be needed, but they can stay for now
			int y;

			//known value 1-9, 0 for undecided.
			int knownValue;
			//possibilities array.
			//possibilities[0] is unused
			//possibilities[n] represents if it is possible for the square at x,y to be the number n(1-9)
			private bool[] possibilities;

			//number of possibilites remaining
			int possCount;

			//set whenever possibilities at x,y are changes, cleared whenever x,y is checked.
			bool recheck;

			public SudokuGrid.SolveType solveType { get; set; }

			public gridSquare(int x, int y)
			{
				this.x = x;
				this.y = y;

				knownValue = 0;

				possibilities = new bool[9 + 1];//possibilities[0] is not used, possibilities[1->9] is possible values (true/false)
				for (int i = 1; i <= 9; ++i)
					possibilities[i] = true;
				possCount = 9;

				recheck = false; //nothing needs checking to start with
				solveType = SudokuGrid.SolveType.Unsolved;
			}

			public gridSquare(gridSquare toCopy)
			{
				possibilities = new bool[9 + 1];
				this.copy(toCopy);
			}

			public void copy(gridSquare toCopy)
			{
				this.x = toCopy.x;
				this.y = toCopy.y;

				this.knownValue = toCopy.knownValue;
				for (int i = 1; i <= 9; ++i)
					this.possibilities[i] = toCopy.possibilities[i];
				this.possCount = toCopy.possCount;

				this.recheck = toCopy.recheck;
				this.solveType = toCopy.solveType;
			}

			public void reset()
			{
				knownValue = 0;

				for (int i = 1; i <= 9; ++i)
					possibilities[i] = true;
				possCount = 9;

				recheck = false; //nothing needs checking to start with
				solveType = SudokuGrid.SolveType.Unsolved;
			}

			public int X
			{
				get { return x; }
			}

			public int Y
			{
				get { return y; }
			}

			public int KnownValue
			{
				get { return knownValue; }
				set
				{
					if (!isPossible(value))
						throw new Exception("nope");
					if (knownValue == value)
						throw new Exception("wasting time"); //TODO: replace with simple return
					knownValue = value;
					recheck = true;

					//eliminate all other possibilities
					for (int i = 1; i < 10; i++)
						eliminate(i);
				}
			}

			public bool Recheck
			{
				get { return recheck; }
			}

			public void setRecheck()
			{ recheck = true; }

			public void clearRecheck()
			{ recheck = false; }

			public bool isPossible(int i)
			{ return possibilities[i]; }

			//returns true if eliminate changed cell
			public bool eliminate(int i)
			{
				if (knownValue != i)
				{
					if (possibilities[i]) //it was listed as possible before
					{
						--possCount;
						recheck = true;
						possibilities[i] = false;
						if (possCount == 0)
							solveType = SolveType.Invalid;
						return true;
					}

				}
				return false;
			}

			public int PossibilityCount
			{
				get { return possCount; }
			}

			public string PossibilityString
			{
				get
				{
					string ret = "";
					for (int i = 1; i < 10; i++)
						if (isPossible(i))
							ret += " " + i;
					return ret;

				}
			}
		}


		public SudokuGrid()
		{
			solveGrid = new gridSquare[9, 9];

			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					solveGrid[i, j] = new gridSquare(i, j);
				}
			}
		}

		public SudokuGrid(SudokuGrid toCopy)
		{
			solveGrid = new gridSquare[9, 9];

			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					solveGrid[i, j] = new gridSquare(toCopy.solveGrid[i, j]);
				}
			}
		}

		private gridSquare[,] solveGrid;

		/// <summary>
		/// index access to known values
		/// </summary>
		/// <param name="x">x index, 0-8</param>
		/// <param name="y">y  index, 0-8</param>
		/// <returns>solved value 1-9, or 0 when unknown</returns>
		public int this[int x, int y]
		{
			//@TODO: validate all parameters
			get { return solveGrid[x, y].KnownValue; }
		}

		/// <summary>
		/// Sets the known value in the grid, if possible. Updates possibilities, but does no other solving
		/// </summary>
		/// <param name="x">x index, 0-8</param>
		/// <param name="y">y index, 0-8</param>
		/// <param name="value">known value, 1-9</param>
		/// <returns>true if value was accepted</returns>
		public bool setKnownValue(int x, int y, int value)
		{
			//@TODO: validate all parameters
			//validate input, but don't try to solve
			if (solveGrid[x, y].KnownValue == value)
				return true; //nothing to do
			if (solveGrid[x, y].isPossible(value))
			{
				solveGrid[x, y].solveType = SolveType.Entered;
				solveGrid[x, y].KnownValue = value;
				eliminate(x, y);
				return true;
			}

			return false;
		}

		public SolveType solveType(int x, int y)
		{
			return solveGrid[x, y].solveType;
		}

		public String PossibilityString(int x, int y)
		{
			return solveGrid[x, y].PossibilityString;
		}

		//eliminate other grid squares based on known value of solveGrid[x,y]
		private void eliminate(int x, int y)
		{
			int val = solveGrid[x, y].KnownValue;
			//eliminate value across its row, col
			for (int i = 0; i < 9; i++)
			{
				solveGrid[x, i].eliminate(val);
				solveGrid[i, y].eliminate(val);
			}
			//eliminate value in its square
			for (int i = x - x % 3; i < x - x % 3 + 3; i++)
				for (int j = y - y % 3; j < y - y % 3 + 3; j++)
				{
					solveGrid[i, j].eliminate(val);
				}
		}

		/// <summary>
		/// Some optimizations to shortcut redundant checks could have bugs that prevent further solving.
		/// Reset all of those to allow retesting to show any such  bugs.
		/// </summary>
		public void dubug_resetOptimizations()
		{
			//TODO:reset any optimizations for testing
			throw new NotImplementedException();
		}

		/// <summary>
		/// Solve as much of the puzzle as possible from current information
		/// </summary>
		/// <returns> true only if more of the puzzle was solved</returns>
		public bool solve()
		{
			bool madeChanges = false;
			bool madeIterationChanges;
			do
			{
				madeIterationChanges = false;

				//Eliminations (could be done in KnowValue Set, but then unchecking Autosolve wouldn't completly work.)
				madeIterationChanges = possibilityEliminationScan();

				//placement eliminations
				madeIterationChanges |= placementEliminationScan();

				//etc...

				//set return value
				if (madeIterationChanges)
					madeChanges = true;
			} while (madeIterationChanges == true);

			return madeChanges;
		}

		private bool possibilityEliminationScan()
		{
			bool madeChanges = false;
			bool madeIterationChanges;
			do
			{
				madeIterationChanges = false;

				//Eliminations (could be done in KnowValue Set, but then unchecking Autosolve wouldn't completly work.)
				for (int i = 0; i < 9; i++)
				{
					for (int j = 0; j < 9; j++)
					{
						//check if possibility elimination applies
						if (/*solveGrid[i, j].Recheck &&*/ solveGrid[i, j].KnownValue == 0 && solveGrid[i, j].PossibilityCount == 1)
						{
							//find and save the remaining possibility
							for (int n = 1; n <= 9; n++)
							{
								if (solveGrid[i, j].isPossible(n))
								{
									//solveGrid[i, j].KnownValue = n;
									setKnownValue(i, j, n);
									solveGrid[i, j].solveType = SolveType.PossibilityElimination;
									break;
								}
							}
							madeIterationChanges = true;
						}

					}
				}

				//set return value
				if (madeIterationChanges)
					madeChanges = true;
			} while (madeIterationChanges == true);

			return madeChanges;
		}

		private bool placementEliminationScan()
		{
			bool madeChanges = false;
			//don't loop inside this one. it's more expensive than eliminationScan and will be looped back to again if anything changes //TODO check if should loop

			//First check by Cols
			for (int X = 0; X < 9; X++) //check each col
			{
				//check each number
				for (int val = 1; val <= 9; val++) //find which digits to place detect for
				{
					//TODO: save which digits are done per col and shortcut past those.
					int sum = 0;
					int savedY = 0;
					for (int Y = 0; Y < 9; Y++) //check in cell in the X col for the number val
					{
						if (solveGrid[X, Y].isPossible(val))
						{
							sum++;
							savedY = Y; //remember the index of any postition of val in case it's the only one in the set
							if (solveGrid[X, Y].KnownValue == val) //val has already been found for this row
							{
								//stop trying to find it
								sum = -1;
								break;
							}
							if (sum > 1)
								break; //only works if sum equals 1, give up on current val
						}
					}
					if (sum == 1)
					{
						//there is definitly a single valid spot for that value, put it there
						setKnownValue(X, savedY, val);
						madeChanges = true;
						solveGrid[X, savedY].solveType = SudokuGrid.SolveType.PlaceElimination;
					}
				}
			}

			//Next check by Rows
			for (int Y = 0; Y < 9; Y++) //check each row
			{
				//check each number
				for (int val = 1; val <= 9; val++) //find which digits to place detect for
				{
					//TODO: save which digits are done per row and shortcut past those.
					int sum = 0;
					int savedX = 0;
					for (int X = 0; X < 9; X++) //check in cell in the X col for the number val
					{
						if (solveGrid[X, Y].isPossible(val))
						{
							sum++;
							savedX = X; //remember the index of any postition of val in case it's the only one in the set
							if (solveGrid[X, Y].KnownValue == val) //val has already been found for this row
							{
								//stop trying to find it
								sum = -1;
								break;
							}
							if (sum > 1)
								break; //only works if sum equals 1, give up on current val
						}
					}
					if (sum == 1)
					{
						//there is definitly a single valid spot for that value, put it there
						setKnownValue(savedX, Y, val);
						madeChanges = true;
						solveGrid[savedX, Y].solveType = SudokuGrid.SolveType.PlaceElimination;
					}
				}
			}

			//last check by boxes
			for (int bY = 0; bY < 3; bY++)
			{
				for (int bX = 0; bX < 3; bX++) //check each of  the 9 boxes
				{
					//check each number
					for (int val = 1; val <= 9; val++) //find which digits to place detect for
					{
						//TODO: save which digits are done per box and shortcut past those.
						int sum = 0;
						int savedX = 0;
						int savedY = 0;
						for (int X = bX * 3; X < (bX + 1) * 3; X++)
						{
							for (int Y = bY * 3; Y < (bY + 1) * 3; Y++) //check in cell in the bx,bY box for the number val
							{
								if (solveGrid[X, Y].isPossible(val))
								{
									sum++;
									savedX = X; //remember the index of a postition of val in case it's the only one in the set
									savedY = Y;
									if (solveGrid[X, Y].KnownValue == val) //val has already been found for this box
									{
										//stop trying to find it
										sum = -1;
										break;
									}
								}
								if (sum > 1)
									break; //only does work if sum equals 1, give up
							}
						}
						if (sum == 1)
						{
							//there is definitly a single valid spot for that value, put it there
							setKnownValue(savedX, savedY, val);
							madeChanges = true;
							solveGrid[savedX, savedY].solveType = SudokuGrid.SolveType.PlaceElimination;
						}
					}
				}
			}
			return madeChanges;
		}
	}
}
