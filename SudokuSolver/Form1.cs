using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

//@todo - Refactor all the grid logic out into another class. This class should only contain input and display code.

namespace SudokuSolver
{
	public partial class Form1 : Form
	{
		//just a little help in knowing a textbox's x,y when it raises an event
		public class TextBoxXY : System.Windows.Forms.TextBox
		{
			public bool handleTextChange;
			int x;
			int y;
			public TextBoxXY(int x, int y)
			{
				handleTextChange = false;
				this.x = x;
				this.y = y;
			}

			public int X
			{
				get { return x; }
				set { x = value; }
			}

			public int Y
			{
				get { return y; }
				set { y = value; }
			}
		}

		//Results grid.
		private SudokuGrid.gridSquare[,] mainGrid;
		private Stack<SudokuGrid.gridSquare[,]> snapshotStack;

		//private int[,] snapshotResults;
		//private bool[, ,] snapshotPos;
		private TextBoxXY[,] textBoxXY;

		public Form1()
		{
			InitializeComponent();
			InitializeComponentDynamic();
			mainGrid = new SudokuGrid.gridSquare[9, 9];

			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					mainGrid[i, j] = new SudokuGrid.gridSquare(i, j);
				}
			}
			snapshotStack = new Stack<SudokuGrid.gridSquare[,]>();
			//snapshotResults = new int[9, 9];
			//snapshotPos = new bool[9, 9, 10];
			resetGrid();

			numericUpDown1.Maximum = testCases.Length - 1;
		}

		void resetGrid()
		{
			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					mainGrid[i, j].reset();
					textBoxXY[i, j].Text = "";
					textBoxXY[i, j].BackColor = System.Drawing.Color.White;
					textBoxXY[i, j].ForeColor = System.Drawing.Color.Black;
					displayPossibilities(i, j);
				}
			}
		}

		private const float fontSize = 18f;
		void InitializeComponentDynamic()
		{
			//dynamically initialize the components for the grid
			//creating the classic sudoku style by spacing the textboxes (with flat style outline) out over a black panel
			this.SuspendLayout();

			const int smallMargin = 0;
			const int extraMargin = 2;

			//use the invisble dummy text box to get the correct text box height for the font size
			textBoxDummy.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			textBoxDummy.Size = new System.Drawing.Size(1, 1); //setting it to 1x1 pixel, the textbox will clamp that value to smallest allowed for font size
			int textBoxSize = textBoxDummy.Size.Height; //get the clamped value to use in our real text boxes

			//create the text boxes 
			textBoxXY = new TextBoxXY[9, 9];
			for (int y = 0; y < 9; y++)
			{
				for (int x = 0; x < 9; x++)
				{
					//int boxIX = x % 3; //x index within group box it will be in (only matters for layout and assignments)
					//int boxIY = y % 3; //y " " "
					int boxX = smallMargin * (x + 1) + extraMargin * (1 + x / 3) + textBoxSize * x; //x coordinate within group box it will be in
					int boxY = smallMargin * (y + 1) + extraMargin * (1 + y / 3) + textBoxSize * y;  //y " " "

					textBoxXY[x, y] = new TextBoxXY(x, y);
					textBoxXY[x, y].Location = new System.Drawing.Point(boxX, boxY);
					textBoxXY[x, y].MaxLength = 1;
					textBoxXY[x, y].Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
					textBoxXY[x, y].Name = "textBox" + x.ToString() + y.ToString();
					textBoxXY[x, y].Multiline = true; //allow multiline for Clutter (doesn't affect normal single character usage) (changes size when set for some odd reason?)
					textBoxXY[x, y].Size = new System.Drawing.Size(textBoxSize, textBoxSize);
					textBoxXY[x, y].TabIndex = y * 9 + x;
					textBoxXY[x, y].Text = ""; //textBoxXY[x, y].TabIndex.ToString();
					textBoxXY[x, y].TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
					//if(x < 5)
					textBoxXY[x, y].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

					textBoxXY[x, y].TextChanged += new System.EventHandler(this.textBoxXY_TextChanged);
					textBoxXY[x, y].Enter += new System.EventHandler(this.textBoxXY_Enter);
					textBoxXY[x, y].Leave += new System.EventHandler(this.textBoxXY_Leave);
					textBoxXY[x, y].KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxXY_KeyDown);
					panel1.Controls.Add(textBoxXY[x, y]);
				}
			}

			int panelSize = 10 * smallMargin + 4 * extraMargin + 9 * textBoxSize;
			panel1.Size = new Size(panelSize, panelSize);
			this.ResumeLayout(true);

			panelDebugTools.Location = new Point(panelDebugTools.Location.X, panel1.Location.Y + panel1.Size.Height + panel1.Margin.Size.Height);
			
			ResetSize();
		}

		private void ResetSize()
		{
			if(!panelDebugTools.Visible)
				this.Size = new Size(panel1.Location.X * 2 + panel1.Size.Width + panel1.Margin.Size.Width, panel1.Location.Y + panel1.Size.Height + 64);
			else
				this.Size = new Size(panel1.Location.X * 2 + panel1.Size.Width + panel1.Margin.Size.Width, panelDebugTools.Location.Y + panelDebugTools.Size.Height + 64);
		}


		private void textBoxXY_TextChanged(object sender, EventArgs e)
		{
			TextBoxXY curr = (TextBoxXY)sender;

			if (!curr.handleTextChange)
				return;

			String text = curr.Text;
			if (text.Length < 1) //test for text entered
			{
				if (mainGrid[curr.X, curr.Y].KnownValue == 0)//no text, no value,  nothing to do.
					return;
				else //not text, but should be a value, reset back to correct text
				{
					curr.Text = "" + mainGrid[curr.X, curr.Y].KnownValue;
					return;
				}
			}
			//make sure a valid digit was entered.
			char digit = text[0];
			if (!Char.IsDigit(digit) || digit == '0')
			{
				curr.Text = ""; //not a digit, delete it
				return;
			}
			//see if just setting it to the value it is already known to be
			if (Char.GetNumericValue(digit) == mainGrid[curr.X, curr.Y].KnownValue)
			{
				curr.Text = "" + mainGrid[curr.X, curr.Y].KnownValue; //no change, just reset the text
				return;
			}

			//reset the text color. other functions will sometimes change this
			curr.ForeColor = System.Drawing.Color.Black;

			//attempt to apply the new digit
			if (!digitChanged(curr.X, curr.Y, (int)Char.GetNumericValue(digit)))
			{
				curr.Text = ""; //change rejected, clear the text
				return;
			}
			mainGrid[curr.X, curr.Y].solveType = SudokuGrid.SolveType.Entered;

			labelDebugInfo.Text = listPossibilities(curr.X, curr.Y); //change accepted, update possibilty list for this cell
			//checks entire grid, if autosolve is on
			scanGrid();
		}

		void scanGrid()
		{
			if (!checkBoxAuto.Checked)
			{
				UpdatePossibilities();
				return;
			}
			
			//before starting scan, do a full Check_digit scan, then begin changes loop regardless of result
			//number elimination is fairly cheap to loop through, since it only calls check_digit() on changed cells
			ScanCheckDigitChanges(); //recheck all the changed digits for more number possibility eliminations

			bool foundChanges;
			do
			{
				foundChanges = false;
				//place eliminantion across the whole grid (sadly can't shortcut these, since these will only be found in response to something somewhere else changing)
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
							if (mainGrid[X, Y].isPossible(val))
							{
								sum++;
								savedY = Y; //remember the index of a postition of val in case it's the only one in the set
								if (mainGrid[X, Y].KnownValue == val) //val has already been found for this row
								{
									//stop trying to find it
									sum = -1;
									break;
								}
								if (sum > 1)
									break; //only does work if sum equals 1, give up
							}
						}
						if (sum == -1)
							continue;
						//sum == 0 has already been handled per cell, I think
						if (sum == 1)
						{
							//there is definitly a single valid spot for that value, put it there
							digitChanged(X, savedY, val);
							foundChanges = true;
							textBoxXY[X, savedY].ForeColor = System.Drawing.Color.Red;
							mainGrid[X, savedY].solveType = SudokuGrid.SolveType.PlaceElimination;
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
							if (mainGrid[X, Y].isPossible(val))
							{
								sum++;
								savedX = X; //remember the index of a postition of val in case it's the only one in the set
								if (mainGrid[X, Y].KnownValue == val) //val has already been found for this row
								{
									//stop trying to find it
									sum = -1;
									break;
								}
							}
							if (sum > 1)
								break; //only does work if sum equals 1, give up
						}
						if (sum == -1)
							continue;
						//sum == 0 has already been handled per cell, I think
						if (sum == 1)
						{
							//there is definitly a single valid spot for that value, put it there
							digitChanged(savedX, Y, val);
							foundChanges = true;
							textBoxXY[savedX, Y].ForeColor = System.Drawing.Color.Red;
							mainGrid[savedX, Y].solveType = SudokuGrid.SolveType.PlaceElimination;
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
									if (mainGrid[X, Y].isPossible(val))
									{
										sum++;
										savedX = X; //remember the index of a postition of val in case it's the only one in the set
										savedY = Y;
										if (mainGrid[X, Y].KnownValue == val) //val has already been found for this row
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
							if (sum == -1)
								continue;
							//sum == 0 has already been handled per cell, I think
							if (sum == 1)
							{
								//there is definitly a single valid spot for that value, put it there
								digitChanged(savedX, savedY, val);
								foundChanges = true;
								textBoxXY[savedX, savedY].ForeColor = System.Drawing.Color.Red;
								mainGrid[savedX, savedY].solveType = SudokuGrid.SolveType.PlaceElimination;
							}
						}
					}
				}

				//after each round 
				//number elimination is fairly cheap to loop through, since it only calls check_digit() on changed cells
				bool scanFoundChanges = ScanCheckDigitChanges(); //recheck all the changed digits for more number possibility eliminations
				
				//keeping the foundChanges true if the searches above found any changes, even if ScanCheckDigitChanges found none
				foundChanges = scanFoundChanges || foundChanges;

			} while (foundChanges); //keep checking for number eliminations and place eliminations till nothing changes

			UpdatePossibilities();
		}

		private bool ScanCheckDigitChanges()
		{
			//force recheck of everything
				for (int i = 0; i < 9; i++)
					for (int j = 0; j < 9; j++)
						mainGrid[i, j].needsRecheck();

			bool foundAnyChanges = false;
			bool foundNewChanges;
			do
			{
				foundNewChanges = false;
				for (int i = 0; i < 9; i++)
				{
					for (int j = 0; j < 9; j++)
					{
						if (mainGrid[i, j].Recheck && mainGrid[i, j].KnownValue == 0)
						{
							
							mainGrid[i, j].recheckDone();
							if (check_digit(i, j))
							{
								foundAnyChanges = true;
								foundNewChanges = true;
							}
						}
					}
				}
			} while (foundNewChanges); //do as much by number elimination as possible, before moving on to a single round of place elimination
			return foundAnyChanges;
		}

		bool digitChanged(int X, int Y, int val)
		{
			restoreFonts(textBoxXY[X, Y]);
			if (mainGrid[X, Y].KnownValue != 0)
				return false; //can't change already known values
			if (!mainGrid[X, Y].isPossible(val))
				return false; //reject already eliminated values
			//value is now known by user entry
			mainGrid[X, Y].KnownValue = val;
			textBoxXY[X, Y].Text = "" + val;

			//eliminate entered value across its row, col
			for (int i = 0; i < 9; i++)
			{
				mainGrid[X, i].eliminate(val);
				mainGrid[i, Y].eliminate(val);
			}
			//eleminate entered value in its square
			for (int i = X - X % 3; i < X - X % 3 + 3; i++)
				for (int j = Y - Y % 3; j < Y - Y % 3 + 3; j++)
				{
					mainGrid[i, j].eliminate(val);
				}

			return true;
		}//digitChanged()


		//checks for anything about a specific spot.
		//returns true if it made any changes to the grid
		bool check_digit(int X, int Y)
		{
			if (mainGrid[X, Y].KnownValue != 0)
				return false;

			if (mainGrid[X, Y].PossibilityCount == 0)
			{
				textBoxXY[X, Y].BackColor = System.Drawing.Color.Red;
				return false;
			}

			if (mainGrid[X, Y].PossibilityCount == 1)
			{
				//only one number it could be
				for (int i = 1; i < 10; i++)
				{
					if (mainGrid[X, Y].isPossible(i))
					{
						digitChanged(X, Y, i);
						break;
					}
				}

				textBoxXY[X, Y].ForeColor = System.Drawing.Color.Blue;
				mainGrid[X, Y].solveType = SudokuGrid.SolveType.PossibilityElimination;

				return true;
			}

			bool madeChanges = false;

			//if all instances of a digit in the current box are in a single row/col
			// then that digit must be in that row/col in this box and can't be in that row/col in any other box
			for (int val = 1; val <= 9; val++) //find which digits to place detect for
			{
				if (!mainGrid[X, Y].isPossible(val))
					continue;

				int outOfColCount = 0;
				int outOfRowCount = 0;
				for (int j = X - X % 3; j < X - X % 3 + 3; j++)
					for (int k = Y - Y % 3; k < Y - Y % 3 + 3; k++)
					{
						if (mainGrid[j, k].isPossible(val))
						{
							if (j != X)
								outOfColCount++;
							if (k != Y)
								outOfRowCount++;
						}

					}
				if (outOfRowCount == 0)
				{
					//remove i from all in row outside this box
					for (int j = 0; j < 9; j++)
					{
						if (j - j % 3 == X - X % 3) //if it's in the same box
							continue;
						madeChanges = madeChanges || mainGrid[j, Y].eliminate(val);
					}
				}
				if (outOfColCount == 0)
				{
					//remove i from all in col outside this box
					for (int k = 0; k < 9; k++)
					{
						if (k - k % 3 == Y - Y % 3) //if it's in the same box, don't touch it
							continue;
						madeChanges = madeChanges || mainGrid[X, k].eliminate(val);
					}
				}
			}

			//super pair set finding algorithm
			if (mainGrid[X, Y].PossibilityCount == 2)
			{

				//find the pair of possible numbers for current cell
				int n1;
				for (n1 = 1; n1 <= 9; n1++)
				{
					if (mainGrid[X, Y].isPossible(n1))
						break;
				}
				int n2;
				for (n2 = n1 + 1; n2 <= 9; n2++)
				{
					if (mainGrid[X, Y].isPossible(n2))
						break;
				}

				//found the pair n1, n2 in cell X,Y (this must succeed because there are exactly 2 possibilities in cell X,Y

				//checking for a super pair in this Y row of X,Y's box
				for (int x2 = X - X % 3; x2 < X - X % 3 + 3; x2++)
				{
					if (x2 == X)
						continue; //can't be a pair with itself
					if (mainGrid[x2, Y].PossibilityCount == 2
						&& mainGrid[x2, Y].isPossible(n1) && mainGrid[x2, Y].isPossible(n2))
					{
						//found a super pair in this row
						//clear the row
						for (int j = 0; j < 9; j++)
						{
							if (j == X || j == x2)
								continue; //don't clear the super pair we just found
							madeChanges = madeChanges || mainGrid[j, Y].eliminate(n1);
							madeChanges = madeChanges || mainGrid[j, Y].eliminate(n2);
						}
						//clear the box
						for (int j = X - X % 3; j < X - X % 3 + 3; j++)
						{
							for (int k = Y - Y % 3; k < Y - Y % 3 + 3; k++)
							{
								if (k == Y)
									continue; //skip Y's row, it's already taken care of
								madeChanges = madeChanges || mainGrid[j, k].eliminate(n1);
								madeChanges = madeChanges || mainGrid[j, k].eliminate(n2);
							}
						}
						break; // can't have more than one super pair in a row, stop looking
					}
				}
				//checking for a super pair in this X col of X,Y's box
				for (int y2 = Y - Y % 3; y2 < Y - Y % 3 + 3; y2++)
				{
					if (y2 == Y)
						continue; //can't be a pair with itself
					if (mainGrid[X, y2].PossibilityCount == 2
						&& mainGrid[X, y2].isPossible(n1) && mainGrid[X, y2].isPossible(n2))
					{
						//found a super pair in this col
						//clear the col
						for (int k = 0; k < 9; k++)
						{
							if (k == Y || k == y2)
								continue; //don't clear the super pair we just found
							madeChanges = madeChanges || mainGrid[X, k].eliminate(n1);
							madeChanges = madeChanges || mainGrid[X, k].eliminate(n2);
						}
						//clear the box
						for (int j = X - X % 3; j < X - X % 3 + 3; j++)
						{
							if (j == X)
								continue; //skip X's col, it's already taken care of
							for (int k = Y - Y % 3; k < Y - Y % 3 + 3; k++)
							{
								madeChanges = madeChanges || mainGrid[j, k].eliminate(n1);
								madeChanges = madeChanges || mainGrid[j, k].eliminate(n2);
							}
						}
						break; // can't have more than one super pair in a col, stop looking
					}
				}
			} //if super pair

			//pair set finding algorithm
			//go through all available pairs of numbers in current cell
			for (int n1 = 1; n1 <= 9; n1++)
			{
				if (!mainGrid[X, Y].isPossible(n1))
					continue;
				for (int n2 = n1 + 1; n2 <= 9; n2++)
				{
					if (n1 == n2 || !mainGrid[X, Y].isPossible(n2))
						continue;
					//have a pair n1, n2 found in cell X,Y
					//TODO: also check for triples??...
					//now scan box for the pair set
					int inCol = 0; //pairs found in col X
					int outCol = 0; //pairs found outside col X
					int inRow = 0;//pairs found in row Y
					int outRow = 0;//pairs found outside row Y
					for (int j = X - X % 3; j < X - X % 3 + 3; j++)
					{
						for (int k = Y - Y % 3; k < Y - Y % 3 + 3; k++)
						{
							//if the number for this cell is already known, it doesn't affect this calc at all, skip it.
							if (mainGrid[j, k].KnownValue != 0)
								continue;
							//see how many times BOTH are found in the row/col
							if (mainGrid[j, k].isPossible(n1) && mainGrid[j, k].isPossible(n2))
							{
								if (j == X)
									inCol++;
								if (k == Y)
									inRow++;
							}
							//see how many times Either is found outside of the row/col
							if (mainGrid[j, k].isPossible(n1) || mainGrid[j, k].isPossible(n2))
							{
								if (j != X)
									outCol++;
								if (k != Y)
									outRow++;
							}
							//check for any NONPAIRS found inside of the row/col, count them as marks against...
							if (mainGrid[j, k].isPossible(n1) ^ mainGrid[j, k].isPossible(n2))
							{
								if (j == X)
									outCol++;
								if (k == Y)
									outRow++;
							}
						}
					}
					if (inCol == 2 && outCol == 0)
					{
						//found a pair set in column
						//no other cell in this col can be either of the numbers in the pair set
						for (int i = 0; i < 9; i++)
						{
							//if the number for this cell is already known, it doesn't need changing at all, skip it.
							if (mainGrid[X, i].KnownValue != 0)
								continue;
							if (i - i % 3 != Y - Y % 3) //don't clear in the current box
							{
								madeChanges = madeChanges || mainGrid[X, i].eliminate(n1);
								madeChanges = madeChanges || mainGrid[X, i].eliminate(n2);
							}
							else if (mainGrid[X, i].isPossible(n1) && mainGrid[X, i].isPossible(n2))
							{
								//in the current box, and looking at one of the two pair cells, clear out the pair's other possibilities
								for (int cval = 1; cval <= 9; cval++)
									if (cval != n1 && cval != n2)
									{
										madeChanges = madeChanges || mainGrid[X, i].eliminate(cval);
									}
							}
						}
					}
					if (inRow == 2 && outRow == 0)
					{
						//found a pair set in row
						//no other cell in this row can be either of the numbers in the pair set
						for (int i = 0; i < 9; i++)
						{
							//if the number for this cell is already known, it doesn't need changing at all, skip it.
							if (mainGrid[i, Y].KnownValue != 0)
								continue;
							if (i - i % 3 != X - X % 3) //don't clear in the current box
							{
								madeChanges = madeChanges || mainGrid[i, Y].eliminate(n1);
								madeChanges = madeChanges || mainGrid[i, Y].eliminate(n2);
							}
							else if (mainGrid[i, Y].isPossible(n1) && mainGrid[i, Y].isPossible(n2))
							{
								//in the current box, and looking at one of the two pair cells, clear out the pair's other possibilities
								for (int cval = 1; cval <= 9; cval++)
									if (cval != n1 && cval != n2)
									{
										madeChanges = madeChanges || mainGrid[i, Y].eliminate(cval);
									}
							}
						}
					}

				}

			}

			return madeChanges;

		}//void check_digit(X,Y)


		//display possibility info in labelMessage whenever textbox is entered
		private void textBoxXY_Enter(object sender, EventArgs e)
		{
			TextBoxXY tb = (TextBoxXY)sender;
			labelDebugInfo.Text = listPossibilities(tb.X, tb.Y);
			tb.handleTextChange = true;
			restoreFonts(tb);
		}
		private void restoreFonts(TextBoxXY tb, bool force = false)
		{
			if (mainGrid[tb.X, tb.Y].KnownValue == 0 || force)
			{
				Size size = tb.Size;
				tb.Text = "";
				tb.MaxLength = 1;
				tb.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				tb.Size = size;
			}
		}

		private void textBoxXY_Leave(object sender, EventArgs e)
		{
			TextBoxXY tb = (TextBoxXY)sender;
			tb.handleTextChange = false;
			displayPossibilities(tb.X, tb.Y);
		}

		//make arrow keys move between text boxes for easier entry
		private void textBoxXY_KeyDown(object sender, KeyEventArgs e)
		{
			TextBoxXY tb = (TextBoxXY)sender;
			//don't let text box handle arrow key presses
			if (e.KeyValue == 37 || e.KeyValue == 38 || e.KeyValue == 39 || e.KeyValue == 40)
				e.SuppressKeyPress = true;
			else
				return;

			//handle left arrow
			if (e.KeyValue == 37 && tb.X > 0)
				textBoxXY[tb.X - 1, tb.Y].Focus();
			//right arrow
			if (e.KeyValue == 39 && tb.X < 8)
				textBoxXY[tb.X + 1, tb.Y].Focus();
			//up arrow
			if (e.KeyValue == 38 && tb.Y > 0)
				textBoxXY[tb.X, tb.Y - 1].Focus();
			//down arrow
			if (e.KeyValue == 40 && tb.Y < 8)
				textBoxXY[tb.X, tb.Y + 1].Focus();

		}

		//lists the possibilites for this location in the message box
		string listPossibilities(int X, int Y)
		{
			String ret = "";
			if (checkBoxDebugTools.Checked)
				ret = "(" + X + "," + Y + ")\t";
			for (int i = 1; i < 10; i++)
				if (mainGrid[X, Y].isPossible(i))
					ret += " " + i;
			return ret;
		}

		private void buttonReset_Click(object sender, EventArgs e)
		{
			resetGrid();
			labelDebugInfo.Text = "";
		}

		private void buttonSnapShot_Click(object sender, EventArgs e)
		{
			SudokuGrid.gridSquare[,] snapshotGrid;
			snapshotGrid = new SudokuGrid.gridSquare[9, 9];
			//clone current grid into snapshot grid
			for (int i = 0; i < 9; i++)
				for (int j = 0; j < 9; j++)
				{
					snapshotGrid[i, j] = new SudokuGrid.gridSquare(mainGrid[i, j]);
				}
			snapshotStack.Push(snapshotGrid);

			buttonRestore.Enabled = true;
			buttonPopSnapshot.Enabled = true;
			buttonPopSnapshot.Text = "Pop Snapshot (" + snapshotStack.Count + ")";
		}

		private void buttonRestore_Click(object sender, EventArgs e)
		{
			RestoreSnapshot(snapshotStack.Peek());
		}

		private void RestoreSnapshot(SudokuGrid.gridSquare[,] snapshotGrid)
		{
			for (int i = 0; i < 9; i++)
				for (int j = 0; j < 9; j++)
				{
					mainGrid[i, j].copy(snapshotGrid[i, j]);

					//Restore correct background color
					if (mainGrid[i,j].PossibilityCount == 0)
						textBoxXY[i, j].BackColor = System.Drawing.Color.Red;
					else
						textBoxXY[i, j].BackColor = System.Drawing.Color.White;
					//Restore text
					if (mainGrid[i, j].KnownValue != 0)
					{
						restoreFonts(textBoxXY[i, j], true);
						textBoxXY[i, j].Text = mainGrid[i, j].KnownValue.ToString();
					}
					else
						displayPossibilities(i, j);
					//Restore text color
					switch (mainGrid[i, j].solveType)
					{
						case SudokuGrid.SolveType.Unsolved:
						case SudokuGrid.SolveType.Entered:
							textBoxXY[i, j].ForeColor = System.Drawing.Color.Black;
							break;
						case SudokuGrid.SolveType.PlaceElimination:
							textBoxXY[i, j].ForeColor = System.Drawing.Color.Red;
							break;
						case SudokuGrid.SolveType.PossibilityElimination:
							textBoxXY[i, j].ForeColor = System.Drawing.Color.Blue;
							break;
					}
				}
			labelDebugInfo.Text = "";
		}

		private void buttonPopSnapshot_Click(object sender, EventArgs e)
		{
			RestoreSnapshot(snapshotStack.Pop());
			if (snapshotStack.Count <= 0)
			{
				buttonRestore.Enabled = false;
				buttonPopSnapshot.Enabled = false;
			}
			buttonPopSnapshot.Text = "Pop Snapshot (" + snapshotStack.Count + ")";
		}


		private void checkBoxAuto_CheckedChanged(object sender, EventArgs e)
		{
			scanGrid();
		}

		private void buttonRecheck_Click(object sender, EventArgs e)
		{
			//force a recheck of the entire grid
			//NOTE: THIS FUNCTION SHOULD HAVE NO EFFECT when scanGrid is working correctly!
			for (int i = 0; i < 9; i++)
				for (int j = 0; j < 9; j++)
					mainGrid[i, j].needsRecheck();
			scanGrid();
		}


		private void displayPossibilities(int x, int y)
		{
			if (mainGrid[x, y].KnownValue != 0
				|| !checkBoxClutter.Checked)
			{
				restoreFonts( textBoxXY[x, y]);
				return;
			}
			Size size = textBoxXY[x, y].Size;
			textBoxXY[x, y].Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize / 3, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			textBoxXY[x, y].MaxLength = 100;
			textBoxXY[x, y].Text = listPossibilities(x, y);
			textBoxXY[x, y].Size = size;
		}

		private void checkBoxClutter_CheckedChanged(object sender, EventArgs e)
		{
			UpdatePossibilities();
		}

		private void UpdatePossibilities()
		{
			for (int i = 0; i < 9; i++)
				for (int j = 0; j < 9; j++)
				{
					displayPossibilities(i, j);
				}
		}

		private void checkBoxDebugTools_CheckedChanged(object sender, EventArgs e)
		{
			panelDebugTools.Visible = checkBoxDebugTools.Checked;
			ResetSize();
			UpdatePossibilities();
		}

		struct testCase
		{
			public int[] X;
			public int[] Y;
			public int[] n;
		}

		testCase[] testCases = new testCase[]
		{
			//0. simple test case for elimination of possiblities for a cell (blue numbers)
			// and elimination of places for a number in a box (red numbers)
			new testCase
			{
				X = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 1, 6, 7, 4},
				Y = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 6, 7, 7, 8},
				n = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 1, 9, 7, 1}
			},
			//1. simple test case of a super pair elimination
			new testCase
			{

				X = new int[] { 0, 0, 0, 2, 2, 3, 4, 6, 7, },
				Y = new int[] { 0, 1, 2, 0, 1, 0, 0, 1, 1, },
				n = new int[] { 1, 2, 3, 4, 5, 6, 7, 6, 7, },
			},
			//2. full puzzle I'm still trying to solve. was used to discover super pair technique
			new testCase
			{
				X = new int[] { 4, 8, 0, 3, 5, 7, 0, 2, 5, 6, 1, 5, 0, 4, 8, 3, 7, 2, 3, 6, 8, 1, 3, 5, 8, 0, 4},
				Y = new int[] { 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 4, 4, 4, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8},
				n = new int[] { 8, 1, 6, 2, 1, 9, 2, 9, 3, 8, 4, 9, 3, 4, 8, 6, 7, 8, 5, 6, 3, 3, 1, 8, 5, 4, 6}
			},
			//3. should be able to compute that (2,1) = 2 because of 4's using "X-wing technique"
			new testCase
			{
				X = new int[] { 0, 0, 0, 1, 2, 2, 2, 2, 3, 4, 4, 5, 5, 6, 7, 8, 8, 8, },
				Y = new int[] { 2, 5, 6, 5, 0, 4, 5, 7, 7, 1, 3, 0, 8, 4, 1, 2, 5, 6, },
				n = new int[] { 3, 5, 2, 4, 5, 8, 1, 7, 4, 6, 4, 4, 3, 4, 9, 7, 9, 3, },
			},
			//4. full puzzle I'm still trying to solve. has not yet yealded any new techniques
			new testCase
			{
				X = new int[] { 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 3, 3, 4, 4, 4, 4, 5, 5, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, },
				Y = new int[] { 2, 4, 6, 0, 2, 4, 5, 0, 2, 3, 5, 8, 0, 1, 7, 8, 0, 3, 5, 6, 8, 3, 4, 6, 8, 2, 4, 6, },
				n = new int[] { 3, 7, 2, 5, 7, 3, 9, 9, 4, 6, 4, 6, 6, 4, 8, 3, 1, 5, 2, 6, 9, 1, 5, 8, 4, 8, 4, 3, },
			},
			//5. new full puzzle. unsolved.
			new testCase
			{
				X = new int[] { 0, 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 4, 4, 4, 4, 5, 5, 6, 6, 6, 7, 7, 7, 8, 8, 8, 8, },
				Y = new int[] { 0, 2, 3, 8, 2, 3, 5, 0, 5, 8, 2, 3, 1, 3, 5, 7, 5, 6, 0, 3, 8, 3, 5, 6, 0, 5, 6, 8, },
				n = new int[] { 7, 4, 8, 2, 2, 4, 9, 6, 7, 4, 1, 7, 8, 6, 2, 5, 1, 7, 9, 2, 6, 3, 8, 5, 5, 6, 8, 7, },
			},
			//6. new full puzzle. unsolved.
			new testCase
			{
				X = new int[] { 0, 0, 0, 0, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5, 6, 6, 6, 7, 8, 8, 8, 8, },
				Y = new int[] { 2, 4, 5, 8, 7, 2, 3, 8, 1, 3, 4, 0, 2, 4, 6, 8, 4, 5, 7, 0, 5, 6, 1, 0, 3, 4, 6, },
				n = new int[] { 1, 5, 7, 6, 5, 4, 9, 8, 4, 5, 9, 3, 9, 1, 2, 4, 3, 4, 7, 4, 9, 1, 7, 8, 6, 4, 5, },
			},
			//7. new full puzzle. unsolved.
			new testCase
			{
				X = new int[] { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 5, 5, 5, 5, 6, 6, 7, 7, 8, 8, 8, 8, },
				Y = new int[] { 1, 2, 4, 8, 3, 7, 2, 6, 1, 5, 6, 7, 0, 4, 8, 1, 2, 3, 7, 2, 6, 1, 5, 0, 4, 6, 7, },
				n = new int[] { 6, 2, 3, 4, 4, 3, 9, 8, 2, 6, 5, 1, 8, 4, 6, 1, 3, 9, 8, 8, 6, 9, 7, 1, 8, 3, 5, },
			},
		};

		private void buttonTestCase_Click(object sender, EventArgs e)
		{
			testCase test = testCases[(int)numericUpDown1.Value];

			TimeSpan sum = TimeSpan.Zero;

			//for (int i = 0; i < 100; i++) //uncomment to run test 100 times for better time comparisons
			{
				resetGrid();
				labelDebugInfo.Text = "";

				System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

				timer.Start();
				for (int j = 0; j < test.X.Length; j++)
				{
					digitChanged(test.X[j], test.Y[j], test.n[j]);
				}

				scanGrid();
				
				timer.Stop();

				sum += timer.Elapsed;
			}
			labelDebugInfo.Text = "Elapsed:" + sum.ToString();
		}

		private void buttonExtract_Click(object sender, EventArgs e)
		{
			//in the form:
			//X = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 1, 6, 7, 4},
			//Y = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 6, 7, 7, 8},
			//n = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 1, 9, 7, 1}

			string X = "X = new int[] { ";
			string Y = "Y = new int[] { ";
			string n = "n = new int[] { ";

			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					if (mainGrid[i, j].solveType == SudokuGrid.SolveType.Entered)
					{
						X += i.ToString() + ", ";
						Y += j.ToString() + ", ";
						n += mainGrid[i, j].KnownValue.ToString() + ", ";
					}
				}
			}

			X += "},\r\n";
			Y += "},\r\n";
			n += "},\r\n";

			Clipboard.SetText(X + Y + n);
			MessageBox.Show(X + Y + n + "\r\nData copied to clipboard");
		}






	}//class Form1
}//namespace
