using System;

namespace ShallowThought
{
	public class BitMaster
	{
		const UInt32 debruijn32 = 0x077CB531;
		/* table to convert debruijn index to standard index */
		static readonly int[] index32 = new int[32]
		{
			  0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 
			  31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
		};

		//computes index of rightmost 1 of 32-bit vector
		//public static int RightMost32(UInt32 b)
		//{
		//  return index32[(b & (~b + 1)) * debruijn32 >> 27];
		//}

		//computes index of rightmost 1 of 32-bit vector
		//assumes only one bit is set
		private static int SingleBit32(UInt32 b)
		{
			if (b == 0)
				return -1;
			return index32[b * debruijn32 >> 27];
		}

		//computes index of rightmost 1 of 64-bit vector. Assumes only one bit is set
		public static int SingleBit64(UInt64 b)
		{
			int u;
			if ((u = SingleBit32((UInt32)(b >> 32))) == -1)
				return SingleBit32((UInt32)b);
			else
				return u + 32;
		}

		////computes index of the two 1's of 64-bit vector
		//public static void TwoBit64(UInt64 b, int[] a)
		//{
		//  UInt64 v = b & (~b + 1);
		//  a[0] = SingleBit64(v);
		//  b ^= v;
		//  v = b & (~b + 1);
		//  a[1] = SingleBit64(b & (~b + 1));
		//}

		//computes index of <a.Length> 1's of 64-bit vector
		public static void MultiBits64(UInt64 b, int[] a)
		{
			UInt64 v;
			int i;
			for (i = 0; i < a.Length - 1; i++)
			{
				v = b & (~b + 1);
				if ((a[i] = SingleBit64(v)) == -1)
					return;
				b ^= v;
			}
			a[i] = SingleBit64(b & (~b + 1));
		}

		//counts the number of 1's in a 64-bit vector
		public static int BitCount64(UInt64 b)
		{
			int c;
			for (c = 0; b != 0; c++)
				b &= b - 1; // clear the least significant bit set
			return c;
		}
	}
}
