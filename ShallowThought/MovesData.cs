using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShallowThought
{
	[Serializable]
	public class MovesData : ISerializable
	{
		const int HashBitsNum = 18;

		public Board board;
		public Stack<Board> doneBoards;
		public Stack<Board> undoneBoards;
		public Board boardToPresent;
		public HashInfo hashInfo;
		public TTable ttable;
		public List<MoveOptions> possibleMoves;

		//temporals
		public UInt64 allMoves;
		List<MoveOptions> tmp;
		UInt64 both;
		UInt64 enemy_or_empty;

		private void Init()
		{
			possibleMoves = new List<MoveOptions>(40);
			doneBoards = new Stack<Board>();
			undoneBoards = new Stack<Board>();
			hashInfo = new HashInfo(HashBitsNum);
			ttable = new TTable((int)Math.Pow(2, HashBitsNum));
			tmp = new List<MoveOptions>(40);
		}

		public MovesData()
		{
			Init();
			board = new Board(ref hashInfo);
			UpdateBoardToPresent();
		}

		public MovesData(SerializationInfo info, StreamingContext context)
		{
			Init();
			board = (Board)info.GetValue("board", typeof(Board));
			UpdateBoardToPresent();
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("board", board);
		}


		public void UpdateBoardToPresent()
		{
			boardToPresent = new Board(board);
		}

		#region move generation

		public void GenerateLegalMoves()
		{
			possibleMoves.Clear();
			GeneratePseudoMoves(possibleMoves);
			FilterMoves();
		}

		public void FilterMoves()
		{
			UInt64 moves, b;
			byte index;
			for (int i = 0; i < possibleMoves.Count; i++)
			{
				MoveOptions mop = possibleMoves[i];
				moves = mop.to;
				while (moves != 0)
				{
					b = moves & (~moves + 1);
					moves ^= b;
					index = (byte)BitMaster.SingleBit64(b);
					MakeMove(mop.from, index, mop.pieceType);
					GeneratePseudoMoves(tmp);
					if (InvalidState())
						possibleMoves[i].to ^= b;
					tmp.Clear();
					board = doneBoards.Pop();
				}
			}
		}

		public void GeneratePseudoMoves(List<MoveOptions> list)
		{
			//set commonly used boards
			both = board.player[0] | board.player[1];
			enemy_or_empty = ~board.player[board.active];
			allMoves = 0;

			list.Clear();
			GenerateQueenMoves(list);
			GenerateBishopMoves(list);
			GenerateKnightMoves(list);
			GenerateRookMoves(list);
			GeneratePawnMoves(list);
			GenerateKingMoves(list);
		}

		void GeneratePawnMoves(List<MoveOptions> list)
		{
			UInt64 pawns = board[PIECE.PAWN];
			UInt64 pawn;
			UInt64 moves;
			int index;
			while (pawns != 0)
			{
				pawn = pawns & (~pawns + 1);
				pawns ^= pawn;
				index = BitMaster.SingleBit64(pawn);
				if (board.active == C.BLACK)
				{
					if ((moves = C.D[index] & ~both) != 0)
						moves |= C.D_2[index] & C.LINE_4 & ~both;
					moves |= C.DIAGD[index] & board.player[1 - board.active];
				}
				else
				{
					if ((moves = C.U[index] & ~both) != 0)
						moves |= C.U_2[index] & C.LINE_5 & ~both;
					moves |= C.DIAGU[index] & board.player[1 - board.active];
				}
				allMoves |= moves;
				list.Add(new MoveOptions(PIECE.PAWN, (byte)index, moves));
			}
		}

		void GenerateKnightMoves(List<MoveOptions> list)
		{
			UInt64 knights = board[PIECE.KNIGHT];

			while (knights != 0)
			{
				UInt64 knight = knights & (~knights + 1);
				knights ^= knight;
				int index = BitMaster.SingleBit64(knight);
				UInt64 moves = C.KNIGHT[index] & ~board.player[board.active];
				allMoves |= moves;
				list.Add(new MoveOptions(PIECE.KNIGHT, (byte)index, moves));
			}
		}

		void GenerateKingMoves(List<MoveOptions> list)
		{
			int index = BitMaster.SingleBit64(board[PIECE.KING]);
			UInt64 castlingMove1 = 0, castlingMove2 = 0,
				moves = C.KING[index] & ~board.player[board.active];

			#region set castling moves and areas
			if (board.active == C.WHITE)
			{
				if (board.CanCastle(CASTLING_TYPE.LOWER_RIGHT) && (both & 0x06) == 0)
					castlingMove1 = 0x01;
				if (board.CanCastle(CASTLING_TYPE.LOWER_LEFT) && (both & 0x70) == 0)
					castlingMove2 = 0x80;
			}
			else
			{
				if (board.CanCastle(CASTLING_TYPE.UPPER_RIGHT) && (both & ((UInt64)0x06 << 56)) == 0)
					castlingMove1 = (UInt64)0x01 << 56;
				if (board.CanCastle(CASTLING_TYPE.UPPER_LEFT) && (both & ((UInt64)0x70 << 56)) == 0)
					castlingMove2 = (UInt64)0x80 << 56;
			}
			#endregion
			
			moves |= castlingMove1 | castlingMove2;
			allMoves |= moves;
			list.Add(new MoveOptions(PIECE.KING, (byte)index, moves));
		}

		UInt64 GetStraightLineMoves(int index)
		{
			UInt64 right_moves = C.RIGHTS[index] & both;
			right_moves = ~((right_moves >> 1) | (right_moves >> 2) | (right_moves >> 3) | (right_moves >> 4) |
					 (right_moves >> 5) | (right_moves >> 6)) & C.RIGHTS[index];

			UInt64 left_moves = C.LEFTS[index] & both;
			left_moves = ~((left_moves << 1) | (left_moves << 2) | (left_moves << 3) | (left_moves << 4) |
					 (left_moves << 5) | (left_moves << 6)) & C.LEFTS[index];

			UInt64 down_moves = C.DOWNS[index] & both;
			down_moves = ~((down_moves >> 8) | (down_moves >> 16) | (down_moves >> 24) | (down_moves >> 32) |
					 (down_moves >> 40) | (down_moves >> 48)) & C.DOWNS[index];

			UInt64 up_moves = C.UPS[index] & both;
			up_moves = ~((up_moves << 8) | (up_moves << 16) | (up_moves << 24) | (up_moves << 32) |
					 (up_moves << 40) | (up_moves << 48)) & C.UPS[index];

			return (right_moves | left_moves | down_moves | up_moves) & enemy_or_empty;
		}

		void GenerateRookMoves(List<MoveOptions> list)
		{
			UInt64 rooks = board[PIECE.ROOK];

			while (rooks != 0)
			{
				UInt64 rook = rooks & (~rooks + 1);
				rooks ^= rook;
				int index = BitMaster.SingleBit64(rook);
				UInt64 moves = GetStraightLineMoves(index);
				allMoves |= moves;
				list.Add(new MoveOptions(PIECE.ROOK, (byte)index, moves));
			}
		}

		UInt64 GetDiagonalMoves(int index)
		{
			UInt64 ur_moves = C.URS[index] & both;
			ur_moves = (~((ur_moves << 7) | (ur_moves << 14) | (ur_moves << 21) | (ur_moves << 28) |
					 ur_moves << 35) | (ur_moves << 42)) & C.URS[index];

			UInt64 ul_moves = C.ULS[index] & both;
			ul_moves = (~((ul_moves << 9) | (ul_moves << 18) | (ul_moves << 27) | (ul_moves << 36) |
					 ul_moves << 45) | (ul_moves << 54)) & C.ULS[index];

			UInt64 dr_moves = C.DRS[index] & both;
			dr_moves = (~((dr_moves >> 9) | (dr_moves >> 18) | (dr_moves >> 27) | (dr_moves >> 36) |
					 dr_moves >> 45) | (dr_moves >> 54)) & C.DRS[index];

			UInt64 dl_moves = C.DLS[index] & both;
			dl_moves = (~((dl_moves >> 7) | (dl_moves >> 14) | (dl_moves >> 21) | (dl_moves >> 28) |
					 dl_moves >> 35) | (dl_moves >> 42)) & C.DLS[index];


			return (ur_moves | ul_moves | dr_moves | dl_moves) & enemy_or_empty;
		}

		void GenerateBishopMoves(List<MoveOptions> list)
		{
			int index;
			UInt64 bishops = board[PIECE.BISHOP];
			UInt64 bishop;

			bishop = bishops & C.XOR_MAP;
			if (bishop != 0)
			{
				index = BitMaster.SingleBit64(bishop);
				UInt64 moves = GetDiagonalMoves(index);
				allMoves |= moves;
				list.Add(new MoveOptions(PIECE.BISHOP, (byte)index, moves));
			}
			bishop = bishops & ~C.XOR_MAP;
			if (bishop != 0)
			{
				index = BitMaster.SingleBit64(bishop);
				UInt64 moves = GetDiagonalMoves(index);
				allMoves |= moves;
				list.Add(new MoveOptions(PIECE.BISHOP, (byte)index, moves));
			}
		}

		void GenerateQueenMoves(List<MoveOptions> list)
		{
			int index;
			UInt64 queens = board[PIECE.QUEEN];
			UInt64 queen;
			while (queens != 0)
			{
				queen = queens & (~queens + 1);
				queens ^= queen;
				index = BitMaster.SingleBit64(queen);
				UInt64 moves = GetStraightLineMoves(index) | GetDiagonalMoves(index);
				allMoves |= moves;
				list.Add(new MoveOptions(PIECE.QUEEN, (byte)index, moves));
			}
		}

		public bool InvalidState()	//after moves calculated
		{
			return
				(allMoves & board.pieces[(byte)PIECE.KING, 1 - board.active]) != 0 ||
				(board.lastCastling != CASTLING_TYPE.NONE && 
					(C.CASTLING_NEUTRAL_AREAS[(int)board.lastCastling] & allMoves) != 0);
		}

		public bool KingInCheck()
		{
			ChangeTurn();
			GeneratePseudoMoves(tmp);
			bool flag = (allMoves & board.pieces[(byte)PIECE.KING, 1 - board.active]) != 0;
			tmp.Clear();
			ChangeTurn();
			return flag;
		}

		//bool CanEatKing(MoveOptions info)
		//{
		//  return (info.to & board[PIECE.KING]) != 0;
		//}

		#endregion

		#region move making

		public void MakeMove(MoveInfo info)
		{
			if (info.cachedResultBoard != null)
			{
				doneBoards.Push(new Board(board));
				board = new Board(info.cachedResultBoard);
			}
			else
				MakeMove(info.from, info.to, info.pieceType);
		}


		public void MakeMove(byte from, byte to, PIECE pieceType)
		{
			doneBoards.Push(new Board(board));
			if (((board.player[0] | board.player[1]) & C.ONE[to]) != 0) // interaction
			{
				if (((board.player[1 - board.active]) & (C.ONE[to])) != 0) // loot
				{
					byte op;		// opponent piece type index
					for (op = 0; op < 6 && ((board.pieces[op, 1 - board.active] & C.ONE[to]) == 0); op++) ;

					board.materialBal += C.WEIGHTS[op];// *(1 - 2 * board.active);

					board.player[1 - board.active] &= C.ZERO[to];
					board.pieces[op, 1 - board.active] &= C.ZERO[to];

					int b = op + 6 * (1 - board.active);
					board.hashKey ^= hashInfo.hashRandKeys[b, to];
					board.hashLock ^= hashInfo.hashRandLocks[b, to];

					board.unchangingMoves = -1;

					if (op == (byte)PIECE.ROOK)
						board.DepriveCastle_Rook(to);
				}
				else	//castling
					ResolveCastling(ref to);
			}

			//regular moves

			if (pieceType == PIECE.PAWN)
			{
				board.unchangingMoves = -1;
				if (!CheckPromotion(from, to))
					MovePiece((byte)pieceType, from, to);
			}
			else
				MovePiece((byte)pieceType, from, to);
			
			#region canCastle modification

			switch (pieceType)
			{
				case PIECE.KING:
					board.DepriveCastle();
					break;
				case PIECE.ROOK:
					board.DepriveCastle_Rook(to);
					break;
			}

			#endregion

			int a = (int)pieceType + 6 * board.active;
			board.hashKey ^= (ushort)(hashInfo.hashRandKeys[a, to] ^ hashInfo.hashRandKeys[a, from]);
			board.hashLock ^= (ushort)(hashInfo.hashRandLocks[a, to] ^ hashInfo.hashRandLocks[a, from]);

			board.unchangingMoves++;
			ChangeTurn();
		}

		void MovePiece(byte pieceType, byte from, byte to)
		{
			board.player[board.active] = (board.player[board.active] & C.ZERO[from]) | C.ONE[to];
			board.pieces[pieceType, board.active] = (board[pieceType] & C.ZERO[from]) | C.ONE[to];
		}

		private bool CheckPromotion(byte from, byte to)
		{
			if ((board.active == 1 && to >= 56 || board.active == 0 && to <= 7))		//promotion
			{
				board.player[board.active] = (board.player[board.active] & C.ZERO[from]) | C.ONE[to];
				board.pieces[(byte)PIECE.QUEEN, board.active] |= C.ONE[to];
				board.pieces[(byte)PIECE.PAWN, board.active] &= C.ZERO[from];
				board.materialBal += C.PROMOTION_DIFF;

				return true;
			}
			return false;
		}

		public void ResolveCastling(ref byte to)
		{
			byte type = (byte)(board.lastCastling = C.CASTLING_MAPPING[to]);
			CastlingOption option = C.CASTLING_OPTIONS[type];

			to = option.endKingPosition;
			MovePiece((byte)PIECE.ROOK, option.startRookPosition, option.endRookPosition);
		}

		public void UndoMove()
		{
			board = doneBoards.Pop();
		}

		public void UndoMove(bool log)
		{
			if (doneBoards.Count == 0)
				return;
			if (log)
				undoneBoards.Push(board);
			UndoMove();
		}

		public void ChangeTurn()
		{
			board.active = (byte)(1 - board.active);
			board.materialBal = -board.materialBal;
		}

		#endregion

		public void MakeMovesList(List<MoveInfo> movesList)
		{
			UInt64 to;
			byte toIndex;
			foreach (MoveOptions op in possibleMoves)
				while (op.to != 0)
				{
					op.to ^= (to = op.to & (~op.to + 1));
					toIndex = (byte)BitMaster.SingleBit64(to);
					movesList.Add(new MoveInfo(op.pieceType, op.from, toIndex, GetMoveType(op.pieceType, op.from, toIndex)));
				}
		}

		public MOVE_TYPE GetMoveType(PIECE pieceType, byte from, byte to)
		{
			if ((C.ONE[to] & board.player[1 - board.active]) == 0)
			{
				if ((C.ONE[to] & board.player[board.active]) == 0)
					return (pieceType == PIECE.PAWN && (board.active == C.WHITE && to >= 56 || board.active == C.BLACK && to <= 7)) ?
						MOVE_TYPE.PROMOTION : MOVE_TYPE.REGULAR;
				else
					return MOVE_TYPE.CASTLING;
			}
			else
				return (pieceType == PIECE.PAWN && (board.active == C.WHITE && to >= 56 || board.active == C.BLACK && to <= 7)) ?
					MOVE_TYPE.CAP_AND_PRO : MOVE_TYPE.CAPTURE;
		}

	}

	public class MoveOptions
	{
		public PIECE pieceType;
		public byte from;
		public UInt64 to;

		public MoveOptions(PIECE pieceType, byte from, UInt64 to)
		{
			this.pieceType = pieceType;
			this.from = from;
			this.to = to;
		}
	}

	public class MoveInfo
	{
		public PIECE pieceType;
		public byte from;
		public byte to;
		public int orderingResult;
		public MOVE_TYPE type;
		public Board cachedResultBoard = null;

		public MoveInfo(PIECE pieceType, byte from, byte to, MOVE_TYPE type)
		{
			this.pieceType = pieceType;
			this.from = from;
			this.to = to;
			orderingResult = 0;
			this.type = type;
		}


	}

	public struct CastlingOption
	{
		public byte endKingPosition;
		public byte startRookPosition;
		public byte endRookPosition;
		public UInt64 deprivation;

		public CastlingOption(byte endKingPosition, byte startRookPosition, byte endRookPosition, UInt64 deprivation)
		{
			this.endKingPosition = endKingPosition;
			this.startRookPosition = startRookPosition;
			this.endRookPosition = endRookPosition;
			this.deprivation = deprivation;
		}
	}

}