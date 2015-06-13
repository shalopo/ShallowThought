using System;
using System.Drawing;
using System.Collections;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

namespace ShallowThought
{
	public class GameEngine : System.Windows.Forms.Form
	{
		#region vars

		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem5;
		//public static Point[,] sets = new Point[2,16];
		private System.ComponentModel.IContainer components;
		private string path = Application.StartupPath + "/";
		private static readonly string[] NAMES = new string[] { "pawn", "rook", "knight", "bishop", "king", "queen" };
		private const int PIECES_NUM = 6;
		private Color TrColor = Color.FromArgb(255, 255, 128, 64);
		private Point squareFrom;
		private int selectedPieceNum;
		private string stats = "";
		private bool compBusy = false;
		private bool first = true;
		private bool gameHasEnded = false;
		private Bitmap[,] bitmaps;
		private const float write_space = 30;
		private float _boardSide;
		private float sq_size;
		private DateTime gameStart;
		private DateTime thinkingStart;
		private DateTime thinkingEnd;
		private StreamWriter moveLogger;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.RadioButton white1;
		private System.Windows.Forms.RadioButton white2;
		private System.Windows.Forms.RadioButton black2;
		private System.Windows.Forms.RadioButton black1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.ProgressBar progressBar1;
		public MovesData data;
		public ShallowThought shallowThought;
		private System.Windows.Forms.Button Hint;
		private System.Windows.Forms.Label ThinkingDepth;
		private System.Windows.Forms.CheckBox LostSpecials;
		private System.Windows.Forms.Label Taunts;
		private Label Statistics;
		private MenuItem menuItem6;
		private MenuItem menuItem7;
		private System.Windows.Forms.NumericUpDown numericUpDown1;

		#endregion

		#region Init
		public GameEngine()
		{
			InitializeComponent();
			shallowThought = new ShallowThought();
			Start();
			moveLogger = new StreamWriter("movesLog.txt");
			moveLogger.AutoFlush = true;
		}

		private void Start()
		{
			data = new MovesData();
			selectedPieceNum = -1;
			NewTurn();
			gameStart = DateTime.Now;
		}
		#endregion

		#region Main + Dispose bla bla
		[STAThread]
		static void Main()
		{
			Application.Run(new GameEngine());
		}
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.timer = new System.Windows.Forms.Timer();
			this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.label1 = new System.Windows.Forms.Label();
			this.white1 = new System.Windows.Forms.RadioButton();
			this.white2 = new System.Windows.Forms.RadioButton();
			this.black2 = new System.Windows.Forms.RadioButton();
			this.black1 = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.Hint = new System.Windows.Forms.Button();
			this.ThinkingDepth = new System.Windows.Forms.Label();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.LostSpecials = new System.Windows.Forms.CheckBox();
			this.Taunts = new System.Windows.Forms.Label();
			this.Statistics = new System.Windows.Forms.Label();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.SuspendLayout();
			//
			// timer
			//
			this.timer.Interval = 100;
			this.timer.Tick += new EventHandler(TimerTick);
			this.timer.Start();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2,
            this.menuItem3,
            this.menuItem4,
            this.menuItem5,
            this.menuItem6,
            this.menuItem7});
			this.menuItem1.Text = "Game";
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 0;
			this.menuItem2.Text = "Restart";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Enabled = false;
			this.menuItem3.Index = 1;
			this.menuItem3.Text = "Undo move";
			this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Enabled = false;
			this.menuItem4.Index = 2;
			this.menuItem4.Text = "Redo move";
			this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 3;
			this.menuItem5.Text = "Save Game";
			this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 4;
			this.menuItem6.Text = "Load Game";
			this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(620, 368);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(292, 23);
			this.label1.TabIndex = 0;
			// 
			// white1
			// 
			this.white1.Checked = true;
			this.white1.Location = new System.Drawing.Point(48, 32);
			this.white1.Name = "white1";
			this.white1.Size = new System.Drawing.Size(104, 24);
			this.white1.TabIndex = 1;
			this.white1.TabStop = true;
			this.white1.Text = "human";
			// 
			// white2
			// 
			this.white2.Location = new System.Drawing.Point(48, 56);
			this.white2.Name = "white2";
			this.white2.Size = new System.Drawing.Size(104, 24);
			this.white2.TabIndex = 2;
			this.white2.Text = "computer";
			this.white2.CheckedChanged += new System.EventHandler(this.white2_CheckedChanged);
			// 
			// black2
			// 
			this.black2.Location = new System.Drawing.Point(40, 55);
			this.black2.Name = "black2";
			this.black2.Size = new System.Drawing.Size(104, 24);
			this.black2.TabIndex = 4;
			this.black2.Text = "computer";
			this.black2.CheckedChanged += new System.EventHandler(this.black2_CheckedChanged);
			// 
			// black1
			// 
			this.black1.Checked = true;
			this.black1.Location = new System.Drawing.Point(40, 31);
			this.black1.Name = "black1";
			this.black1.Size = new System.Drawing.Size(104, 24);
			this.black1.TabIndex = 3;
			this.black1.TabStop = true;
			this.black1.Text = "human";
			this.black1.CheckedChanged += new System.EventHandler(this.radioButton4_CheckedChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 23);
			this.label2.TabIndex = 5;
			this.label2.Text = "white player:";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(100, 23);
			this.label3.TabIndex = 6;
			this.label3.Text = "black player:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(830, 65);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(170, 100);
			this.label4.TabIndex = 7;
			this.label4.Text = "";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(830, 200);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(250, 50);
			this.label5.TabIndex = 8;
			this.label5.Text = "";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(640, 20);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(100, 23);
			this.label6.TabIndex = 9;
			this.label6.Text = "";
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.Add(this.white2);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.white1);
			this.panel1.Location = new System.Drawing.Point(616, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(200, 100);
			this.panel1.TabIndex = 7;
			// 
			// panel2
			// 
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Controls.Add(this.black2);
			this.panel2.Controls.Add(this.black1);
			this.panel2.Controls.Add(this.label3);
			this.panel2.Location = new System.Drawing.Point(616, 184);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(200, 100);
			this.panel2.TabIndex = 8;
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(620, 400);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(296, 24);
			this.progressBar1.TabIndex = 9;
			this.progressBar1.Visible = false;
			// 
			// Hint
			// 
			this.Hint.BackColor = System.Drawing.SystemColors.Control;
			this.Hint.Location = new System.Drawing.Point(620, 328);
			this.Hint.Name = "Hint";
			this.Hint.Size = new System.Drawing.Size(75, 23);
			this.Hint.TabIndex = 10;
			this.Hint.Text = "Hint";
			this.Hint.UseVisualStyleBackColor = false;
			this.Hint.Click += new System.EventHandler(this.button1_Click);
			// 
			// ThinkingDepth
			// 
			this.ThinkingDepth.Location = new System.Drawing.Point(624, 296);
			this.ThinkingDepth.Name = "ThinkingDepth";
			this.ThinkingDepth.Size = new System.Drawing.Size(96, 23);
			this.ThinkingDepth.TabIndex = 11;
			this.ThinkingDepth.Text = "Thinking Depth";
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Location = new System.Drawing.Point(720, 296);
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(40, 20);
			this.numericUpDown1.TabIndex = 12;
			this.numericUpDown1.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			// 
			// LostSpecials
			// 
			this.LostSpecials.Location = new System.Drawing.Point(616, 24);
			this.LostSpecials.Name = "LostSpecials";
			this.LostSpecials.Size = new System.Drawing.Size(104, 24);
			this.LostSpecials.TabIndex = 13;
			this.LostSpecials.Text = "Lost Specials";
			this.LostSpecials.Visible = false;
			// 
			// Taunts
			// 
			this.Taunts.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
			this.Taunts.ForeColor = System.Drawing.Color.RoyalBlue;
			this.Taunts.Location = new System.Drawing.Point(624, 440);
			this.Taunts.Name = "Taunts";
			this.Taunts.Size = new System.Drawing.Size(296, 40);
			this.Taunts.TabIndex = 14;
			// 
			// Statistics
			// 
			this.Statistics.AutoSize = true;
			this.Statistics.Location = new System.Drawing.Point(841, 67);
			this.Statistics.Name = "Statistics";
			this.Statistics.Size = new System.Drawing.Size(0, 13);
			this.Statistics.TabIndex = 15;
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 5;
			this.menuItem7.Text = "Exit";
			this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
			// 
			// GameEngine
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(968, 700);
			this.Controls.Add(this.Statistics);
			this.Controls.Add(this.Taunts);
			this.Controls.Add(this.LostSpecials);
			this.Controls.Add(this.numericUpDown1);
			this.Controls.Add(this.ThinkingDepth);
			this.Controls.Add(this.Hint);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.panel2);
			this.DoubleBuffered = true;
			this.Menu = this.mainMenu1;
			this.Name = "GameEngine";
			this.Text = "Loading...";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Click += new System.EventHandler(this.Board_Click);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			//this.Icon = new Icon(@"..\..\..\Icon1.ico");
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		#region bitmaps initialization
		private void LoadBitmaps(Graphics g)
		{
			bitmaps = new Bitmap[PIECES_NUM, 2];
			int i, j;
			for (i = 0; i < PIECES_NUM; i++)
				for (j = 0; j < 2; j++)
				{
					bitmaps[i, j] = new Bitmap(path + "Bitmaps\\" + ((j == 0) ? 'b' : 'w') + NAMES[i] + ".bmp");
					bitmaps[i, j].MakeTransparent(TrColor);
				}
		}

		#endregion

		#region board drawing
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Graphics g = e.Graphics;
			_boardSide = Math.Min(600, Math.Min(ClientSize.Height, ClientSize.Width));
			sq_size = (_boardSide - write_space) / 8;
			if (first)
			{
				LoadBitmaps(g);
				this.Text = "Welcome to Shallow Thought!";
				first = false;
			}
			DrawBoard(g);
			DrawPieces(g);
		}

		private void DrawPieces(Graphics g)
		{
			int i, j;
			int[] a = new int[9];
			for (j = 0; j < PIECES_NUM; j++)
			{
				BitMaster.MultiBits64(data.boardToPresent.pieces[j, C.BLACK], a);
				for (i = 0; i < a.Length; i++)
				{
					if (a[i] == -1)
						break;
					PlacePiece(C.BLACK, j, 7 - a[i] % 8, a[i] / 8, g);
				}

				BitMaster.MultiBits64(data.boardToPresent.pieces[j, C.WHITE], a);
				for (i = 0; i < a.Length; i++)
				{
					if (a[i] == -1)
						break;
					PlacePiece(C.WHITE, j, 7 - a[i] % 8, a[i] / 8, g);
				}
			}
		}

		private void DrawBoard(Graphics g)
		{
			Font font = new Font(FontFamily.GenericSansSerif.Name, 8, FontStyle.Bold);
			Pen pen = new Pen(Brushes.Black, 2);
			Brush letters_brush = Brushes.DimGray, filling_brush = Brushes.CadetBlue;

			int serugin = 0;
			for (int i = 0; i < 9; i++)
			{
				if (i < 8)
				{
					serugin = 1 - serugin;
					g.DrawString(((char)((int)'A' + i)).ToString(), font, letters_brush,
					  write_space + sq_size * (i + 1 / (float)2) - 3, _boardSide - write_space / 2);
					g.DrawString((8 - i).ToString(), font, letters_brush, 0, sq_size * (i + 1 / (float)2) - 6);
					for (int j = 0; j < 4; j++)
					{
						RectangleF rect = new RectangleF(write_space + i * sq_size + 3, (j * 2 + serugin) * sq_size + 3, sq_size - 6, sq_size - 6);
						g.FillRectangle(filling_brush, rect);
					}
				}
				g.DrawLine(pen, sq_size * i + write_space, 0, sq_size * i + write_space, _boardSide - write_space);
				g.DrawLine(pen, write_space, sq_size * i, _boardSide, sq_size * i);
			}
			if (squareFrom.X != -1)
			{
				HighlightSquare(new Pen(Brushes.Khaki, 2), squareFrom.Y, squareFrom.X);
				HighlightPieceMoves(true, squareFrom, Brushes.Chartreuse, Brushes.SandyBrown, Brushes.Navy);
			}
		}

		private void PlacePiece(int color, int piece, int x, int y, Graphics g)
		{
			g.DrawImage(bitmaps[piece, color], write_space + sq_size * (x + 1 / (float)10),
			  sq_size * (7 - y + 1 / (float)10), 7 * sq_size / 8, 7 * sq_size / 8);
		}
		#endregion

		#region board interactivity
		private int IdentifyObject(int color, int index)
		{
			if (data.board.ColorOf(index) == color)
				for (int i = 0; i < PIECES_NUM; i++)
					if ((data.board.pieces[i, color] & (C.ONE[index])) != 0)
						return i;
			return -1;
		}

		private void Board_Click(object sender, System.EventArgs e)
		{
			if (compBusy || CompTurn())
				return;
			int c = (int)Math.Floor((MousePosition.X - this.Left - write_space - 4) / sq_size);
			int r = 8 - (int)Math.Ceiling((Control.MousePosition.Y - this.Top - 43) / sq_size);
			if (r >= 0 && r < 8 && c >= 0 && c < 8)
			{
				Point selected = new Point(c, r);
				if (squareFrom.X == -1)
				{
					int index = 8 * r - c + 7;
					int black = IdentifyObject(C.BLACK, index);
					int white = IdentifyObject(C.WHITE, index);
					if (black != -1 && data.board.active == C.BLACK || white != -1 && data.board.active == C.WHITE)
					{
						selectedPieceNum = black + white + 1;
						HighlightPieceMoves(true, selected, Brushes.Chartreuse, Brushes.SandyBrown, Brushes.Navy);
					}
				}
				else if (squareFrom == selected)
				{
					HighlightPieceMoves(false, selected, Brushes.White, Brushes.White, Brushes.White);
					//HighlightSquare(new Pen(Brushes.White, 2), r, c);
					squareFrom = new Point(-1, -1);
				}
				else
				{
					if ((data.possibleMoves[GetMovesIndex(squareFrom)].to & (C.ONE[Trans(selected)])) != 0)
					{
						Point tmp = squareFrom;
						data.undoneBoards.Clear();
						//HighlightSquare(new Pen(Brushes.White, 2), squareFrom.Y, squareFrom.X);
						HighlightPieceMoves(false, squareFrom, Brushes.White, Brushes.White, Brushes.White);
						MakeAndDrawMove(new MoveInfo((PIECE)selectedPieceNum, Trans(tmp), Trans(selected), 0));
						//if (LostSpecials.Checked)
						//  GetLostSpecials(info.player_num, data.players[info.player_num].pieces[info.commiterIndex], info);
						NewTurn();
					}
				}
			}
		}

		byte Trans(Point point)
		{
			return (byte)(7 - point.X + 8 * point.Y);
		}

		int GetMovesIndex(Point point)
		{
			int j;
			for (j = 0; j < 16 && (data.possibleMoves[j].from != 7 - point.X + 8 * point.Y); j++) ;
			return j;
		}
		#endregion

		#region turns and moves handling
		private bool CompTurn()
		{
			return white2.Checked && data.board.active != C.BLACK || black2.Checked && data.board.active == C.BLACK;
		}

		private void TimerTick(object sender, EventArgs e)
		{
			TimeSpan elapsed = (DateTime.Now.Subtract(gameStart));
			TimeSpan thinking = (compBusy) ? DateTime.Now.Subtract(thinkingStart) :
				thinkingEnd.Subtract(thinkingStart);
			this.label5.Text = "elapsedTime: " + elapsed.ToString().Substring(0, 8) +
				"." + elapsed.Milliseconds.ToString()[0];
			if (shallowThoughtMove != null || compBusy)
			{
				this.label5.Text += "\nthinking time: " + thinking.ToString().Substring(0, 8) +
				"." + thinking.Milliseconds.ToString()[0];
				ShowProgress();
			}

			if (workerFinished)
				ShallowThoughtComplete();
		}

		private void NewTurn()
		{
			label6.Text = "Turn: " + ((data.board.active == C.BLACK) ? "Black" : "White");
			data.GenerateLegalMoves();
			squareFrom = new Point(-1, -1);
			BeginPotentialCompTurn();
		}

		private void BeginPotentialCompTurn()
		{
			if (!gameHasEnded && !CheckIfEnd() && CompTurn())
			{
				compBusy = true;
				menuItem1.Enabled = false;
				Hint.Visible = false;
				label1.Text = ((data.board.active == C.BLACK) ? "black" : "white") + " is deep down in shallow thought..";
				Refresh();
				CallShallowThought();
			}
		}

		private void EndCompTurn()
		{
			label1.Text = "";
			HighlightMove(shallowThoughtMove, 1);
			MakeAndDrawMove(shallowThoughtMove);
			//if (LostSpecials.Checked)
			//  GetLostSpecials(info.player_num, data.players[info.player_num].pieces[info.commiterIndex], info);
			//shallowThoughtMove = null;
			menuItem1.Enabled = true;
			compBusy = false;
			NewTurn();
		}

		private bool CheckIfEnd()
		{
			if (data.possibleMoves.Exists(new Predicate<MoveOptions>(NonZero)))
				return false;
			if (data.KingInCheck())
				Declare("Checkmate! " + ((data.board.active == C.BLACK) ? "White" : "Black") + " is the winner!");
			else
				Declare("Stalemate! Everybody's a winner!");
			gameHasEnded = true;
			return true;
		}

		bool NonZero(MoveOptions info)
		{
			return info.to != 0;
		}

		private void Declare(string msg)
		{
			MessageBox.Show(msg);
		}

		private void MakeAndDrawMove(MoveInfo move)
		{
			if (move == null)
			{
				MessageBox.Show("Draw!");
				gameHasEnded = true;
			}
			else
			{
				data.MakeMove(move);
				data.UpdateBoardToPresent();
				this.menuItem3.Enabled = true;
				this.menuItem4.Enabled = false;
				Refresh();
				HighlightSquare(new Pen(Brushes.White, 2), squareFrom.Y, squareFrom.X);
				LogMove(move);
			}
		}

		private void LogMove(MoveInfo move)
		{
			string time = thinkingEnd.Subtract(thinkingStart).TotalMilliseconds.ToString();
			moveLogger.WriteLine(NumToSquare(move.from) + NumToSquare(move.to) + " : " +
				time.Substring(0, Math.Min(9, time.Length)));
		}

		private string NumToSquare(int num)
		{
			return ((char)((num % 8) + 'a')).ToString() + (num / 8 + 1).ToString();
		}

		#endregion

		#region Highlighting
		private void HighlightPieceMoves(bool highlight, Point selected, Brush b1, Brush b2, Brush b3)
		{
			int[] a = new int[28];
			int index = GetMovesIndex(selected);
			if (data.possibleMoves[index].to == 0)
			{
				squareFrom = new Point(-1, -1);
				return;
			}
			HighlightSquare(new Pen((highlight) ? Brushes.Khaki : Brushes.White, 2), selected.Y, selected.X);
			squareFrom = (highlight) ? selected : new Point(-1, -1);
			BitMaster.MultiBits64(data.possibleMoves[index].to, a);
			for (int i = 0; i < 28 && a[i] >= 0; i++)		// 28 is the top maximum of number of moves
			{
				if ((C.ONE[a[i]] & (data.board.player[1 - data.board.active])) != 0)  //loot
					HighlightSquare(new Pen(b2, 2), a[i] / 8, 7 - a[i] % 8);
				else if ((C.ONE[a[i]] & (data.board.player[data.board.active])) != 0)
					HighlightSquare(new Pen(b3, 2), a[i] / 8, 7 - a[i] % 8);
				else
					HighlightSquare(new Pen(b1, 2), a[i] / 8, 7 - a[i] % 8);
			}
		}

		private void HighlightSquare(Pen pen, int r, int c)
		{
			Graphics g = CreateGraphics();
			g.DrawRectangle(pen, write_space + sq_size * c + 2, sq_size * (7 - r) + 2, sq_size - 4, sq_size - 4);
		}

		private void HighlightMove(MoveInfo info, int ticks)
		{
			Refresh();
			Point from = new Point(7 - info.from % 8, info.from / 8);
			Point to = new Point(7 - info.to % 8, info.to / 8);
			for (int i = 1; i <= ticks; i++)
			{
				HighlightSquare(new Pen(Brushes.MediumBlue, 2), from.Y, from.X);
				HighlightSquare(new Pen(Brushes.IndianRed, 2), to.Y, to.X);
				Thread.Sleep(400);
				HighlightSquare(new Pen(Brushes.White, 2), from.Y, from.X);
				HighlightSquare(new Pen(Brushes.White, 2), to.Y, to.X);
				Thread.Sleep(400);
			}
		}
		#endregion

		#region LOST specials
		//private void GetLostSpecials(, PieceInfo piece, MoveOptions info)
		//{      
		//  if (info.loot.occurred && piece.pieceType == (int)PIECE.KNIGHT && player_num == WHITE_NUM)
		//  {
		//    Taunts.Text = "HAVE A CLUCKILY CLUCK CLUCK DAY!";
		//    return;
		//  }
		//  if (info.loot.occurred && piece.pieceType == (int)PIECE.ROOK && player_num == BLACK_NUM)
		//  {
		//    Taunts.Text = "EEEEEHHH, I KINDA ATE HIM...";
		//    return;
		//  }
		//  if (player_num == WHITE_NUM && data.LegalMove(WHITE_NUM, 15, data.players[BLACK_NUM].pieces[14].point, false))
		//  {
		//    Taunts.Text = "GIVE ME MY MEDICINE, JACK!";
		//    return;
		//  }
		//  if (player_num == BLACK_NUM && (data.LegalMove(BLACK_NUM, 12, data.players[WHITE_NUM].pieces[14].point, false)
		//    || data.LegalMove(BLACK_NUM, 13, data.players[WHITE_NUM].pieces[14].point, false)))
		//  {
		//    Taunts.Text = "I COULD USE A TRIANGULATION TO FIND SHANNON";
		//    return;
		//  }
		//  if (info.loot.occurred && info.loot.pieceIndex < 8 && player_num == WHITE_NUM)
		//  {
		//    Taunts.Text = "HEY MY WALT DOLL IS BROKEN!";
		//    return;
		//  }
		//  if (info.loot.occurred && info.loot.pieceIndex < 8 && player_num == BLACK_NUM)
		//  {
		//    Taunts.Text = "NOOOOOOB!!1";
		//    return;
		//  }
		//  if (player_num == BLACK_NUM && info.loot.occurred && (info.loot.pieceIndex == 12 || info.loot.pieceIndex == 13))
		//  {
		//    Taunts.Text = "YOU WERE EVERYBODY!";
		//    return;
		//  }
		//}

		private void WriteTaunt(string msg)
		{
			Font font = new Font("New Times Romans", 8);
			Graphics g = CreateGraphics();
			g.DrawString(msg, font, Brushes.BlueViolet, 60, sq_size * 8);
		}
		#endregion

		#region menu item + button clicks
		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			RestartGame();
		}

		private void menuItem3_Click(object sender, System.EventArgs e)
		{
			UndoMove();
		}

		private void menuItem4_Click(object sender, System.EventArgs e)
		{
			RedoMove();
		}

		private void RestartGame()
		{
			Start();
			Refresh();
			menuItem3.Enabled = false;
		}

		private void RedoMove()
		{
			data.board = data.undoneBoards.Pop();
			data.UpdateBoardToPresent();
			NewTurn();
			Refresh();
			menuItem3.Enabled = true;
			menuItem4.Enabled = (data.undoneBoards.Count > 0);
		}

		private void UndoMove()
		{
			data.UndoMove(true);
			if (CompTurn())
				data.UndoMove(true);
			gameHasEnded = false;
			squareFrom = new Point(-1, -1);
			data.UpdateBoardToPresent();
			data.GenerateLegalMoves();
			Refresh();
			//menuItem3.Enabled = false; // (data.doneBoards.Count > 0);
			menuItem4.Enabled = true;
		}

		private void menuItem5_Click(object sender, System.EventArgs e)
		{
			SaveGameState();
		}

		private void SaveGameState()
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.InitialDirectory = "saves";
			dialog.Filter = "Chess Loster (*.sts)|*.sts";
			dialog.DefaultExt = "sts";
			dialog.AddExtension = true;
			if (dialog.ShowDialog() != DialogResult.OK)
				return;
			StateHelper.Save(data, dialog.FileName);
		}

		private void menuItem6_Click(object sender, EventArgs e)
		{
			LoadGameState();
		}

		private void LoadGameState()
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.InitialDirectory = "saves";
			dialog.Filter = "Chess Loster (*.sts)|*.sts";
			dialog.DefaultExt = "sts";
			dialog.AddExtension = true;
			if (dialog.ShowDialog() != DialogResult.OK)
				return;
			data = (MovesData)StateHelper.Load(dialog.FileName);
			gameHasEnded = false;
			NewTurn();
			Refresh();
		}

		private void menuItem7_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void radioButton4_CheckedChanged(object sender, System.EventArgs e)
		{

		}

		private void white2_CheckedChanged(object sender, System.EventArgs e)
		{
			if (!compBusy)
				NewTurn();
		}

		private void black2_CheckedChanged(object sender, System.EventArgs e)
		{
			if (!compBusy)
				NewTurn();
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			//label1.Text = "Hint will be ready within eternity...";
			//Refresh();
			//MoveInfo info = CallShallowThought(); 
			//label1.Text = "";
			//data.GenerateLegalMoves();
			//HighlightMove(info, 2);
		}
		#endregion

		#region AI call

		private BackgroundWorker w;
		private MoveInfo shallowThoughtMove;
		private int movesProgress;
		private int movesCount;
		private bool workerFinished;

		private void CallShallowThought()
		{
			thinkingStart = DateTime.Now;
			movesProgress = 0;
			progressBar1.Value = 0;
			progressBar1.Maximum = 1;
			progressBar1.Visible = true;
			w = new BackgroundWorker();
			w.WorkerReportsProgress = true;
			w.DoWork += ShallowThoughtStart;
			//w.ProgressChanged += ShallowThoughtProgress;
			//w.RunWorkerCompleted += ShallowThoughtComplete;
			workerFinished = false;
			w.RunWorkerAsync();
			//ShallowThoughtStart(null, null);
		}

		void ShallowThoughtStart(object sender, DoWorkEventArgs e)
		{
			shallowThought.Search(data, (int)numericUpDown1.Value, w, ref stats,
				ref movesCount, ref movesProgress, ref workerFinished, ref shallowThoughtMove);
		}

		//void ShallowThoughtProgress(object sender, ProgressChangedEventArgs e)
		//{
		//  int p = e.ProgressPercentage;
		//  if (p == -1)
		//  {
		//    progressBar1.Value++;
		//    label4.Text = stats;
		//  }
		//  else
		//    progressBar1.Maximum = p;
		//}

		void ShowProgress()
		{
			progressBar1.Maximum = movesCount;
			progressBar1.Value = movesProgress;
			label4.Text = stats;
		}

		void ShallowThoughtComplete()//object sender, RunWorkerCompletedEventArgs e)
		{
			workerFinished = false;
			progressBar1.Visible = false;
			label4.Text = stats;
			ShowProgress();
			//shallowThoughtMove = (MoveInfo)e.Result;
			thinkingEnd = DateTime.Now;
			EndCompTurn();
		}

		#endregion

	}
}

