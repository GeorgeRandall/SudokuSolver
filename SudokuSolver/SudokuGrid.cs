﻿using System;
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
			PossibilityElimination //blue
		}

		//@TODO: make private when done pulling solve logic from Form1
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

					for (int i = 1; i < 10; i++)
						eliminate(i);
				}
			}

			public bool Recheck
			{
				get { return recheck; }
			}

			public void needsRecheck()
			{ recheck = true; }

			public void recheckDone()
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
						return true;
					}

				}
				return false;
			}

			public int PossibilityCount
			{
				get { return possCount; }
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
			//@TODO copy constructor
		}

		private gridSquare[,] solveGrid;

		public int this[int x, int y]
		{
			get { return solveGrid[x, y].KnownValue; }
			set
			{
				//validate input, but don't solve
				if (solveGrid[x, y].KnownValue == value)
					return; //nothing to do
				if (solveGrid[x, y].isPossible(value))
				{
					solveGrid[x, y].KnownValue = value;
					solveGrid[x, y].needsRecheck();
					solveGrid[x, y].solveType = SolveType.Entered;
				}
			}
		}

		/// <summary>
		/// Solve as much of the puzzle as possible from current information
		/// </summary>
		/// <returns> true only if more of the puzzle was solved</returns>
		public bool solve()
		{
			//@TODO:Write solve function
			return false;
		}


	}
}
