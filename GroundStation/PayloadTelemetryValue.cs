using BKSystem.IO;
using System;
using System.Text;
namespace GroundStation
{
	public class PayloadTelemetryValue : TelemetryValue
	{
		public override BitStream AsBitStream
		{
			get
			{
				return this.raw as BitStream;
			}
		}
		public override string AsString
		{
			get
			{
				BitStream bitStream = this.raw as BitStream;
				System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
				int num = 0;
				while ((long)num < bitStream.Length8)
				{
					stringBuilder.AppendFormat("{0:X2}", bitStream.ReadByte());
					num++;
				}
				return stringBuilder.ToString();
			}
		}
		public byte[] AsBytes
		{
			get
			{
				BitStream bitStream = this.raw as BitStream;
				return bitStream.ToByteArray();
			}
		}
		public PayloadTelemetryValue(BitStream bs) : base(bs)
		{
		}
	}
}
