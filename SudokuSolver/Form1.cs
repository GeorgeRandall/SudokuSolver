using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;


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

		private SudokuGrid currentGrid; //private SudokuGrid.gridSquare[,] mainGrid;
		private Stack<SudokuGrid> snapshotStack; //private Stack<SudokuGrid.gridSquare[,]> snapshotStack;

		private TextBoxXY[,] textBoxXY;

		public Form1()
		{
			InitializeComponent();
			InitializeComponentDynamic();
			
			currentGrid = new SudokuGrid();
			snapshotStack = new Stack<SudokuGrid>();
			refreshDisplay();

			numericUpDown1.Maximum = testCases.Length - 1;
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
			panelDebugTools.Location = new Point(panelDebugTools.Location.X, panel1.Location.Y + panel1.Size.Height + panel1.Margin.Size.Height);
			panelDebugTools.Visible = checkBoxDebugTools.Checked; 
			
			this.ResumeLayout(true);
			
			ResetSize();
		}

		private void ResetSize()
		{
			if (!checkBoxDebugTools.Checked)
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
			if (text.Length < 1) //test for text entered //TODO: redo this section with refreshDisplay
			{
				if (currentGrid[curr.X, curr.Y] == 0)//no text, no value,  nothing to do.
					return;
				else //not text, but should be a known value
				{
					curr.Text = "" + currentGrid[curr.X, curr.Y]; //just reset text. No need to refreshDisplay
					return;
				}
			}
			//make sure a valid digit was entered.
			char digit = text[0];
			if (!Char.IsDigit(digit) || digit == '0')
			{
				curr.Text = ""; //not a digit, delete text. No need to refreshDisplay
				return;
			}
			//see if just setting it to the value it is already known to be
			if (Char.GetNumericValue(digit) == currentGrid[curr.X, curr.Y])
			{
				curr.Text = "" + currentGrid[curr.X, curr.Y]; //no change, just reset the text. No need to refreshDisplay
				return;
			}

			//attempt to apply the new digit
			if (!currentGrid.setKnownValue(curr.X, curr.Y, (int)Char.GetNumericValue(digit)))
			{
				curr.Text = ""; //change rejected, clear the text. No need to refreshDisplay
				return;
			}

			labelDebugInfo.Text = listPossibilities(curr.X, curr.Y); //force update possibilty list for this cell, since this cell has focus
			//change accepted, check entire grid, if autosolve is on
			if (checkBoxAuto.Checked)
				currentGrid.solve();
				
			refreshDisplay();
			
		}

		//Resetting everything is fast enough that it is not worth the maintanance risk to update the display piecemeal
		private void refreshDisplay()
		{
			for (int i = 0; i < 9; i++)
				for (int j = 0; j < 9; j++)
				{
					//reset correct background color
					if (currentGrid.solveType(i,j) == SudokuGrid.SolveType.Invalid)
						textBoxXY[i, j].BackColor = System.Drawing.Color.Red;
					else
						textBoxXY[i, j].BackColor = System.Drawing.Color.White;
					//reset text
					if (currentGrid[i, j] != 0)
					{
						restoreFonts(textBoxXY[i, j], true);
						textBoxXY[i, j].Text = currentGrid[i, j].ToString();
					}
					else
						displayPossibilities(i, j);
					//reset text color
					switch (currentGrid.solveType(i, j))
					{
						case SudokuGrid.SolveType.Unsolved:
						case SudokuGrid.SolveType.Entered:
							textBoxXY[i, j].ForeColor = System.Drawing.Color.Black;
							break;
						case SudokuGrid.SolveType.PlaceElimination:
							textBoxXY[i, j].ForeColor = System.Drawing.Color.Red;//TODO: red means error, pick new color
							break;
						case SudokuGrid.SolveType.PossibilityElimination:
							textBoxXY[i, j].ForeColor = System.Drawing.Color.Blue;
							break;
					}
				}
		}

		//display possibility info in labelMessage whenever textbox is entered
		private void textBoxXY_Enter(object sender, EventArgs e)
		{
			TextBoxXY tb = (TextBoxXY)sender;
			labelDebugInfo.Text = listPossibilities(tb.X, tb.Y);
			tb.handleTextChange = true;
			restoreFonts(tb);
		}

		//restore Font to normal size for number display/entry
		private void restoreFonts(TextBoxXY tb, bool force = false)
		{
			if (currentGrid[tb.X, tb.Y] == 0 || force)
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
			labelDebugInfo.Text = "";
		}

		//make arrow keys move between text boxes for easier entry
		private void textBoxXY_KeyDown(object sender, KeyEventArgs e)
		{
			TextBoxXY tb = (TextBoxXY)sender;
			if (e.KeyValue == 37 || e.KeyValue == 38 || e.KeyValue == 39 || e.KeyValue == 40)
			{
				//don't let text box handle arrow key presses
				e.SuppressKeyPress = true;

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

			return;
		}

		//returns a string with the coordinates and a list of possibilites for this location
		string listPossibilities(int X, int Y)
		{
			String ret = "";
			if (checkBoxDebugTools.Checked)
				ret = "(" + X + "," + Y + ")\t";
			ret += currentGrid.PossibilityString(X, Y);
			return ret;
		}

		private void buttonReset_Click(object sender, EventArgs e)
		{
			currentGrid = new SudokuGrid();
			refreshDisplay();
		}

		private void buttonSnapShot_Click(object sender, EventArgs e)
		{
			snapshotStack.Push(new SudokuGrid(currentGrid));

			buttonRestore.Enabled = true;
			buttonPopSnapshot.Enabled = true;
			buttonPopSnapshot.Text = "Pop Snapshot (" + snapshotStack.Count + ")";
		}

		private void buttonRestore_Click(object sender, EventArgs e)
		{
			currentGrid = new SudokuGrid(snapshotStack.Peek());
			refreshDisplay();
		}

		private void buttonPopSnapshot_Click(object sender, EventArgs e)
		{
			currentGrid = new SudokuGrid(snapshotStack.Pop());
			refreshDisplay();
			
			if (snapshotStack.Count <= 0)
			{
				buttonRestore.Enabled = false;
				buttonPopSnapshot.Enabled = false;
			}
			buttonPopSnapshot.Text = "Pop Snapshot (" + snapshotStack.Count + ")";
		}


		private void checkBoxAuto_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxAuto.Checked)
			{
				currentGrid.solve();
				refreshDisplay();
			}
		}

		private void buttonRecheck_Click(object sender, EventArgs e)
		{
			//force a recheck of the solution
			//NOTE: THIS FUNCTION SHOULD HAVE NO EFFECT when solve() is working correctly!
			currentGrid.dubug_resetOptimizations();
			currentGrid.solve(); 
			refreshDisplay();
		}

		private void displayPossibilities(int x, int y)
		{
			if (currentGrid[x, y] != 0
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
			refreshDisplay();
		}

		private void checkBoxDebugTools_CheckedChanged(object sender, EventArgs e)
		{
			panelDebugTools.Visible = checkBoxDebugTools.Checked;
			ResetSize();
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
			//2. full puzzle I'm still trying to solve. was used to discover pair set technique. Solved after adding xwing
			new testCase
			{
				X = new int[] { 4, 8, 0, 3, 5, 7, 0, 2, 5, 6, 1, 5, 0, 4, 8, 3, 7, 2, 3, 6, 8, 1, 3, 5, 8, 0, 4},
				Y = new int[] { 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 4, 4, 4, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8},
				n = new int[] { 8, 1, 6, 2, 1, 9, 2, 9, 3, 8, 4, 9, 3, 4, 8, 6, 7, 8, 5, 6, 3, 3, 1, 8, 5, 4, 6}
			},
			//3. is able to compute that (2,1) = 2 because of 4's using "X-wing technique". otherwise still unsolved.
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
			//7. new full puzzle. Solved after adding xwing
			new testCase
			{
				X = new int[] { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 5, 5, 5, 5, 6, 6, 7, 7, 8, 8, 8, 8, },
				Y = new int[] { 1, 2, 4, 8, 3, 7, 2, 6, 1, 5, 6, 7, 0, 4, 8, 1, 2, 3, 7, 2, 6, 1, 5, 0, 4, 6, 7, },
				n = new int[] { 6, 2, 3, 4, 4, 3, 9, 8, 2, 6, 5, 1, 8, 4, 6, 1, 3, 9, 8, 8, 6, 9, 7, 1, 8, 3, 5, },
			},

			//8. new full puzzle. helped by set matching.  Solved after adding xwing
			new testCase
			{
				X = new int[] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 3, 3, 3, 4, 4, 5, 5, 5, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8, },
				Y = new int[] { 1, 3, 5, 7, 0, 2, 6, 8, 1, 7, 0, 4, 8, 3, 5, 0, 4, 8, 1, 7, 0, 2, 6, 8, 1, 3, 5, 7, },
				n = new int[] { 4, 2, 7, 9, 9, 3, 7, 2, 1, 8, 8, 6, 3, 3, 2, 6, 1, 5, 6, 3, 3, 4, 6, 9, 5, 6, 9, 1, },
			},
		};

		private void buttonTestCase_Click(object sender, EventArgs e)
		{
			runTestCase(testCases[(int)numericUpDown1.Value]);
		}

		private void runTestCase(testCase test)
		{
			TimeSpan sum = TimeSpan.Zero;

			//TODO: add option for this to debug gui
			//for (int i = 0; i < 100; i++) //uncomment to run test 100 times for better time comparisons
			{
				currentGrid = new SudokuGrid();

				System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();


				for (int j = 0; j < test.X.Length; j++)
				{
					currentGrid.setKnownValue(test.X[j], test.Y[j], test.n[j]);
				}

				if (checkBoxAuto.Checked)
				{
					timer.Start();
					currentGrid.solve(); //TODO: ?call solve after each number to better represent user solve times? Add debug gui Choice?
					timer.Stop();
				}


				refreshDisplay();

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
					if (currentGrid.solveType( i, j) == SudokuGrid.SolveType.Entered)
					{
						X += i.ToString() + ", ";
						Y += j.ToString() + ", ";
						n += currentGrid[i, j].ToString() + ", ";
					}
				}
			}

			X += "},\r\n";
			Y += "},\r\n";
			n += "},\r\n";

			Clipboard.SetText(X + Y + n);
			MessageBox.Show(X + Y + n + "\r\nData copied to clipboard");
		}

		private void buttonPasteTest_Click(object sender, EventArgs e)
		{
			//X = new int[] { 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 7, 7, 7, 7, 8, },
			//Y = new int[] { 3, 4, 5, 0, 2, 3, 5, 8, 0, 1, 3, 0, 1, 2, 3, 4, 6, 7, 0, 1, 2, 3, 4, 6, 7, 0, 1, 3, 4, 5, 6, 7, 3, 4, 5, 6, 7, 3, 4, 5, 8, 6, },
			//n = new int[] { 2, 3, 1, 3, 4, 6, 8, 2, 2, 8, 4, 8, 6, 7, 9, 5, 1, 4, 9, 4, 3, 8, 1, 2, 7, 1, 5, 3, 4, 7, 8, 6, 7, 2, 9, 4, 5, 5, 8, 3, 6, 3, },
			string testCase = Clipboard.GetText();
			//strip out everythign execept numbers, commas, and newlines
			if (testCase.Length == 0)
			{
				MessageBox.Show("No text in clipboard");
				return;
			}

			HashSet<char> allowed = new HashSet<char>("1234567890,\n");
			StringBuilder builder = new StringBuilder(testCase.Length);

			for (int i = 0; i < testCase.Length; i++)
			{
				if(allowed.Contains(testCase[i]))
				{
					builder.Append(testCase[i]);
				}
			}

			testCase = builder.ToString();
			if (testCase.Length == 0)
			{
				MessageBox.Show("No VALID text in clipboard");
				return;
			}

			string[] splitTestCase = testCase.Split(new char[]{'\n'}, System.StringSplitOptions.RemoveEmptyEntries);

			if(splitTestCase.Length != 3)
			{
				MessageBox.Show("Needs exactly 3 value lists: X,Y,n");
				return;
			}

			int[] xArray;
			int[] yArray;
			int[] nArray;

			try{
				xArray = splitTestCase[0].Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries).Select(str => int.Parse(str)).ToArray();
				yArray = splitTestCase[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(str => int.Parse(str)).ToArray();
				nArray = splitTestCase[2].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(str => int.Parse(str)).ToArray();
			}
			catch{
				MessageBox.Show("Could not parse values");
				return;
			}

			if(xArray.Length != yArray.Length || yArray.Length != nArray.Length)
			{
				MessageBox.Show("value lists must be same length");
				return;
			}

			if (!xArray.All(x => x >= 0 && x < 9) || !yArray.All(x => x >= 0 && x < 9) || !nArray.All(x => x > 0 && x <= 9))
			{
				MessageBox.Show("Value out of range");
				return;
			}

			runTestCase(new testCase{X = xArray, Y= yArray, n=nArray});
		}






	}//class Form1
}//namespace
