using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
namespace BKSystem.IO
{
	public class BitStream : System.IO.Stream
	{
		private sealed class BitStreamResources
		{
			private static System.Resources.ResourceManager _resman;
			private static object _oResManLock;
			private static bool _blnLoadingResource;
			private static void InitialiseResourceManager()
			{
				if (BitStream.BitStreamResources._resman == null)
				{
					lock (typeof(BitStream.BitStreamResources))
					{
						if (BitStream.BitStreamResources._resman == null)
						{
							BitStream.BitStreamResources._oResManLock = new object();
							BitStream.BitStreamResources._resman = new System.Resources.ResourceManager("BKSystem.IO.BitStream", typeof(BitStream).Assembly);
						}
					}
				}
			}
			public static string GetString(string name)
			{
				if (BitStream.BitStreamResources._resman == null)
				{
					BitStream.BitStreamResources.InitialiseResourceManager();
				}
				string @string;
				lock (BitStream.BitStreamResources._oResManLock)
				{
					if (BitStream.BitStreamResources._blnLoadingResource)
					{
						return "The resource manager was unable to load the resource: " + name;
					}
					BitStream.BitStreamResources._blnLoadingResource = true;
					@string = BitStream.BitStreamResources._resman.GetString(name, null);
					BitStream.BitStreamResources._blnLoadingResource = false;
				}
				return @string;
			}
		}
		private const int SizeOfByte = 8;
		private const int SizeOfChar = 128;
		private const int SizeOfUInt16 = 16;
		private const int SizeOfUInt32 = 32;
		private const int SizeOfSingle = 32;
		private const int SizeOfUInt64 = 64;
		private const int SizeOfDouble = 64;
		private const uint BitBuffer_SizeOfElement = 32u;
		private const int BitBuffer_SizeOfElement_Shift = 5;
		private const uint BitBuffer_SizeOfElement_Mod = 31u;
		private static uint[] BitMaskHelperLUT = new uint[]
		{
			0u,
			1u,
			3u,
			7u,
			15u,
			31u,
			63u,
			127u,
			255u,
			511u,
			1023u,
			2047u,
			4095u,
			8191u,
			16383u,
			32767u,
			65535u,
			131071u,
			262143u,
			524287u,
			1048575u,
			2097151u,
			4194303u,
			8388607u,
			16777215u,
			33554431u,
			67108863u,
			134217727u,
			268435455u,
			536870911u,
			1073741823u,
			2147483647u,
			4294967295u
		};
		private bool _blnIsOpen = true;
		private uint[] _auiBitBuffer;
		private uint _uiBitBuffer_Length;
		private uint _uiBitBuffer_Index;
		private uint _uiBitBuffer_BitIndex;
		private static System.IFormatProvider _ifp = System.Globalization.CultureInfo.InvariantCulture;
		public override long Length
		{
			get
			{
				if (!this._blnIsOpen)
				{
					throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
				}
				return (long)((ulong)this._uiBitBuffer_Length);
			}
		}
		public virtual long Length8
		{
			get
			{
				if (!this._blnIsOpen)
				{
					throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
				}
				return (long)((ulong)(this._uiBitBuffer_Length >> 3) + (ulong)(((this._uiBitBuffer_Length & 7u) > 0u) ? 1L : 0L));
			}
		}
		public virtual long Length16
		{
			get
			{
				if (!this._blnIsOpen)
				{
					throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
				}
				return (long)((ulong)(this._uiBitBuffer_Length >> 4) + (ulong)(((this._uiBitBuffer_Length & 15u) > 0u) ? 1L : 0L));
			}
		}
		public virtual long Length32
		{
			get
			{
				if (!this._blnIsOpen)
				{
					throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
				}
				return (long)((ulong)(this._uiBitBuffer_Length >> 5) + (ulong)(((this._uiBitBuffer_Length & 31u) > 0u) ? 1L : 0L));
			}
		}
		public virtual long Length64
		{
			get
			{
				if (!this._blnIsOpen)
				{
					throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
				}
				return (long)((ulong)(this._uiBitBuffer_Length >> 6) + (ulong)(((this._uiBitBuffer_Length & 63u) > 0u) ? 1L : 0L));
			}
		}
		public virtual long Capacity
		{
			get
			{
				if (!this._blnIsOpen)
				{
					throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
				}
				return (long)this._auiBitBuffer.Length << 5;
			}
		}
		public override long Position
		{
			get
			{
				if (!this._blnIsOpen)
				{
					throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
				}
				uint num = (this._uiBitBuffer_Index << 5) + this._uiBitBuffer_BitIndex;
				return (long)((ulong)num);
			}
			set
			{
				if (!this._blnIsOpen)
				{
					throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
				}
				if (value < 0L)
				{
					throw new System.ArgumentOutOfRangeException("value", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativePosition"));
				}
				uint num = (uint)value;
				if (this._uiBitBuffer_Length < num)
				{
					throw new System.ArgumentOutOfRangeException("value", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_InvalidPosition"));
				}
				this._uiBitBuffer_Index = num >> 5;
				if ((num & 31u) > 0u)
				{
					this._uiBitBuffer_BitIndex = (num & 31u);
					return;
				}
				this._uiBitBuffer_BitIndex = 0u;
			}
		}
		public override bool CanRead
		{
			get
			{
				return this._blnIsOpen;
			}
		}
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}
		public override bool CanWrite
		{
			get
			{
				return this._blnIsOpen;
			}
		}
		public static bool CanSetLength
		{
			get
			{
				return false;
			}
		}
		public static bool CanFlush
		{
			get
			{
				return false;
			}
		}
		public BitStream()
		{
			this._auiBitBuffer = new uint[1];
		}
		public BitStream(long capacity)
		{
			if (capacity <= 0L)
			{
				throw new System.ArgumentOutOfRangeException(BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeOrZeroCapacity"));
			}
			this._auiBitBuffer = new uint[(capacity >> 5) + (((capacity & 31L) > 0L) ? 1L : 0L)];
		}
		public BitStream(System.IO.Stream bits) : this()
		{
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			byte[] buffer = new byte[bits.Length];
			long position = bits.Position;
			bits.Position = 0L;
			bits.Read(buffer, 0, (int)bits.Length);
			bits.Position = position;
			this.Write(buffer, 0, (int)bits.Length);
		}
		private void Write(ref uint bits, ref uint bitIndex, ref uint count)
		{
			uint num = (this._uiBitBuffer_Index << 5) + this._uiBitBuffer_BitIndex;
			uint num2 = this._uiBitBuffer_Length >> 5;
			uint num3 = bitIndex + count;
			int num4 = (int)bitIndex;
			uint num5 = BitStream.BitMaskHelperLUT[(int)((System.UIntPtr)count)] << num4;
			bits &= num5;
			uint num6 = 32u - this._uiBitBuffer_BitIndex;
			num4 = (int)(num6 - num3);
			uint num7;
			if (num4 < 0)
			{
				num7 = bits >> System.Math.Abs(num4);
			}
			else
			{
				num7 = bits << num4;
			}
			if (this._uiBitBuffer_Length >= num + 1u)
			{
				int num8 = (int)(num6 - count);
				uint num9;
				if (num8 < 0)
				{
					num9 = (4294967295u ^ BitStream.BitMaskHelperLUT[(int)((System.UIntPtr)count)] >> System.Math.Abs(num8));
				}
				else
				{
					num9 = (4294967295u ^ BitStream.BitMaskHelperLUT[(int)((System.UIntPtr)count)] << num8);
				}
				this._auiBitBuffer[(int)((System.UIntPtr)this._uiBitBuffer_Index)] &= num9;
				if (num2 == this._uiBitBuffer_Index)
				{
					uint num10;
					if (num6 >= count)
					{
						num10 = num + count;
					}
					else
					{
						num10 = num + num6;
					}
					if (num10 > this._uiBitBuffer_Length)
					{
						uint bits2 = num10 - this._uiBitBuffer_Length;
						this.UpdateLengthForWrite(bits2);
					}
				}
			}
			else if (num6 >= count)
			{
				this.UpdateLengthForWrite(count);
			}
			else
			{
				this.UpdateLengthForWrite(num6);
			}
			this._auiBitBuffer[(int)((System.UIntPtr)this._uiBitBuffer_Index)] |= num7;
			if (num6 >= count)
			{
				this.UpdateIndicesForWrite(count);
				return;
			}
			this.UpdateIndicesForWrite(num6);
			uint num11 = count - num6;
			uint num12 = bitIndex;
			this.Write(ref bits, ref num12, ref num11);
		}
		public virtual void Write(bool bit)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			uint num = bit ? 1u : 0u;
			uint num2 = 0u;
			uint num3 = 1u;
			this.Write(ref num, ref num2, ref num3);
		}
		public virtual void Write(bool[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			this.Write(bits, 0, bits.Length);
		}
		public virtual void Write(bool[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			for (int i = offset; i < num; i++)
			{
				this.Write(bits[i]);
			}
		}
		public virtual void Write(byte bits)
		{
			this.Write(bits, 0, 8);
		}
		public virtual void Write(byte bits, int bitIndex, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bitIndex < 0)
			{
				throw new System.ArgumentOutOfRangeException("bitIndex", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > 8 - bitIndex)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_Byte"));
			}
			uint num = (uint)bits;
			uint num2 = (uint)bitIndex;
			uint num3 = (uint)count;
			this.Write(ref num, ref num2, ref num3);
		}
		public virtual void Write(byte[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			this.Write(bits, 0, bits.Length);
		}
		public override void Write(byte[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			for (int i = offset; i < num; i++)
			{
				this.Write(bits[i]);
			}
		}
		public virtual void Write(sbyte bits)
		{
			this.Write(bits, 0, 8);
		}
		public virtual void Write(sbyte bits, int bitIndex, int count)
		{
			byte bits2 = (byte)bits;
			this.Write(bits2, bitIndex, count);
		}
		public virtual void Write(sbyte[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			this.Write(bits, 0, bits.Length);
		}
		public virtual void Write(sbyte[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			byte[] array = new byte[count];
			System.Buffer.BlockCopy(bits, offset, array, 0, count);
			this.Write(array, 0, count);
		}
		public override void WriteByte(byte value)
		{
			this.Write(value);
		}
		public virtual void Write(char bits)
		{
			this.Write(bits, 0, 128);
		}
		public virtual void Write(char bits, int bitIndex, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bitIndex < 0)
			{
				throw new System.ArgumentOutOfRangeException("bitIndex", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > 128 - bitIndex)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_Char"));
			}
			uint num = (uint)bits;
			uint num2 = (uint)bitIndex;
			uint num3 = (uint)count;
			this.Write(ref num, ref num2, ref num3);
		}
		public virtual void Write(char[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			this.Write(bits, 0, bits.Length);
		}
		public virtual void Write(char[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			for (int i = offset; i < num; i++)
			{
				this.Write(bits[i]);
			}
		}
		public virtual void Write(ushort bits)
		{
			this.Write(bits, 0, 16);
		}
		public virtual void Write(ushort bits, int bitIndex, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bitIndex < 0)
			{
				throw new System.ArgumentOutOfRangeException("bitIndex", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > 16 - bitIndex)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_UInt16"));
			}
			uint num = (uint)bits;
			uint num2 = (uint)bitIndex;
			uint num3 = (uint)count;
			this.Write(ref num, ref num2, ref num3);
		}
		public virtual void Write(ushort[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			this.Write(bits, 0, bits.Length);
		}
		public virtual void Write(ushort[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			for (int i = offset; i < num; i++)
			{
				this.Write(bits[i]);
			}
		}
		public virtual void Write(short bits)
		{
			this.Write(bits, 0, 16);
		}
		public virtual void Write(short bits, int bitIndex, int count)
		{
			ushort bits2 = (ushort)bits;
			this.Write(bits2, bitIndex, count);
		}
		public virtual void Write(short[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			this.Write(bits, 0, bits.Length);
		}
		public virtual void Write(short[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			ushort[] array = new ushort[count];
			System.Buffer.BlockCopy(bits, offset << 1, array, 0, count << 1);
			this.Write(array, 0, count);
		}
		public virtual void Write(uint bits)
		{
			this.Write(bits, 0, 32);
		}
		public virtual void Write(uint bits, int bitIndex, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bitIndex < 0)
			{
				throw new System.ArgumentOutOfRangeException("bitIndex", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > 32 - bitIndex)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_UInt32"));
			}
			uint num = (uint)bitIndex;
			uint num2 = (uint)count;
			this.Write(ref bits, ref num, ref num2);
		}
		public virtual void Write(uint[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			this.Write(bits, 0, bits.Length);
		}
		public virtual void Write(uint[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			for (int i = offset; i < num; i++)
			{
				this.Write(bits[i]);
			}
		}
		public virtual void Write(int bits)
		{
			this.Write(bits, 0, 32);
		}
		public virtual void Write(int bits, int bitIndex, int count)
		{
			this.Write((uint)bits, bitIndex, count);
		}
		public virtual void Write(int[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			this.Write(bits, 0, bits.Length);
		}
		public virtual void Write(int[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			uint[] array = new uint[count];
			System.Buffer.BlockCopy(bits, offset << 2, array, 0, count << 2);
			this.Write(array, 0, count);
		}
		public virtual void Write(float bits)
		{
			this.Write(bits, 0, 32);
		}
		public virtual void Write(float bits, int bitIndex, int count)
		{
			byte[] bytes = System.BitConverter.GetBytes(bits);
			uint bits2 = (uint)((int)bytes[0] | (int)bytes[1] << 8 | (int)bytes[2] << 16 | (int)bytes[3] << 24);
			this.Write(bits2, bitIndex, count);
		}
		public virtual void Write(float[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			this.Write(bits, 0, bits.Length);
		}
		public virtual void Write(float[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			for (int i = offset; i < num; i++)
			{
				this.Write(bits[i]);
			}
		}
		public virtual void Write(ulong bits)
		{
			this.Write(bits, 0, 64);
		}
		public virtual void Write(ulong bits, int bitIndex, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bitIndex < 0)
			{
				throw new System.ArgumentOutOfRangeException("bitIndex", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > 64 - bitIndex)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_UInt64"));
			}
			int num = (bitIndex >> 5 < 1) ? bitIndex : -1;
			int num2 = (bitIndex + count > 32) ? ((num < 0) ? (bitIndex - 32) : 0) : -1;
			int num3 = (num > -1) ? ((num + count > 32) ? (32 - num) : count) : 0;
			int num4 = (num2 > -1) ? ((num3 == 0) ? count : (count - num3)) : 0;
			if (num3 > 0)
			{
				uint num5 = (uint)bits;
				uint num6 = (uint)num;
				uint num7 = (uint)num3;
				this.Write(ref num5, ref num6, ref num7);
			}
			if (num4 > 0)
			{
				uint num8 = (uint)(bits >> 32);
				uint num9 = (uint)num2;
				uint num10 = (uint)num4;
				this.Write(ref num8, ref num9, ref num10);
			}
		}
		public virtual void Write(ulong[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			this.Write(bits, 0, bits.Length);
		}
		public virtual void Write(ulong[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			for (int i = offset; i < num; i++)
			{
				this.Write(bits[i]);
			}
		}
		public virtual void Write(long bits)
		{
			this.Write(bits, 0, 64);
		}
		public virtual void Write(long bits, int bitIndex, int count)
		{
			this.Write((ulong)bits, bitIndex, count);
		}
		public virtual void Write(long[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			this.Write(bits, 0, bits.Length);
		}
		public virtual void Write(long[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			ulong[] array = new ulong[count];
			System.Buffer.BlockCopy(bits, offset << 4, array, 0, count << 4);
			this.Write(array, 0, count);
		}
		public virtual void Write(double bits)
		{
			this.Write(bits, 0, 64);
		}
		public virtual void Write(double bits, int bitIndex, int count)
		{
			byte[] bytes = System.BitConverter.GetBytes(bits);
			ulong bits2 = (ulong)bytes[0] | (ulong)bytes[1] << 8 | (ulong)bytes[2] << 16 | (ulong)bytes[3] << 24 | (ulong)bytes[4] << 32 | (ulong)bytes[5] << 40 | (ulong)bytes[6] << 48 | (ulong)bytes[7] << 56;
			this.Write(bits2, bitIndex, count);
		}
		public virtual void Write(double[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			this.Write(bits, 0, bits.Length);
		}
		public virtual void Write(double[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			for (int i = offset; i < num; i++)
			{
				this.Write(bits[i]);
			}
		}
		public virtual void WriteTo(System.IO.Stream bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_Stream"));
			}
			byte[] array = this.ToByteArray();
			bits.Write(array, 0, array.Length);
		}
		private uint Read(ref uint bits, ref uint bitIndex, ref uint count)
		{
			uint num = (this._uiBitBuffer_Index << 5) + this._uiBitBuffer_BitIndex;
			uint num2 = count;
			if (this._uiBitBuffer_Length < num + num2)
			{
				num2 = this._uiBitBuffer_Length - num;
			}
			uint num3 = this._auiBitBuffer[(int)((System.UIntPtr)this._uiBitBuffer_Index)];
			int num4 = (int)(32u - (this._uiBitBuffer_BitIndex + num2));
			if (num4 < 0)
			{
				num4 = System.Math.Abs(num4);
				uint num5 = BitStream.BitMaskHelperLUT[(int)((System.UIntPtr)num2)] >> num4;
				num3 &= num5;
				num3 <<= num4;
				uint num6 = (uint)num4;
				uint num7 = 0u;
				uint num8 = 0u;
				this.UpdateIndicesForRead(num2 - num6);
				this.Read(ref num8, ref num7, ref num6);
				num3 |= num8;
			}
			else
			{
				uint num9 = BitStream.BitMaskHelperLUT[(int)((System.UIntPtr)num2)] << num4;
				num3 &= num9;
				num3 >>= num4;
				this.UpdateIndicesForRead(num2);
			}
			bits = num3 << (int)bitIndex;
			return num2;
		}
		public virtual int Read(out bool bit)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			uint num = 0u;
			uint num2 = 1u;
			uint value = 0u;
			uint result = this.Read(ref value, ref num, ref num2);
			bit = System.Convert.ToBoolean(value);
			return (int)result;
		}
		public virtual int Read(bool[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			return this.Read(bits, 0, bits.Length);
		}
		public virtual int Read(bool[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			int num2 = 0;
			for (int i = offset; i < num; i++)
			{
				num2 += this.Read(out bits[i]);
			}
			return num2;
		}
		public virtual int Read(out byte bits)
		{
			return this.Read(out bits, 0, 8);
		}
		public virtual int Read(out byte bits, int bitIndex, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bitIndex < 0)
			{
				throw new System.ArgumentOutOfRangeException("bitIndex", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > 8 - bitIndex)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_Byte"));
			}
			uint num = (uint)bitIndex;
			uint num2 = (uint)count;
			uint num3 = 0u;
			uint result = this.Read(ref num3, ref num, ref num2);
			bits = (byte)num3;
			return (int)result;
		}
		public virtual int Read(byte[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			return this.Read(bits, 0, bits.Length);
		}
		public override int Read(byte[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			int num2 = 0;
			for (int i = offset; i < num; i++)
			{
				num2 += this.Read(out bits[i]);
			}
			return num2;
		}
		public virtual int Read(out sbyte bits)
		{
			return this.Read(out bits, 0, 8);
		}
		public virtual int Read(out sbyte bits, int bitIndex, int count)
		{
			byte b = 0;
			int result = this.Read(out b, bitIndex, count);
			bits = (sbyte)b;
			return result;
		}
		public virtual int Read(sbyte[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			return this.Read(bits, 0, bits.Length);
		}
		public virtual int Read(sbyte[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			int num2 = 0;
			for (int i = offset; i < num; i++)
			{
				num2 += this.Read(out bits[i]);
			}
			return num2;
		}
		public override int ReadByte()
		{
			byte result;
			if (this.Read(out result) == 0)
			{
				return -1;
			}
			return (int)result;
		}
		public virtual byte[] ToByteArray()
		{
			long position = this.Position;
			this.Position = 0L;
			byte[] array = new byte[this.Length8];
			this.Read(array, 0, (int)this.Length8);
			if (this.Position != position)
			{
				this.Position = position;
			}
			return array;
		}
		public virtual int Read(out char bits)
		{
			return this.Read(out bits, 0, 128);
		}
		public virtual int Read(out char bits, int bitIndex, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bitIndex < 0)
			{
				throw new System.ArgumentOutOfRangeException("bitIndex", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > 128 - bitIndex)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_Char"));
			}
			uint num = (uint)bitIndex;
			uint num2 = (uint)count;
			uint num3 = 0u;
			uint result = this.Read(ref num3, ref num, ref num2);
			bits = (char)num3;
			return (int)result;
		}
		public virtual int Read(char[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			return this.Read(bits, 0, bits.Length);
		}
		public virtual int Read(char[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			int num2 = 0;
			for (int i = offset; i < num; i++)
			{
				num2 += this.Read(out bits[i]);
			}
			return num2;
		}
		public virtual int Read(out ushort bits)
		{
			return this.Read(out bits, 0, 16);
		}
		public virtual int Read(out ushort bits, int bitIndex, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bitIndex < 0)
			{
				throw new System.ArgumentOutOfRangeException("bitIndex", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > 16 - bitIndex)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_UInt16"));
			}
			uint num = (uint)bitIndex;
			uint num2 = (uint)count;
			uint num3 = 0u;
			uint result = this.Read(ref num3, ref num, ref num2);
			bits = (ushort)num3;
			return (int)result;
		}
		public virtual int Read(ushort[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			return this.Read(bits, 0, bits.Length);
		}
		public virtual int Read(ushort[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			int num2 = 0;
			for (int i = offset; i < num; i++)
			{
				num2 += this.Read(out bits[i]);
			}
			return num2;
		}
		public virtual int Read(out short bits)
		{
			return this.Read(out bits, 0, 16);
		}
		public virtual int Read(out short bits, int bitIndex, int count)
		{
			ushort num = 0;
			int result = this.Read(out num, bitIndex, count);
			bits = (short)num;
			return result;
		}
		public virtual int Read(short[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			return this.Read(bits, 0, bits.Length);
		}
		public virtual int Read(short[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			int num2 = 0;
			for (int i = offset; i < num; i++)
			{
				num2 += this.Read(out bits[i]);
			}
			return num2;
		}
		public virtual int Read(out uint bits)
		{
			return this.Read(out bits, 0, 32);
		}
		public virtual int Read(out uint bits, int bitIndex, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bitIndex < 0)
			{
				throw new System.ArgumentOutOfRangeException("bitIndex", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > 32 - bitIndex)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_UInt32"));
			}
			uint num = (uint)bitIndex;
			uint num2 = (uint)count;
			uint num3 = 0u;
			uint result = this.Read(ref num3, ref num, ref num2);
			bits = num3;
			return (int)result;
		}
		public virtual int Read(uint[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			return this.Read(bits, 0, bits.Length);
		}
		public virtual int Read(uint[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			int num2 = 0;
			for (int i = offset; i < num; i++)
			{
				num2 += this.Read(out bits[i]);
			}
			return num2;
		}
		public virtual int Read(out int bits)
		{
			return this.Read(out bits, 0, 32);
		}
		public virtual int Read(out int bits, int bitIndex, int count)
		{
			uint num = 0u;
			int result = this.Read(out num, bitIndex, count);
			bits = (int)num;
			return result;
		}
		public virtual int Read(int[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			return this.Read(bits, 0, bits.Length);
		}
		public virtual int Read(int[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			int num2 = 0;
			for (int i = offset; i < num; i++)
			{
				num2 += this.Read(out bits[i]);
			}
			return num2;
		}
		public virtual int Read(out float bits)
		{
			return this.Read(out bits, 0, 32);
		}
		public virtual int Read(out float bits, int bitIndex, int count)
		{
			int value = 0;
			int result = this.Read(out value, bitIndex, count);
			bits = System.BitConverter.ToSingle(System.BitConverter.GetBytes(value), 0);
			return result;
		}
		public virtual int Read(float[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			return this.Read(bits, 0, bits.Length);
		}
		public virtual int Read(float[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			int num2 = 0;
			for (int i = offset; i < num; i++)
			{
				num2 += this.Read(out bits[i]);
			}
			return num2;
		}
		public virtual int Read(out ulong bits)
		{
			return this.Read(out bits, 0, 64);
		}
		public virtual int Read(out ulong bits, int bitIndex, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bitIndex < 0)
			{
				throw new System.ArgumentOutOfRangeException("bitIndex", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > 64 - bitIndex)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_UInt64"));
			}
			int num = (bitIndex >> 5 < 1) ? bitIndex : -1;
			int num2 = (bitIndex + count > 32) ? ((num < 0) ? (bitIndex - 32) : 0) : -1;
			int num3 = (num > -1) ? ((num + count > 32) ? (32 - num) : count) : 0;
			int num4 = (num2 > -1) ? ((num3 == 0) ? count : (count - num3)) : 0;
			uint num5 = 0u;
			uint num6 = 0u;
			uint num7 = 0u;
			if (num3 > 0)
			{
				uint num8 = (uint)num;
				uint num9 = (uint)num3;
				num5 = this.Read(ref num6, ref num8, ref num9);
			}
			if (num4 > 0)
			{
				uint num10 = (uint)num2;
				uint num11 = (uint)num4;
				num5 += this.Read(ref num7, ref num10, ref num11);
			}
			bits = ((ulong)num7 << 32 | (ulong)num6);
			return (int)num5;
		}
		public virtual int Read(ulong[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			return this.Read(bits, 0, bits.Length);
		}
		public virtual int Read(ulong[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			int num2 = 0;
			for (int i = offset; i < num; i++)
			{
				num2 += this.Read(out bits[i]);
			}
			return num2;
		}
		public virtual int Read(out long bits)
		{
			return this.Read(out bits, 0, 64);
		}
		public virtual int Read(out long bits, int bitIndex, int count)
		{
			ulong num = 0uL;
			int result = this.Read(out num, bitIndex, count);
			bits = (long)num;
			return result;
		}
		public virtual int Read(long[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			return this.Read(bits, 0, bits.Length);
		}
		public virtual int Read(long[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			int num2 = 0;
			for (int i = offset; i < num; i++)
			{
				num2 += this.Read(out bits[i]);
			}
			return num2;
		}
		public virtual int Read(out double bits)
		{
			return this.Read(out bits, 0, 64);
		}
		public virtual int Read(out double bits, int bitIndex, int count)
		{
			ulong value = 0uL;
			int result = this.Read(out value, bitIndex, count);
			bits = System.BitConverter.ToDouble(System.BitConverter.GetBytes(value), 0);
			return result;
		}
		public virtual int Read(double[] bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			return this.Read(bits, 0, bits.Length);
		}
		public virtual int Read(double[] bits, int offset, int count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitBuffer"));
			}
			if (offset < 0)
			{
				throw new System.ArgumentOutOfRangeException("offset", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", BitStream.BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
			}
			if (count > bits.Length - offset)
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_InvalidCountOrOffset"));
			}
			int num = offset + count;
			int num2 = 0;
			for (int i = offset; i < num; i++)
			{
				num2 += this.Read(out bits[i]);
			}
			return num2;
		}
		public virtual BitStream And(BitStream bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitStream"));
			}
			if (bits.Length != (long)((ulong)this._uiBitBuffer_Length))
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_DifferentBitStreamLengths"));
			}
			BitStream bitStream = new BitStream((long)((ulong)this._uiBitBuffer_Length));
			uint num = this._uiBitBuffer_Length >> 5;
			uint num2;
			for (num2 = 0u; num2 < num; num2 += 1u)
			{
				bitStream._auiBitBuffer[(int)((System.UIntPtr)num2)] = (this._auiBitBuffer[(int)((System.UIntPtr)num2)] & bits._auiBitBuffer[(int)((System.UIntPtr)num2)]);
			}
			if ((this._uiBitBuffer_Length & 31u) > 0u)
			{
				uint num3 = 4294967295u << (int)(32u - (this._uiBitBuffer_Length & 31u));
				bitStream._auiBitBuffer[(int)((System.UIntPtr)num2)] = (this._auiBitBuffer[(int)((System.UIntPtr)num2)] & bits._auiBitBuffer[(int)((System.UIntPtr)num2)] & num3);
			}
			return bitStream;
		}
		public virtual BitStream Or(BitStream bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitStream"));
			}
			if (bits.Length != (long)((ulong)this._uiBitBuffer_Length))
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_DifferentBitStreamLengths"));
			}
			BitStream bitStream = new BitStream((long)((ulong)this._uiBitBuffer_Length));
			uint num = this._uiBitBuffer_Length >> 5;
			uint num2;
			for (num2 = 0u; num2 < num; num2 += 1u)
			{
				bitStream._auiBitBuffer[(int)((System.UIntPtr)num2)] = (this._auiBitBuffer[(int)((System.UIntPtr)num2)] | bits._auiBitBuffer[(int)((System.UIntPtr)num2)]);
			}
			if ((this._uiBitBuffer_Length & 31u) > 0u)
			{
				uint num3 = 4294967295u << (int)(32u - (this._uiBitBuffer_Length & 31u));
				bitStream._auiBitBuffer[(int)((System.UIntPtr)num2)] = (this._auiBitBuffer[(int)((System.UIntPtr)num2)] | (bits._auiBitBuffer[(int)((System.UIntPtr)num2)] & num3));
			}
			return bitStream;
		}
		public virtual BitStream Xor(BitStream bits)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitStream"));
			}
			if (bits.Length != (long)((ulong)this._uiBitBuffer_Length))
			{
				throw new System.ArgumentException(BitStream.BitStreamResources.GetString("Argument_DifferentBitStreamLengths"));
			}
			BitStream bitStream = new BitStream((long)((ulong)this._uiBitBuffer_Length));
			uint num = this._uiBitBuffer_Length >> 5;
			uint num2;
			for (num2 = 0u; num2 < num; num2 += 1u)
			{
				bitStream._auiBitBuffer[(int)((System.UIntPtr)num2)] = (this._auiBitBuffer[(int)((System.UIntPtr)num2)] ^ bits._auiBitBuffer[(int)((System.UIntPtr)num2)]);
			}
			if ((this._uiBitBuffer_Length & 31u) > 0u)
			{
				uint num3 = 4294967295u << (int)(32u - (this._uiBitBuffer_Length & 31u));
				bitStream._auiBitBuffer[(int)((System.UIntPtr)num2)] = (this._auiBitBuffer[(int)((System.UIntPtr)num2)] ^ (bits._auiBitBuffer[(int)((System.UIntPtr)num2)] & num3));
			}
			return bitStream;
		}
		public virtual BitStream Not()
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			BitStream bitStream = new BitStream((long)((ulong)this._uiBitBuffer_Length));
			uint num = this._uiBitBuffer_Length >> 5;
			uint num2;
			for (num2 = 0u; num2 < num; num2 += 1u)
			{
				bitStream._auiBitBuffer[(int)((System.UIntPtr)num2)] = ~this._auiBitBuffer[(int)((System.UIntPtr)num2)];
			}
			if ((this._uiBitBuffer_Length & 31u) > 0u)
			{
				uint num3 = 4294967295u << (int)(32u - (this._uiBitBuffer_Length & 31u));
				bitStream._auiBitBuffer[(int)((System.UIntPtr)num2)] = (~this._auiBitBuffer[(int)((System.UIntPtr)num2)] & num3);
			}
			return bitStream;
		}
		public virtual BitStream ShiftLeft(long count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			BitStream bitStream = this.Copy();
			uint num = (uint)count;
			uint num2 = (uint)bitStream.Length;
			if (num >= num2)
			{
				bitStream.Position = 0L;
				for (uint num3 = 0u; num3 < num2; num3 += 1u)
				{
					bitStream.Write(false);
				}
			}
			else
			{
				bool bit = false;
				for (uint num4 = 0u; num4 < num2 - num; num4 += 1u)
				{
					bitStream.Position = (long)((ulong)(num + num4));
					bitStream.Read(out bit);
					bitStream.Position = (long)((ulong)num4);
					bitStream.Write(bit);
				}
				for (uint num5 = num2 - num; num5 < num2; num5 += 1u)
				{
					bitStream.Write(false);
				}
			}
			bitStream.Position = 0L;
			return bitStream;
		}
		public virtual BitStream ShiftRight(long count)
		{
			if (!this._blnIsOpen)
			{
				throw new System.ObjectDisposedException(BitStream.BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
			}
			BitStream bitStream = this.Copy();
			uint num = (uint)count;
			uint num2 = (uint)bitStream.Length;
			if (num >= num2)
			{
				bitStream.Position = 0L;
				for (uint num3 = 0u; num3 < num2; num3 += 1u)
				{
					bitStream.Write(false);
				}
			}
			else
			{
				bool bit = false;
				for (uint num4 = 0u; num4 < num2 - num; num4 += 1u)
				{
					bitStream.Position = (long)((ulong)num4);
					bitStream.Read(out bit);
					bitStream.Position = (long)((ulong)(num4 + num));
					bitStream.Write(bit);
				}
				bitStream.Position = 0L;
				for (uint num5 = 0u; num5 < num; num5 += 1u)
				{
					bitStream.Write(false);
				}
			}
			bitStream.Position = 0L;
			return bitStream;
		}
		public override string ToString()
		{
			uint num = this._uiBitBuffer_Length >> 5;
			uint num2 = 1u;
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder((int)this._uiBitBuffer_Length);
			uint num3;
			for (num3 = 0u; num3 < num; num3 += 1u)
			{
				stringBuilder.Append("[" + num3.ToString(BitStream._ifp) + "]:{");
				for (int i = 31; i >= 0; i--)
				{
					uint num4 = num2 << i;
					if ((this._auiBitBuffer[(int)((System.UIntPtr)num3)] & num4) == num4)
					{
						stringBuilder.Append('1');
					}
					else
					{
						stringBuilder.Append('0');
					}
				}
				stringBuilder.Append("}\r\n");
			}
			if ((this._uiBitBuffer_Length & 31u) > 0u)
			{
				stringBuilder.Append("[" + num3.ToString(BitStream._ifp) + "]:{");
				int num5 = (int)(32u - (this._uiBitBuffer_Length & 31u));
				for (int i = 31; i >= num5; i--)
				{
					uint num6 = num2 << i;
					if ((this._auiBitBuffer[(int)((System.UIntPtr)num3)] & num6) == num6)
					{
						stringBuilder.Append('1');
					}
					else
					{
						stringBuilder.Append('0');
					}
				}
				for (int i = num5 - 1; i >= 0; i--)
				{
					stringBuilder.Append('.');
				}
				stringBuilder.Append("}\r\n");
			}
			return stringBuilder.ToString();
		}
		public static string ToString(bool bit)
		{
			return "Boolean{" + (bit ? 1 : 0) + "}";
		}
		public static string ToString(byte bits)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(8);
			uint num = 1u;
			stringBuilder.Append("Byte{");
			for (int i = 7; i >= 0; i--)
			{
				uint num2 = num << i;
				if (((uint)bits & num2) == num2)
				{
					stringBuilder.Append('1');
				}
				else
				{
					stringBuilder.Append('0');
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		public static string ToString(sbyte bits)
		{
			byte b = (byte)bits;
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(8);
			uint num = 1u;
			stringBuilder.Append("SByte{");
			for (int i = 7; i >= 0; i--)
			{
				uint num2 = num << i;
				if (((uint)b & num2) == num2)
				{
					stringBuilder.Append('1');
				}
				else
				{
					stringBuilder.Append('0');
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		public static string ToString(char bits)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(16);
			uint num = 1u;
			stringBuilder.Append("Char{");
			for (int i = 15; i >= 0; i--)
			{
				uint num2 = num << i;
				if (((uint)bits & num2) == num2)
				{
					stringBuilder.Append('1');
				}
				else
				{
					stringBuilder.Append('0');
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		public static string ToString(ushort bits)
		{
			short num = (short)bits;
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(16);
			uint num2 = 1u;
			stringBuilder.Append("UInt16{");
			for (int i = 15; i >= 0; i--)
			{
				uint num3 = num2 << i;
				if (((long)num & (long)((ulong)num3)) == (long)((ulong)num3))
				{
					stringBuilder.Append('1');
				}
				else
				{
					stringBuilder.Append('0');
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		public static string ToString(short bits)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(16);
			uint num = 1u;
			stringBuilder.Append("Int16{");
			for (int i = 15; i >= 0; i--)
			{
				uint num2 = num << i;
				if (((long)bits & (long)((ulong)num2)) == (long)((ulong)num2))
				{
					stringBuilder.Append('1');
				}
				else
				{
					stringBuilder.Append('0');
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		public static string ToString(uint bits)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(32);
			uint num = 1u;
			stringBuilder.Append("UInt32{");
			for (int i = 31; i >= 0; i--)
			{
				uint num2 = num << i;
				if ((bits & num2) == num2)
				{
					stringBuilder.Append('1');
				}
				else
				{
					stringBuilder.Append('0');
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		public static string ToString(int bits)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(32);
			uint num = 1u;
			stringBuilder.Append("Int32{");
			for (int i = 31; i >= 0; i--)
			{
				uint num2 = num << i;
				if ((bits & (int)num2) == (int)num2)
				{
					stringBuilder.Append('1');
				}
				else
				{
					stringBuilder.Append('0');
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		public static string ToString(ulong bits)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(64);
			ulong num = 1uL;
			stringBuilder.Append("UInt64{");
			for (int i = 63; i >= 0; i--)
			{
				ulong num2 = num << i;
				if ((bits & num2) == num2)
				{
					stringBuilder.Append('1');
				}
				else
				{
					stringBuilder.Append('0');
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		public static string ToString(long bits)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(64);
			ulong num = 1uL;
			stringBuilder.Append("Int64{");
			for (int i = 63; i >= 0; i--)
			{
				ulong num2 = num << i;
				if ((bits & (long)num2) == (long)num2)
				{
					stringBuilder.Append('1');
				}
				else
				{
					stringBuilder.Append('0');
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		public static string ToString(float bits)
		{
			byte[] bytes = System.BitConverter.GetBytes(bits);
			uint num = (uint)((int)bytes[0] | (int)bytes[1] << 8 | (int)bytes[2] << 16 | (int)bytes[3] << 24);
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(32);
			uint num2 = 1u;
			stringBuilder.Append("Single{");
			for (int i = 31; i >= 0; i--)
			{
				uint num3 = num2 << i;
				if ((num & num3) == num3)
				{
					stringBuilder.Append('1');
				}
				else
				{
					stringBuilder.Append('0');
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		public static string ToString(double bits)
		{
			byte[] bytes = System.BitConverter.GetBytes(bits);
			ulong num = (ulong)bytes[0] | (ulong)bytes[1] << 8 | (ulong)bytes[2] << 16 | (ulong)bytes[3] << 24 | (ulong)bytes[4] << 32 | (ulong)bytes[5] << 40 | (ulong)bytes[6] << 48 | (ulong)bytes[7] << 56;
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(64);
			ulong num2 = 1uL;
			stringBuilder.Append("Double{");
			for (int i = 63; i >= 0; i--)
			{
				ulong num3 = num2 << i;
				if ((num & num3) == num3)
				{
					stringBuilder.Append('1');
				}
				else
				{
					stringBuilder.Append('0');
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		private void UpdateLengthForWrite(uint bits)
		{
			this._uiBitBuffer_Length += bits;
		}
		private void UpdateIndicesForWrite(uint bits)
		{
			this._uiBitBuffer_BitIndex += bits;
			if (this._uiBitBuffer_BitIndex == 32u)
			{
				this._uiBitBuffer_Index += 1u;
				this._uiBitBuffer_BitIndex = 0u;
				if ((long)this._auiBitBuffer.Length == (long)((ulong)(this._uiBitBuffer_Length >> 5)))
				{
					this._auiBitBuffer = BitStream.ReDimPreserve(this._auiBitBuffer, (uint)((uint)this._auiBitBuffer.Length << 1));
					return;
				}
			}
			else if (this._uiBitBuffer_BitIndex > 32u)
			{
				throw new System.InvalidOperationException(BitStream.BitStreamResources.GetString("InvalidOperation_BitIndexGreaterThan32"));
			}
		}
		private void UpdateIndicesForRead(uint bits)
		{
			this._uiBitBuffer_BitIndex += bits;
			if (this._uiBitBuffer_BitIndex == 32u)
			{
				this._uiBitBuffer_Index += 1u;
				this._uiBitBuffer_BitIndex = 0u;
				return;
			}
			if (this._uiBitBuffer_BitIndex > 32u)
			{
				throw new System.InvalidOperationException(BitStream.BitStreamResources.GetString("InvalidOperation_BitIndexGreaterThan32"));
			}
		}
		private static uint[] ReDimPreserve(uint[] buffer, uint newLength)
		{
			uint[] array = new uint[newLength];
			uint num = (uint)buffer.Length;
			if (num < newLength)
			{
				System.Buffer.BlockCopy(buffer, 0, array, 0, (int)((int)num << 2));
			}
			else
			{
				System.Buffer.BlockCopy(buffer, 0, array, 0, (int)((int)newLength << 2));
			}
			buffer = null;
			return array;
		}
		public override void Close()
		{
			this._blnIsOpen = false;
			this._uiBitBuffer_Index = 0u;
			this._uiBitBuffer_BitIndex = 0u;
		}
		public virtual uint[] GetBuffer()
		{
			return this._auiBitBuffer;
		}
		public virtual BitStream Copy()
		{
			BitStream bitStream = new BitStream(this.Length);
			System.Buffer.BlockCopy(this._auiBitBuffer, 0, bitStream._auiBitBuffer, 0, bitStream._auiBitBuffer.Length << 2);
			bitStream._uiBitBuffer_Length = this._uiBitBuffer_Length;
			return bitStream;
		}
		public override System.IAsyncResult BeginRead(byte[] buffer, int offset, int count, System.AsyncCallback callback, object state)
		{
			throw new System.NotSupportedException(BitStream.BitStreamResources.GetString("NotSupported_AsyncOps"));
		}
		public override System.IAsyncResult BeginWrite(byte[] buffer, int offset, int count, System.AsyncCallback callback, object state)
		{
			throw new System.NotSupportedException(BitStream.BitStreamResources.GetString("NotSupported_AsyncOps"));
		}
		public override int EndRead(System.IAsyncResult asyncResult)
		{
			throw new System.NotSupportedException(BitStream.BitStreamResources.GetString("NotSupported_AsyncOps"));
		}
		public override void EndWrite(System.IAsyncResult asyncResult)
		{
			throw new System.NotSupportedException(BitStream.BitStreamResources.GetString("NotSupported_AsyncOps"));
		}
		public override long Seek(long offset, System.IO.SeekOrigin origin)
		{
			throw new System.NotSupportedException(BitStream.BitStreamResources.GetString("NotSupported_Seek"));
		}
		public override void SetLength(long value)
		{
			throw new System.NotSupportedException(BitStream.BitStreamResources.GetString("NotSupported_SetLength"));
		}
		public override void Flush()
		{
			throw new System.NotSupportedException(BitStream.BitStreamResources.GetString("NotSupported_Flush"));
		}
		public static implicit operator BitStream(System.IO.MemoryStream bits)
		{
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_MemoryStream"));
			}
			return new BitStream(bits);
		}
		public static implicit operator System.IO.MemoryStream(BitStream bits)
		{
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitStream"));
			}
			return new System.IO.MemoryStream(bits.ToByteArray());
		}
		public static implicit operator BitStream(System.IO.FileStream bits)
		{
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_FileStream"));
			}
			return new BitStream(bits);
		}
		public static implicit operator BitStream(System.IO.BufferedStream bits)
		{
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BufferedStream"));
			}
			return new BitStream(bits);
		}
		public static implicit operator System.IO.BufferedStream(BitStream bits)
		{
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_BitStream"));
			}
			return new System.IO.BufferedStream(bits);
		}
		public static implicit operator BitStream(NetworkStream bits)
		{
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_NetworkStream"));
			}
			return new BitStream(bits);
		}
		public static implicit operator BitStream(System.Security.Cryptography.CryptoStream bits)
		{
			if (bits == null)
			{
				throw new System.ArgumentNullException("bits", BitStream.BitStreamResources.GetString("ArgumentNull_CryptoStream"));
			}
			return new BitStream(bits);
		}
	}
}
