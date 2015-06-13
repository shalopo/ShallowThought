using System;

using HASH = System.UInt32;
using RESULT = System.Int32;

namespace ShallowThought
{
	public class TTable
	{
		public bool masterState;
		Entry[] RBD;	//Replace by depth

		public TTable(int maxEntries)
		{
			masterState = false;
			RBD = new Entry[maxEntries];
		}

		public void ChangeMasterState()
		{
			masterState = !masterState;
		}

		public RESULT LookUp(Board board, int depth, RESULT alpha, RESULT beta)
		{
			return LookUpTable(RBD, board, depth, alpha, beta);
		}

		private RESULT LookUpTable(Entry[] table, Board board, int depth, RESULT alpha, RESULT beta)
		{
			Entry e = table[board.hashKey];
			if (e != null && e.hashLock == board.hashLock && e.depth >= depth && //(e.depth + depth) % 2 == 0 &&
					e.active == board.active &&
					e.castlingOptions == board.castlingOptions)
			{
				if (e.type == NODE_TYPE.EXACT)
					return e.score;

				if (e.type == NODE_TYPE.FAIL_HIGH && e.score >= beta)
					return beta;

				if (e.type == NODE_TYPE.FAIL_LOW && e.score <= alpha)
					return alpha;
			}
			return C.TT_VALUE_NOT_FOUND;
		}

		public void Store(Entry newE, HASH hash)
		{
			Entry e = RBD[hash];
			if (e == null || newE.depth >= e.depth || e.state != masterState)
				RBD[hash] = newE;
			//RA[hash] = newE;
		}
	}

	public class Entry
	{
		public HASH hashLock;
		public byte active;
		public UInt64 castlingOptions;
		public CASTLING_TYPE lastCastling = CASTLING_TYPE.NONE;
		public int depth;
		public NODE_TYPE type;
		public RESULT score;
		public bool state;

		public Entry(Board board, int depth, NODE_TYPE type, RESULT score, bool state)
		{
			this.hashLock = board.hashLock;
			this.active = board.active;
			this.castlingOptions = board.castlingOptions;
			this.lastCastling = board.lastCastling;
			this.depth = depth;
			this.type = type;
			this.score = score;
			this.state = state;
		}
	}

}
