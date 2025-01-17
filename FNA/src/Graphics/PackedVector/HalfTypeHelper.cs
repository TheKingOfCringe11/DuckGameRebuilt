#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2022 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    internal static class HalfTypeHelper
	{
		#region Private Struct uif

		[StructLayout(LayoutKind.Explicit)]
		private struct uif
		{
			[FieldOffset(0)]
			public float f;
			[FieldOffset(0)]
			public int i;
			[FieldOffset(0)]
			public uint u;
		}

		#endregion

		#region Internal Static Methods

		internal static ushort Convert(float f)
		{
            uif uif = new uif
            {
                f = f
            };
            return Convert(uif.i);
		}

		internal static ushort Convert(int i)
		{
			int s = (i >> 16) & 0x00008000;
			int e = ((i >> 23) & 0x000000ff) - (127 - 15);
			int m = i & 0x007fffff;

			if (e <= 0)
			{
				if (e < -10)
				{
					return (ushort) s;
				}

				m = m | 0x00800000;

				int t = 14 - e;
				int a = (1 << (t - 1)) - 1;
				int b = (m >> t) & 1;

				m = (m + a + b) >> t;

				return (ushort) (s | m);
			}
			else if (e == 0xff - (127 - 15))
			{
				if (m == 0)
				{
					return (ushort) (s | 0x7c00);
				}
				else
				{
					m >>= 13;
					return (ushort) (s | 0x7c00 | m | ((m == 0) ? 1 : 0));
				}
			}
			else
			{
				m = m + 0x00000fff + ((m >> 13) & 1);

				if ((m & 0x00800000) != 0)
				{
					m = 0;
					e += 1;
				}

				if (e > 30)
				{
					return (ushort) (s | 0x7c00);
				}

				return (ushort) (s | (e << 10) | (m >> 13));
			}
		}

		internal static float Convert(ushort value)
		{
			uint rst;
			uint mantissa = (uint)(value & 1023);
			uint exp = 0xfffffff2;

			if ((value & -33792) == 0)
			{
				if (mantissa != 0)
				{
					while ((mantissa & 1024) == 0)
					{
						exp--;
						mantissa = mantissa << 1;
					}
					mantissa &= 0xfffffbff;
					rst = (((uint)value & 0x8000) << 16) | ((exp + 127) << 23) | (mantissa << 13);
				}
				else
				{
					rst = (uint) ((value & 0x8000) << 16);
				}
			}
			else
			{
				rst = ((((uint)value & 0x8000) << 16) | ((((((uint)value >> 10) & 0x1f) - 15) + 127) << 23)) | (mantissa << 13);
			}

            uif uif = new uif
            {
                u = rst
            };
            return uif.f;
		}

		#endregion
	}
}
