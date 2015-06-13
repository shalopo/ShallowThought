using System;
using System.IO;

using HASH = System.UInt32;

namespace ShallowThought
{
	[Serializable]
	public class HashInfo
	{
		public HASH[,] hashRandKeys = new HASH[12, 64];
		public HASH[,] hashRandLocks = new HASH[12, 64];

		public HashInfo(int HashBitsNum)
		{
			int i, j;
			Random r = new Random();
			for (i = 0; i < 12; i++)
				for (j = 0; j < 64; j++)
				{
					hashRandKeys[i, j] = (HASH)(r.Next(0, int.MaxValue) & C.ONES[HashBitsNum]);
					hashRandLocks[i, j] = (HASH)(r.Next(0, int.MaxValue) & C.ONES[HashBitsNum]);
				}
		}

		public HASH InitHashKey(ref HASH[,] hashRand, ref UInt64[,] allPieces)
		{
			HASH hashKey = 0;
			UInt64 pieces, piece;
			int i, j;
			for (i = 0; i < 6; i++)
			{
				for (j = 0; j < 2; j++)
				{
					pieces = allPieces[i, j];
					while (pieces != 0)
					{
						piece = pieces & (~pieces + 1);
						pieces ^= piece;
						hashKey ^= hashRand[i + 6 * j, BitMaster.SingleBit64(piece)];
					}
				}
			}
			return hashKey;
		}

		//public void SaveHashState(StreamWriter sw)
		//{
		//   sw.WriteLine("hashRandKeys=");
		//   SaveHashRand(hashRandKeys, sw);
		//   sw.WriteLine("hashRandLocks=");
		//   SaveHashRand(hashRandLocks, sw);
		//}

		//private void SaveHashRand(HASH[,] r, StreamWriter sw)
		//{
		//   int i, j;
		//   for (i = 0; i < 12; i++)
		//      for (j = 0; j < 64; j++)
		//      {
		//         sw.Write(r[i, j]);
		//         if (j == 63)
		//            sw.Write(sw.NewLine);
		//         else
		//            sw.Write(",");
		//      }
		//}

		//public void LoadHashState(StreamReader sr)
		//{
		//   sr.ReadLine();
		//   LoadHashRand(hashRandKeys, sr);
		//   sr.ReadLine();
		//   LoadHashRand(hashRandLocks, sr);
		//}

		//private void LoadHashRand(HASH[,] r, StreamReader sr)
		//{
		//   int i, j;
		//   string[] line;
		//   for (i = 0; i < 12; i++)
		//   {
		//      line = (sr.ReadLine()).Split(new char[] { ',' });
		//      for (j = 0; j < 64; j++)
		//      {
		//         r[i, j] = HASH.Parse(line[j]);
		//      }
		//   }
		//}

	}

}
