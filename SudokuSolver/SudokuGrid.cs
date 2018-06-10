using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver
{
	/// <summary>
	/// Class to contain and solve a sudoku puzzle.
	/// </summary>
	public class SudokuGrid
	{
		public enum SolveType
		{
			Unsolved,
			Entered,
			PlaceElimination, //blue (or red in debug mode)
			PossibilityElimination, //blue
			Invalid //red background
		}

		/// <summary>
		/// Create and maintain a HashSet of HashSets representing possible combinations.
		/// </summary>
		private class PossibilitySet : HashSet<HashSet<int>>
		{
			/// <summary>
			/// Basic constructor. Populates this with sets for all unordered
			/// permutations of Elements up to maxLen in length.
			/// </summary>
			/// <param name="elements">Elements to combine into sets</param>
			/// <param name="maxLen">Max length of sets. </param>
			public PossibilitySet(HashSet<int> elements, int maxLen)
			{
				fillsets(elements, maxLen);
				//remove the singletons that where added by fillsets().
				//Adding them is essential to filling out the sets, but they serve no further purpose.
				//See note in singlePlacementScan()
				this.RemoveWhere(s => s.Count == 1);
			}

			//copy constructor
			public PossibilitySet(PossibilitySet toCopy)
			{
				foreach (HashSet<int> set in toCopy)
				{
					Add(new HashSet<int>(set));
				}
			}

			//populate this with a set for every permutation of elements that contains maxLen or less elements.
			// Works recursively to combine elements[0] with all existing permuations already stored in this,
			// removing elements[0], then calling itself with the reduced elements set.
			private void fillsets(HashSet<int> elements, int maxLen)
			{
				//extract first element from elements
				int i = elements.ElementAt(0);
				elements.Remove(i);

				//set up workspace (cannot alter <this> while iterating without breaking iterator)
				HashSet<HashSet<int>> workingSet = new HashSet<HashSet<int>>();

				//add singleton of first element
				HashSet<int> temp = new HashSet<int> { i };
				workingSet.Add(temp);

				//add all permutations (that don't exceed maxLen) of first element with existing contents of set
				foreach (HashSet<int> set in this)
				{
					if (set.Count < maxLen)
					{
						temp = new HashSet<int>(set);
						temp.Add(i);
						workingSet.Add(temp);
					}
				}
				this.UnionWith(workingSet);

				//recursive call to fill set with all permutations including remaining elements
				if (elements.Count != 0)
				{
					fillsets(elements, maxLen);
				}
			}

			/// <summary>
			/// Eliminates all sets that contain i
			/// </summary>
			/// <param name="i">value to eleminate all sets of</param>
			public void eliminate(int i)
			{
				RemoveWhere(s => s.Contains(i));
			}

			//simple comparer for use in Contains()
			private class SetOfIntEqualityComparer : IEqualityComparer<HashSet<int>>
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

			/// <summary>
			/// Determines whether a sequence contains a specified
			/// element by using the special built in IEqualityComparer.
			/// </summary>
			/// <param name="value">The value to locate</param>
			/// <returns>true if the source sequence contains an element that has the specified value;
			//     otherwise, false.</returns>
			public new bool Contains(HashSet<int> value)
			{
				return ((HashSet<HashSet<int>>)this).Contains(value, new SetOfIntEqualityComparer());
			}

			/// <summary>
			/// removes all sets containing the value
			/// </summary>
			/// <param name="value">the value to remove</param>
			public void removeAllContaining(int value)
			{
				RemoveWhere(s => s.Contains(value));
			}
		}

		//helper class
		private class gridSquare
		{
			//Used to allow isBuddy check and could have further use
			int x;
			int y;

			//known value 1-9, 0 for undecided.
			int knownValue;
			//possibilities array:
			//possibilities[0] is unused
			//possibilities[n] represents if it is possible for the square at x,y to be the number n(1-9)
			private bool[] possibilities;

			//set of all individual possibilities
			List<int> possList;

			//set of all possible combinations of remaining possibilities
			PossibilitySet setList;

			//keep track of the method by which this gridSquare was solved
			public SudokuGrid.SolveType solveType { get; set; }

			public gridSquare(int x, int y)
			{
				this.x = x;
				this.y = y;

				//Storing information about possibilities in different ways to speed up different kinds of access.
				possibilities = new bool[9 + 1];//possibilities[0] is not used, possibilities[1->9] is possible values (true/false)
				setToInitialState();
			}

			//initialize/return to freshly constructed state
			public void setToInitialState()
			{
				knownValue = 0;

				for (int i = 1; i <= 9; ++i)
					possibilities[i] = true;
				possList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

				setList = new PossibilitySet(new HashSet<int>(possList), maxSetLength);

				solveType = SudokuGrid.SolveType.Unsolved;
			}

			//May want to change this to a class parameter at some future date,
			// but for now it is hardcoded. This limits the size of set that can be
			// detected in SudokuGrid.matchedSetEliminationScan(). Size 4 are rare
			// but can occur. Larger values are meaningless, because a set of size 5
			// or higher would always have a reciprical set of size 4 or lower.
			private const int maxSetLength = 4;

			//copy constructor. does a deep copy.
			public gridSquare(gridSquare toCopy)
			{
				possibilities = new bool[9 + 1];
				this.copy(toCopy);
			}

			//Make this instance a deep copy of toCopy
			private void copy(gridSquare toCopy)
			{
				this.x = toCopy.x;
				this.y = toCopy.y;

				this.knownValue = toCopy.knownValue;
				for (int i = 1; i <= 9; ++i)
					this.possibilities[i] = toCopy.possibilities[i];
				this.possList = new List<int>(toCopy.possList);

				this.solveType = toCopy.solveType;
				this.setList = new PossibilitySet(toCopy.setList);
			}

			public int X
			{
				get { return x; }
			}

			public int Y
			{
				get { return y; }
			}

			//the value that this GridSquare is known to contain
			// or 0 if no value is known for certain
			public int KnownValue
			{
				get { return knownValue; }
				set
				{
					if (!isPossible(value))
						throw new Exception("invalid KnownValue");
					if (value < 1 || value > 9)
						throw new Exception("known value out of range error");
					if (knownValue == value)
					{
#if _debug
						throw new Exception("wasting time");
#else
						return;
#endif
					}
					knownValue = value;

					//eliminate all other possibilities
					for (int i = 1; i < 10; i++)
						eliminate(i);
				}
			}

			//returns true when i has not been eliminated as
			// a possbility for this GridSquare
			public bool isPossible(int i)
			{ return possibilities[i]; }

			//eliminate i as a possibility
			// returns true if doing so altered the state of this
			public bool eliminate(int i)
			{
				if (knownValue != i)
				{
					if (possibilities[i]) //it was listed as possible before
					{
						possList.Remove(i);
						possibilities[i] = false;
						if (PossibilityCount == 0)
							solveType = SolveType.Invalid;
						else
							setList.removeAllContaining(i);
						return true;
					}

				}
				return false;
			}

			//number of values that have not been eliminated
			public int PossibilityCount
			{
				get { return possList.Count; }
			}

			//returns a formatted string listing all remaining possibilities
			public string PossibilityString
			{
				get
				{
					string ret = "";
					for (int i = 0; i < possList.Count; i++)
						ret += " " + possList[i];
					return ret;

				}
			}

			//Set of possibility permutations that have not been eliminated
			internal PossibilitySet SetList
			{
				get
				{
					//Yes, this gives access to a private member, but it can only be accessed from SudokuGrid class.
					// Not ideal, but copying it would be too expensive.
					return setList;
				}
			}

			//Set of individual values that have not been eliminated
			internal List<int> PossibilityList
			{
				get
				{
					//Yes, this gives access to a private member, but it can only be accessed from SudokuGrid class.
					// Not ideal, but copying it would be too expensive.
					return possList;
				}
			}

			//returns true when this is a buddy of g
			// A buddy is a square that has a row, column or box in common.
			public bool isBuddy(gridSquare g)
			{
				if (X == g.X || Y == g.Y)
					return true;
				if (Y / 3 == g.Y / 3 && X / 3 == g.X / 3)
					return true;
				return false;
			}
		}

		/// <summary>
		/// Instantiate a blank SudokuGrid.
		/// </summary>
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

			setUnsolvedValuesToInitialState();
		}

		private void setUnsolvedValuesToInitialState()
		{
			//start with all values unsolved
			HashSet<int> temp = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			unsolvedValues = new HashSet<int>[3, 9]; //3 types of section, nine of each type
			for (int i = 0; i < 9; i++)
			{
				unsolvedValues[(int)iterateBy.Box, i] = new HashSet<int>(temp);
				unsolvedValues[(int)iterateBy.Row, i] = new HashSet<int>(temp);
				unsolvedValues[(int)iterateBy.Col, i] = new HashSet<int>(temp);
			}
		}

		/// <summary>
		/// Copy constructor. Does a deep copy.
		/// </summary>
		/// <param name="toCopy">instance to copy</param>
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

			unsolvedValues = new HashSet<int>[3, 9]; //3 types of section (row, col, box), nine of each type
			for (int i = 0; i < 9; i++)
			{
				unsolvedValues[(int)iterateBy.Box, i] = new HashSet<int>(toCopy.unsolvedValues[0, i]);
				unsolvedValues[(int)iterateBy.Row, i] = new HashSet<int>(toCopy.unsolvedValues[1, i]);
				unsolvedValues[(int)iterateBy.Col, i] = new HashSet<int>(toCopy.unsolvedValues[2, i]);
			}
		}

		//the 9x9 sudoku grid
		private gridSquare[,] solveGrid;
		//track what values each section has remaining to solve
		private HashSet<int>[,] unsolvedValues;

		/// <summary>
		/// access sqaures by which box they are in and which square in the box
		/// to allow simple iteration by box
		/// </summary>
		/// <param name="box">which box, 0-8 (order is arbitrary, but fixed)</param>
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
			Box = 0,
			Row = 1,
			Col = 2
		}

		/// <summary>
		/// Accesses solveGrid by selected iterateBy type
		/// </summary>
		/// <param name="iter">type of access coordinate conversion (box, col or row)</param>
		/// <param name="section">which section of that type (0-8)</param>
		/// <param name="square">which square of that section (0-8)</param>
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
		/// index style access to known values
		/// </summary>
		/// <param name="x">x index, 0-8</param>
		/// <param name="y">y  index, 0-8</param>
		/// <returns>solved value 1-9, or 0 when unknown</returns>
		public int this[int x, int y]
		{
			get
			{
				if (x < 0 || x > 8)
					throw new IndexOutOfRangeException();
				if (y < 0 || y > 8)
					throw new IndexOutOfRangeException();
				return solveGrid[x, y].KnownValue;
			}
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
			if (x < 0 || x > 8)
				throw new IndexOutOfRangeException();
			if (y < 0 || y > 8)
				throw new IndexOutOfRangeException();
			if (value < 1 || value > 9)
				throw new ArgumentOutOfRangeException();

			//validate input, but don't try to solve
			if (solveGrid[x, y].KnownValue == value)
				return true; //nothing to do
			if (solveGrid[x, y].isPossible(value))
			{
				solveGrid[x, y].solveType = SolveType.Entered;
				solveGrid[x, y].KnownValue = value;
				eliminate(x, y);
				//Remove value from solved lists for this square's sections
				unsolvedValues[(int)iterateBy.Box, y - y % 3 + x / 3].Remove(value);
				unsolvedValues[(int)iterateBy.Row, y].Remove(value);
				unsolvedValues[(int)iterateBy.Col, x].Remove(value);

				return true;
			}

			return false;
		}

		/// <summary>
		/// Get the solve type of a particular grid square
		/// </summary>
		/// <param name="x">x coordinate of grid square (0-8)</param>
		/// <param name="y">y coordinate of grid square (0-8)</param>
		/// <returns>SolveType of requested grid square </returns>
		public SolveType solveType(int x, int y)
		{
			return solveGrid[x, y].solveType;
		}

		/// <summary>
		/// Creates a formatted string listing all remaining possibilities of a particular grid square
		/// </summary>
		/// <param name="x">x coordinate of grid square (0-8)</param>
		/// <param name="y">y coordinate of grid square (0-8)</param>
		/// <returns>String of possibilities for requested grid square </returns>
		public String PossibilityString(int x, int y)
		{
			return solveGrid[x, y].PossibilityString;
		}

		//eliminate other grid squares based on known value of solveGrid[x,y]
		private void eliminate(int x, int y)
		{
			int val = solveGrid[x, y].KnownValue;
			if (val == 0)
				throw new InvalidOperationException("Cannot eliminate unkown value");
			//eliminate value across its row, col
			for (int i = 0; i < 9; i++)
			{
				solveGrid[x, i].eliminate(val);
				solveGrid[i, y].eliminate(val);
			}
			//eliminate value in its square
			for (int i = x - x % 3; i < x - x % 3 + 3; i++)
			{
				for (int j = y - y % 3; j < y - y % 3 + 3; j++)
				{
					solveGrid[i, j].eliminate(val);
				}
			}
		}

		/// <summary>
		/// Some optimizations to shortcut redundant checks could have bugs that prevent further solving.
		/// Reset all of those to allow retesting to show any such  bugs.
		/// </summary>
		public void dubug_resetOptimizations()
		{
			//There aren't any "cached" values as such, but all eliminations can be reset
			// to initial state. If any are introduced they should be added here.
			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					if (solveGrid[i, j].PossibilityCount != 1)
						solveGrid[i, j].setToInitialState();
				}
			}
			setUnsolvedValuesToInitialState();
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

				//Go through and attempt each algorithm in turn

				madeIterationChanges = singlePossibilityScan();

				madeIterationChanges |= singlePlacementScan();

				madeIterationChanges |= hiddenSetScan();

				madeIterationChanges |= matchedSetEliminationScan();

				madeIterationChanges |= lineBoxEliminationScan();

				madeIterationChanges |= xwingEliminationScan();

				madeIterationChanges |= ywingEliminationScan();

				//set return value
				if (madeIterationChanges)
					madeChanges = true;
			} while (madeIterationChanges == true); //Repeat until no further progress is made

			return madeChanges;
		}

		//scan for single possibilities and set as known
		private bool singlePossibilityScan()
		{
			bool madeChanges = false;
			bool madeIterationChanges;
			do
			{
				madeIterationChanges = false;

				// This could have been done in KnownValue{Set}, but then Autosolve could not be turned off completely.
				for (int i = 0; i < 9; i++)
				{
					for (int j = 0; j < 9; j++)
					{
						//check if possibility elimination applies
						if (solveGrid[i, j].KnownValue == 0 && solveGrid[i, j].PossibilityCount == 1)
						{
							//save the remaining possibility
							setKnownValue(i, j, solveGrid[i, j].PossibilityList[0]);
							solveGrid[i, j].solveType = SolveType.PossibilityElimination;
							madeIterationChanges = true;
						}
					}
				}

				//set return value
				if (madeIterationChanges)
					madeChanges = true;
			} while (madeIterationChanges == true); //this is very cheap, keep doing it till it stops yeilding results

			return madeChanges;
		}

		//scan for instances were no other square can be a particular value and set as known
		private bool singlePlacementScan()
		{
			//NOTE: technically this algorithm is the same as finding a "hidden matched set" of size 1 and then later
			// finding the revealed single possibility. That means that if singltons were not removed from PossibilitySet,
			// this function could actually be removed. HOWEVER, treating it as a special case here and removing
			// singletons from PossibilitySet is far more efficient.

			bool madeChanges = false;
			//don't loop inside this one. it's slightly more expensive than eliminationScan and will be looped back to again if anything changes

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

		//scan for hidden sets and eliminate possibilities to reveal the matched set
		private bool hiddenSetScan()
		{
			bool madeChanges = false;

			//scan for sets in box, in col, in row TODO: add optimizations

			//check by each section type
			foreach (iterateBy iterType in Enum.GetValues(typeof(iterateBy)))
			{
				//for each section:
				for (int s = 0; s < 9; s++)
				{
					//for each element in section
					for (int e = 0; e < 9; e++)
					{
						HashSet<HashSet<int>> setList = iterCoords(iterType, s, e).SetList;

						foreach (HashSet<int> set in setList)
						{
							//find a match for this set
							int matchCount = 0;
							//scan rest of box for match TODO: check only sqaures later in scan order
							for (int e2 = 0; e2 < 9; e2++)
							{
								//if (iterCoords(iterType, s, e2).SetList.Contains(set, new gridSquare.SetOfIntEqualityComparer()))
								foreach (int i in set)
								{
									if (iterCoords(iterType, s, e2).isPossible(i))
									{
										matchCount++;
										break;
									}
								}
								if (matchCount > set.Count) //test condition already failed, stop searching
									break;
							}

							if (matchCount == set.Count)
							{
								//values in set are only valid values for matching sqaures
								//so eliminate other values in matching sqaures
								for (int eE = 0; eE < 9; eE++)
								{
									if (iterCoords(iterType, s, eE).SetList.Contains(set))
									{
										for (int i = 1; i <= 9; i++)
										//foreach (int i in set)
										{
											if (!set.Contains(i))
												madeChanges |= iterCoords(iterType, s, eE).eliminate(i);
										}
									}
								}
								break; //no other sets apply, move on
							}
						}
					}
				}
			}

			return madeChanges;
		}

		//scan for and use aligned matched sets to eliminate other possibilities
		private bool matchedSetEliminationScan()
		{
			bool madeChanges = false;

			//scan for sets in box, in col, in row

			//check by each section type
			foreach (iterateBy iterType in Enum.GetValues(typeof(iterateBy)))
			{
				//for each section:
				for (int s = 0; s < 9; s++)
				{
					if (unsolvedValues[(int)iterType, s].Count == 0)
					{
						continue; //section already completely solved
					}
					PossibilitySet sectionPossibilities = new PossibilitySet(unsolvedValues[(int)iterType, s], 8);
					foreach (HashSet<int> set in sectionPossibilities)
					{
						HashSet<int> matchIndices = new HashSet<int>();
						//for each element in section
						for (int e = 0; e < 9; e++)
						{
							bool match = true;
							for (int i = 1; i <= 9; i++)
							{
								if (!set.Contains(i) && iterCoords(iterType, s, e).isPossible(i))
								{
									match = false;
									break;
								}
							}
							if (match)
							{
								matchIndices.Add(e);
							}
						}
						if (matchIndices.Count == set.Count)
						{
							bool tempMadeChanges = false;
							//values in set can ONLY be in squares that contain only values in set
							//so eliminate set values all other places in section
							for (int eE = 0; eE < 9; eE++)
							{
								if (matchIndices.Contains(eE))
									continue;

								foreach (int i in set)
									tempMadeChanges |= iterCoords(iterType, s, eE).eliminate(i);
							}
							if (tempMadeChanges)
								madeChanges = true;
						}
					}
				}
			}
			return madeChanges;
		}

		//find eliminations based on the intersection of a line (row or col) and a box
		private bool lineBoxEliminationScan()
		{
			//most eliminations that this finds are already found by matchedSetEliminationScan, but not all.
			bool madeChanges = false;

			//if all instances of a digit in the current box are in a single row/col
			// then that digit must be in that row/col in this box and can't be in that row/col in any other box

			//iterate over boxes
			for (int bX = 0; bX < 3; bX++)
			{
				for (int bY = 0; bY < 3; bY++) //check each of  the 9 boxes
				{

					//iterate over values
					for (int val = 1; val <= 9; val++) //find which digits to line/box detect for
					{
						//TODO:optimize by skipping check for solved values by seperatly storing solved values for box
						//if val is known for any value in square, continue to next value

						int[] countInCol = new int[] { 0, 0, 0 };
						int[] countInRow = new int[] { 0, 0, 0 };

						//iterate across box, adding count of value to Row and Col as found
						for (int i = 0; i < 3; i++)
						{
							for (int j = 0; j < 3; j++)
							{
								int x = bX * 3 + i;
								int y = bY * 3 + j;

								if (solveGrid[x, y].isPossible(val))
								{
									countInCol[i]++;
									countInRow[j]++;
								}

							}
						}

						//find how many rows/cols contain value
						int colsWithVal = 0;
						int rowsWithVal = 0;
						int saveCol = -1;
						int saveRow = -1;
						for (int n = 0; n < 3; n++)
						{
							if (countInCol[n] > 0)
							{
								colsWithVal++;
								saveCol = n;
							}
							if (countInRow[n] > 0)
							{
								rowsWithVal++;
								saveRow = n;
							}
						}

						//test for single column
						if (colsWithVal == 1)
						{
							//saveCol is set to only row with value
							//eliminate all possibles for value in col outside of box
							int x = bX * 3 + saveCol;
							for (int y = 0; y < 9; y++)
							{
								if (y >= bY * 3 && y <= bY * 3 + 2)
									continue;
								madeChanges |= solveGrid[x, y].eliminate(val);
							}
						}
						//test for single row
						if (rowsWithVal == 1)
						{
							//saveRow is set to only row with value
							//eliminate all possibles for value in row outside of box
							int y = bY * 3 + saveRow;
							for (int x = 0; x < 9; x++)
							{
								if (x >= bX * 3 && x <= bX * 3 + 2)
									continue;
								madeChanges |= solveGrid[x, y].eliminate(val);
							}
						}
					}


				}

			}

			return madeChanges;
		}

		//scan for and use standard X-Wing elimination method
		private bool xwingEliminationScan()
		{
			bool madeChanges = false;

			//test each value for matches
			for (int val = 1; val <= 9; val++)
			{
				//scan for a column that contains exactly 2 squares where val is possible
				//TODO: track this information through member variables instead of recalculating
				for (int x1 = 0; x1 < 8; x1++)
				{
					int col1 = -1;
					int col2 = -1;
					int row1 = -1;
					int row2 = -1;
					//scan this column for the two vals
					int count = 0;
					for (int y = 0; y < 9; y++)
					{
						if (solveGrid[x1, y].isPossible(val))
						{
							count++;
							if (count == 1)
								row1 = y;
							else if (count == 2)
								row2 = y;
							else
								break;
						}
					}

					if (count == 2)
					{
						col1 = x1;
						// result found. search for matching column, starting on the next column
						for (int x2 = col1 + 1; x2 < 9; x2++)
						{
							//scan this column for the two values, in the correct places
							count = 0;
							for (int y = 0; y < 9; y++)
							{
								if (solveGrid[x2, y].isPossible(val))
								{
									if (y == row1 || y == row2)
									{
										count++;
									}
									else
									{
										count = -1; //this column has a match out of place. force count to error state
										break;
									}
								}
							}

							if (count == 2)
							{
								col2 = x2;
								break;
							}
						}
						//no matching columns found or only one place in column matched. continue to next val
						if (col1 == -1 || col2 == -1)
							continue;

						//found exactly 2 columns with 2 squares with val. eliminate val from all other rows
						for (int x = 0; x < 9; x++)
						{
							if (x == col1 || x == col2)
								continue;
							madeChanges |= solveGrid[x, row1].eliminate(val);
							madeChanges |= solveGrid[x, row2].eliminate(val);
						}
						//let x1 continue to loop, other x's for the same val might exist
					}
				}

				//scan for a row that contains exactly 2 squares where val is possible
				for (int y1 = 0; y1 < 8; y1++)
				{
					int row1 = -1;
					int row2 = -1;
					int col1 = -1;
					int col2 = -1;
					//scan this row for the two vals
					int count = 0;
					for (int x = 0; x < 9; x++)
					{
						if (solveGrid[x, y1].isPossible(val))
						{
							count++;
							if (count == 1)
								col1 = x;
							else if (count == 2)
								col2 = x;
							else
								break;
						}
					}

					if (count == 2)
					{
						row1 = y1;
						// result found. search for matching row, starting on the next row
						for (int y2 = row1 + 1; y2 < 9; y2++)
						{
							//scan this row for the two values, in the correct places
							count = 0;
							for (int x = 0; x < 9; x++)
							{
								if (solveGrid[x, y2].isPossible(val))
								{
									if (x == col1 || x == col2)
									{
										count++;
									}
									else
									{
										count = -1; //this row has a match out of place. force count to error state
										break;
									}
								}
							}

							if (count == 2)
							{
								row2 = y2;
								break;
							}
						}
						//no matching rows found or only one row matched. continue to next val
						if (row1 == -1 || row2 == -1)
							continue;

						//found exactly 2 rows with 2 squares with val. eliminate val from all other rows
						for (int y = 0; y < 9; y++)
						{
							if (y == row1 || y == row2)
								continue;
							madeChanges |= solveGrid[col1, y].eliminate(val);
							madeChanges |= solveGrid[col2, y].eliminate(val);
						}
						//let x1 continue to loop, other x's for the same val might exist
					}
				}


			}

			return madeChanges;
		}

		//scan for and use standard Y-Wing elimination method
		private bool ywingEliminationScan()
		{
			bool madeChanges = false;
			//don't loop inside this one; it's too expensive

			//fill out a list with all gridsquares that contain only a pair of possibilities (this will be faster than scanning entire grid for each pair combo.)
			List<gridSquare> pairs = new List<gridSquare>();
			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					if (solveGrid[i, j].PossibilityCount == 2)
						pairs.Add(solveGrid[i, j]);
				}
			}

			//scan through for any ABC alignment of pairs
			for (int i = 0; i < pairs.Count; i++)
			{
				gridSquare AB = pairs[i];
				gridSquare AC = null;
				gridSquare BC = null;
				int A = AB.PossibilityList[0];
				int B = AB.PossibilityList[1];
				int C = 0;
				//test for ABC triple of pairs (AB, AC, BC)
				//find a matching XZ
				for (int j = 0; j < pairs.Count; j++)
				{
					if (pairs[j].isPossible(B)) //don't allow to match same pair
						continue;
					if (!pairs[j].isBuddy(AB))
						continue;
					if (pairs[j].isPossible(A))
					{
						AC = pairs[j];
						//extract C value
						if (AC.PossibilityList[0] != A)
							C = AC.PossibilityList[0];
						else
							C = AC.PossibilityList[1];

						//find a matching BC
						for (int k = 0; k < pairs.Count; k++)
						{
							if (k == i)
								continue;
							if (!pairs[k].isBuddy(AB))
								continue;
							if (pairs[k].isPossible(B) && pairs[k].isPossible(C))
							{
								BC = pairs[k];
								//eliminate other possibilites
								foreach (gridSquare g in solveGrid)
								{
									if (g.isBuddy(BC) && g.isBuddy(AC) && g != BC && g != AC)
										madeChanges |= g.eliminate(C);
								}
								if (madeChanges)
									return true;
							}
						}
					}
				}
			}


			return madeChanges;
		}
	}
}
