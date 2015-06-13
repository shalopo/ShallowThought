using System;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ShallowThought
{
	/// <summary>
	/// Summary description for Search.
	/// </summary>
	public class ShallowThought
	{
		//states
		const int INF = 20000;
		const int DRAW = 0;
		const int INVALID_MOVE = -30000;
		const int PRUNED = -30001;

		//rules consts
		const int _50MOVES = 100;
		const int REPETITIONS = 3;

		//algorithm parameters
		const int ORDERING_MAX_MOVES = 1;
		const int ORDER_MIN_DEPTH = 3;
		const int SCOUT_MIN_DEPTH = 3;
		const int SCOUT_MIN_INDEX = 3;
		const int ETC_MAX_TRIALS = 5;
		const int DIFF_TO_PRUNE = 1000;
		const int QUIESCENT_MAX_MOVES = 2;
		const int NUM_SAVED_BOARDS = 30;

		MovesData data;
		int maxMoves;
		//List<MoveInfo>[] moves;
		int player;
		//Board[,] savedBoards;

		Random rand;

		//Statistics
		int nullNodes;
		int nodesCounter;
		int researchCounter;
		int hashedCounter;
		float hashedRatio;
		long acc_nodesCounter;
		long acc_hashedCounter;
		float acc_hashedRatio;

		TimeSpan eval;
		TimeSpan hashPut;
		TimeSpan hashLook;

		public ShallowThought()
		{
			rand = new Random();
			acc_nodesCounter = 0;
			acc_hashedCounter = 0;
		}


		public void Search(MovesData data, int maxMoves, BackgroundWorker w,
			ref string stats, ref int movesCount, ref int movesProgress, ref bool finished,
			ref MoveInfo shallowThoughtMove)
		{
			this.maxMoves = maxMoves;
			this.data = data;
			player = data.board.active;

			nodesCounter = 0;
			researchCounter = 0;
			hashedCounter = 0;
			nullNodes = 0;

			int bestPos = 0, i = -1;
			int child, alpha = -INF;
			bool foundPV = false;

			eval = TimeSpan.Zero;
			hashLook = TimeSpan.Zero;
			hashPut = TimeSpan.Zero;

			List<MoveInfo> moves = new List<MoveInfo>(40);
			data.FilterMoves();
			data.MakeMovesList(moves);

			movesCount = moves.Count;

			//savedBoards = new Board[maxMoves, NUM_SAVED_BOARDS];

			OrderMoves(ref moves, maxMoves - 1, -INF, INF);

			foreach (MoveInfo move in moves)
			{
				i++;
				data.MakeMove(move);

				if (foundPV)
				{
					child = -NegaScout(maxMoves - 1, -alpha - 1, -alpha);
					if (child > alpha)	// && child < INF)
						child = -NegaScout(maxMoves - 1, -INF, -child);
				}
				else
					child = -NegaScout(maxMoves - 1, -INF, -alpha);

				data.board = data.doneBoards.Pop();
				if (child == INVALID_MOVE)
					continue;
				//if (child >= INF)
				//{
				//   alpha = child;
				//   bestPos = i;
				//   break;
				//}
				if (child > alpha)
				{
					alpha = child;
					bestPos = i;
					foundPV = true;
				}
				movesProgress = i;
				SetStats(ref stats);
			}

			shallowThoughtMove = moves[bestPos];
			finished = true;
			data.ttable.ChangeMasterState();
			//MessageBox.Show(hashPut.ToString());
		}


		private void SetStats(ref string stats)
		{
			hashedRatio = (float)hashedCounter / nodesCounter;
			acc_hashedRatio = (float)acc_hashedCounter / acc_nodesCounter;
			stats = "nodes checked: " + nodesCounter +
								"\nhashed ratio: " + hashedRatio +
								"\noverall nodes checked: " + acc_nodesCounter +
								"\noverall hashed ratio: " + acc_hashedRatio +
								"\nnull nodes: " + nullNodes +
								"\nre-search count: " + researchCounter;
		}


		private int MovesCmprByType(MoveInfo m1, MoveInfo m2)
		{
			return Math.Sign((int)m2.type - (int)m1.type);
		}

		private int MovesCmprByResult(MoveInfo m1, MoveInfo m2)
		{
			return Math.Sign(m2.orderingResult - m1.orderingResult);
		}

		private int MovesCmprByResultAndType(MoveInfo m1, MoveInfo m2)
		{
			int tmp = Math.Sign(m2.orderingResult - m1.orderingResult);
			if (tmp != 0)
				return tmp;
			return Math.Sign((int)m2.type - (int)m1.type);
		}

		private void OrderMoves(ref List<MoveInfo> moves, int depth, int alpha, int beta)
		{
			foreach (MoveInfo m in moves)
			{
				data.MakeMove(m);
				m.cachedResultBoard = data.board;
				m.orderingResult = -Evaluate();
				data.UndoMove();
			}

			moves.Sort(MovesCmprByResultAndType);

			//int i = 0;
			//MoveInfo move;
			//int child;
			//while (i < moves.Count)
			//{
			//  move = moves[i];
			//  data.MakeMove(move);
			//  child = -NegaScout(depth - 1, -beta, -alpha);
			//  data.board = data.doneBoards.Pop();
			//  if (child == INVALID_MOVE)
			//  {
			//    moves.RemoveAt(i);
			//    continue;
			//  }
			//  alpha = Math.Max(alpha, child);
			//  //if (alpha >= beta)
			//  //  break;
			//  moves[i].orderingResult = child;
			//  i++;
			//}
			//moves.Sort(new Comparison<MoveInfo>(MovesCmprByResult));
		}


		private int NegaScout(int depth, int alpha, int beta)
		{
			int byHash;
			bool anyLegalMoves = false;
			NODE_TYPE nodeType = NODE_TYPE.FAIL_LOW;
			nodesCounter++;
			acc_nodesCounter++;

			#region hash
			//if ((byHash = data.ttable.LookUp(data.board, depth, alpha, beta)) != C.TT_VALUE_NOT_FOUND)
			//{
			//  hashedCounter++;
			//  acc_hashedCounter++;
			//  return byHash;
			//}
			#endregion

			//if (PruningHeuristic(alpha))
			//  return PRUNED;

			data.GeneratePseudoMoves(data.possibleMoves);

			#region is invalid move
			if (data.InvalidState())
			{
				nullNodes++;
				return -INVALID_MOVE;
			}
			#endregion

			#region draw
			//if (alpha < DRAW && (Repetitive() || data.board.unchangingMoves == _50MOVES))
			//  alpha = DRAW;
			#endregion

			#region leaf + quiescense
			if (depth <= -QUIESCENT_MAX_MOVES || depth <= 0 && QuiescenseHeuristic())
			{
				int res = Evaluate();
				//data.ttable.Store(new Entry(data.board.hashLock, data.board.active, data.board.canCastle,
				//  depth, NODE_TYPE.EXACT, res, data.ttable.masterState),	data.board.hashKey);
				return res;
			}
			#endregion

			#region moves list generation and ordering
			List<MoveInfo> moves = new List<MoveInfo>(40);
			data.MakeMovesList(moves);

			if (depth >= ORDER_MIN_DEPTH)
				OrderMoves(ref moves, ORDERING_MAX_MOVES, alpha, beta);
			#endregion

			#region ETC
			//int ETC_count = 0;
			//foreach (MoveInfo move in moves)
			//{
			//  data.MakeMove(move);
			//  if ((byHash = data.ttable.LookUp(data.board, depth, alpha, beta)) != C.TT_VALUE_NOT_FOUND &&
			//    byHash >= beta)
			//  {
			//    data.board = data.doneBoards.Pop();
			//    hashedCounter++;
			//    acc_hashedCounter++;
			//    return byHash;
			//  }
			//  data.board = data.doneBoards.Pop();
			//  if (ETC_count++ >= ETC_MAX_TRIALS)
			//    break;
			//}
			#endregion

			#region recursion
			int child;
			bool foundPV = false;
			int i = 0;

			foreach (MoveInfo move in moves)
			{
				data.MakeMove(move);

				if (foundPV && i >= SCOUT_MIN_INDEX && depth >= SCOUT_MIN_DEPTH)
				{
					child = -NegaScout(depth - 1, -alpha - 1, -alpha);
					if (child > alpha && child < beta)
						child = -NegaScout(depth - 1, -beta, -child);
				}
				else
					child = -NegaScout(depth - 1, -beta, -alpha);

				data.board = data.doneBoards.Pop();
				if (child == INVALID_MOVE)
					continue;

				anyLegalMoves = true;
				if (child > alpha)
				{
					if ((alpha = child) >= beta)
					{
						nodeType = NODE_TYPE.FAIL_HIGH;
						break;
					}
					nodeType = NODE_TYPE.EXACT;
					foundPV = true;
				}
				i++;
			}
			#endregion

			#region end game check
			if (!anyLegalMoves)
				if (data.KingInCheck())
					return -INF - depth;
				else
					return DRAW;
			#endregion

			//data.ttable.Store(new Entry(data.board.hashLock, data.board.active, data.board.canCastle,
			//  depth, nodeType, alpha, data.ttable.masterState),	data.board.hashKey);
			return alpha;
		}

		private bool QuiescenseHeuristic()
		{
			return (data.allMoves & data.board.pieces[(byte)PIECE.KING, 1 - data.board.active]) == 0 &&
				data.board.materialBal >= -data.doneBoards.Peek().materialBal - 1;
		}

		private bool PruningHeuristic(int alpha)
		{
			return (alpha - Evaluate()) >= DIFF_TO_PRUNE;
		}

		private int Evaluate()
		{
			int res = 80 * data.board.materialBal + Mobility() +2 * PawnStructure() + 3 * PromotionKeening();
			return res;
		}

		private int Mobility()
		{
			//int movesCount = BitMaster.BitCount64(data.allMoves);
			//return movesCount;
			return 0;
		}

		private int PawnStructure()
		{
			UInt64 bpawns = data.board.pieces[(byte)PIECE.PAWN, C.BLACK];
			UInt64 wpawns = data.board.pieces[(byte)PIECE.PAWN, C.WHITE];
			int res =
				BitMaster.BitCount64(bpawns & (bpawns << 7) & ~C.LEFT_COL) +
				BitMaster.BitCount64(bpawns & (bpawns << 9) & ~C.RIGHT_COL) -
				(BitMaster.BitCount64(wpawns & (wpawns >> 7) & ~C.RIGHT_COL) +
				BitMaster.BitCount64(wpawns & (wpawns >> 9) & ~C.LEFT_COL));
			return (data.board.active == C.BLACK) ? res : -res;
		}

		private int PromotionKeening()
		{
			return 0;
		}

		private bool Repetitive()
		{
			int count = 0;
			foreach (Board b in data.doneBoards)
			{
				if (data.board == b)
					count++;
				if (count >= data.board.unchangingMoves)
					return false;
				if (count >= REPETITIONS)
					return true;
			}
			return false;
		}

		//void LogHash()
		//{
		//  StreamWriter sw = new StreamWriter("log.txt");
		//  try
		//  {
		//    foreach (DictionaryEntry de in data.hashInfo.transTable)
		//    {
		//      Result res = (Result)de.Value;
		//      //if (res.moves > 0)
		//        sw.WriteLine(res.moves.ToString() + ", " + res.score.ToString());
		//    }
		//  }
		//  catch{}
		//  finally
		//  {
		//    sw.Close();
		//  }
		//}


	}

	class MoveHolder
	{
		public MoveInfo move;
		public Board afterBoard;
		public int orderingResult;

		public MoveHolder(MoveInfo move, Board afterBoard, int orderingResult)
		{
			this.move = move;
			this.afterBoard = afterBoard;
			this.orderingResult = orderingResult;
		}

	}

}
