using System;
using System.IO;
using System.Collections.Generic;

using HASH = System.UInt32;

namespace ShallowThought
{
	[Serializable]
	public class Board
	{
		public UInt64[] player;
		public UInt64[,] pieces;		//by order of enum PIECE
		public byte active;
		public HASH hashKey;
		public HASH hashLock;
		public int materialBal;
		public int unchangingMoves;
		public UInt64 castlingOptions;
		public CASTLING_TYPE lastCastling = CASTLING_TYPE.NONE;

		public static readonly UInt64[] startPlayer = new UInt64[2] { 0xFFFF000000000000, 0x000000000000FFFF };
		public static readonly UInt64[,] startPieces = new UInt64[6, 2]{
															{ 0x00FF000000000000, 0x000000000000FF00 },
															{ 0x8100000000000000, 0x0000000000000081 },
															{ 0x4200000000000000, 0x0000000000000042 },
															{ 0x2400000000000000, 0x0000000000000024 },
															{ 0x0800000000000000, 0x0000000000000008 },
															{ 0x1000000000000000, 0x0000000000000010 }};

		public Board(ref HashInfo hash)
		{
			player = (UInt64[])startPlayer.Clone();
			pieces = (UInt64[,])startPieces.Clone();
			hashKey = hash.InitHashKey(ref hash.hashRandKeys, ref pieces);
			hashLock = hash.InitHashKey(ref hash.hashRandLocks, ref pieces);
			active = C.WHITE;
			materialBal = 0;
			unchangingMoves = 0;
			castlingOptions = 0x9100000000000091;
		}

		public Board(Board board)
		{
			player = (UInt64[])board.player.Clone();
			pieces = (UInt64[,])board.pieces.Clone();
			hashKey = board.hashKey;
			hashLock = board.hashLock;
			active = board.active;
			materialBal = board.materialBal;
			unchangingMoves = board.unchangingMoves;
			castlingOptions = board.castlingOptions;
		}

		public override bool Equals(object o)
		{
			Board b = (Board)o;
			return
				//(player == board.player &&
				//pieces == board.pieces &&
				(active == b.active &&
				hashKey == b.hashKey &&
				hashLock == b.hashLock &&
				castlingOptions == b.castlingOptions &&
				lastCastling == b.lastCastling);
			//materialBal == board.materialBal &&
			//unchangingMoves == board.unchangingMoves);
		}

		public bool CanCastle(CASTLING_TYPE type)
		{
			return (C.CASTLING_PARTICIPANTS[(int)type] & castlingOptions) != 0;
		}

		public void DepriveCastle(CASTLING_TYPE type)
		{
			castlingOptions &= ~C.CASTLING_PARTICIPANTS[(int)type];
		}

		public void DepriveCastle()
		{
			castlingOptions &= ~C.BASE_LINE[active];
		}

		public void DepriveCastle_Rook(byte indexOnBoard)
		{
			DepriveCastle(C.CASTLING_MAPPING[indexOnBoard]);
		}

		public bool SquareFreeActive(int index)
		{
			return ((player[active]) & (C.ONE[index])) == 0;
		}

		public int ColorOf(int index)
		{
			UInt64 d = C.ONE[index];
			if (((player[C.BLACK]) & d) != 0)
				return C.BLACK;
			else
				if (((player[C.WHITE]) & d) != 0)
					return C.WHITE;
			return -1;
		}

		public UInt64 this[PIECE pieceType]
		{
			get
			{
				return this.pieces[(byte)pieceType, active];
			}
		}

		public UInt64 this[byte pieceType]
		{
			get
			{
				return this.pieces[pieceType, active];
			}
		}

	}
}
