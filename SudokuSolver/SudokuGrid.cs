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
		//@TODO: clean up
		private class gridSquare
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

			//set whenever possibilities at x,y are changed, cleared whenever x,y is checked. TODO: use for future optimizations? (or remove)
			bool recheck;

			//set of possible combinations
			HashSet<HashSet<int>> setList;

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

				initSetList();

				recheck = false; //nothing needs checking to start with
				solveType = SudokuGrid.SolveType.Unsolved;
			}

			private void initSetList()
			{
				//fill out setList
				setList = new HashSet<HashSet<int>>(new gridSquare.SetOfIntEqualityComparer());
				HashSet<int> temp;
				for (int i1 = 1; i1 <= 9; i1++)
				{
					for (int i2 = i1+1; i2 <= 9; i2++)
					{
						//Add pairs
						temp = new HashSet<int>();
						temp.Add(i1);
						temp.Add(i2);
						setList.Add(temp);
						////add larger sets TODO: determine how much larger of sets to add. Higher order sets add to compute time and are less likely to advance a solution
						//for (int i3 = i2 + 1; i3 <= 9; i3++)
						//{
						//    //Add triples
						//    temp = new HashSet<int>();
						//    temp.Add(i1);
						//    temp.Add(i2);
						//    temp.Add(i3);
						//    setList.Add(temp);
						//    //TODO: add larger sets?
						//}
					}
				}
			}

			public gridSquare(gridSquare toCopy)
			{
				possibilities = new bool[9 + 1];
				this.copy(toCopy);
			}

			private void copy(gridSquare toCopy)
			{
				this.x = toCopy.x;
				this.y = toCopy.y;

				this.knownValue = toCopy.knownValue;
				for (int i = 1; i <= 9; ++i)
					this.possibilities[i] = toCopy.possibilities[i];
				this.possCount = toCopy.possCount;

				this.recheck = toCopy.recheck;
				this.solveType = toCopy.solveType;
				this.setList = new HashSet<HashSet<int>>(toCopy.setList);
			}

			public void reset()
			{
				knownValue = 0;

				for (int i = 1; i <= 9; ++i)
					possibilities[i] = true;
				possCount = 9;

				initSetList();

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
						else
							setList.RemoveWhere(s => s.Contains(i));
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

			public HashSet<HashSet<int>> SetList
			{
				get
				{
					return setList; //TODO: refactor to not provide full access to member? This helper class is private to SudokuGrid class...
				}
			}

			public class SetOfIntEqualityComparer : IEqualityComparer<HashSet<int>>
			{
				public bool Equals(HashSet<int> s1, HashSet<int> s2)
				{
					return s1.SetEquals(s2);
				}

				public int GetHashCode(HashSet<int> s)
				{
					return s.GetHashCode();
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
		/// access sqaures by which box they are in and which square in the box
		/// to allow simple iteration by box
		/// </summary>
		/// <param name="box">which box, 0-8</param>
		/// <param name="square">which square in that box, 0-8</param>
		/// <returns>gridSquare that matches that location</returns>
		private gridSquare boxCoords(int box, int square)
		{
			int bx = box % 3;
			int by = box / 3;

			int sx = square % 3;
			int sy = square / 3;

			//convert to full array coords
			int X = bx * 3 + sx;
			int Y = by * 3 + sy;

			return solveGrid[X, Y];
		}

		/// <summary>
		/// access sqaures by which column they are in and which square in the column
		/// to allow simple iteration by column
		/// </summary>
		/// <param name="box">which column, 0-8</param>
		/// <param name="square">which square in that column, 0-8</param>
		/// <returns>gridSquare that matches that location</returns>
		private gridSquare colCoords(int col, int square)
		{
			return solveGrid[col, square];
		}

		/// <summary>
		/// access sqaures by which row they are in and which square in the row
		/// to allow simple iteration by box
		/// </summary>
		/// <param name="box">which row, 0-8</param>
		/// <param name="square">which square in that row, 0-8</param>
		/// <returns>gridSquare that matches that location</returns>
		private gridSquare rowCoords(int row, int square)
		{
			return solveGrid[square, row];
		}

		private enum iterateBy
		{
			Box,
			Row,
			Col
		}

		/// <summary>
		/// Accesses solveGrid by selected iterateBy type
		/// </summary>
		/// <param name="iter">type of access coordinate converstion</param>
		/// <param name="section">which box, col or row</param>
		/// <param name="square">which square of that section</param>
		/// <returns>gridSquare that matches that location</returns>
		private gridSquare iterCoords(iterateBy iter, int section, int square)
		{
			switch (iter)
			{
				case iterateBy.Box:
					return boxCoords(section, square);
				case iterateBy.Col:
					return colCoords(section, square);
				case iterateBy.Row:
					return rowCoords(section, square);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

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

			//check by each section type
			foreach (iterateBy iterType in Enum.GetValues(typeof(iterateBy)))
			{
				//check each section
				for (int sec = 0; sec < 9; sec++) //check each col
				{
					//check each number
					for (int val = 1; val <= 9; val++) //find which digits to place detect for
					{
						//TODO: save which digits are done per section and shortcut past those.
						int sum = 0;
						int savedEl = 0;
						for (int el = 0; el < 9; el++) //check each element in the section for the number val
						{
							if (iterCoords(iterType, sec, el).isPossible(val))
							{
								sum++;
								savedEl = el; //remember the element index of any postition of val in case it's the only one in the section
								if (iterCoords(iterType, sec, el).KnownValue == val) //val has already been found for this section
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
							int x = iterCoords(iterType, sec, savedEl).X;
							int y = iterCoords(iterType, sec, savedEl).Y;
							setKnownValue(x, y, val);
							solveGrid[x, y].solveType = SudokuGrid.SolveType.PlaceElimination; //override default SolveType
							madeChanges = true;
						}
					}
				}
			}
			return madeChanges;
		}
	}
}
