using BKSystem.IO;
using System;
namespace GroundStation
{
	public abstract class TelemetryValue
	{
		protected object raw;
		public virtual object AsRaw
		{
			get
			{
				return this.raw;
			}
		}
		public virtual int AsInt
		{
			get
			{
				int result;
				try
				{
					result = System.Convert.ToInt32(this.raw);
				}
				catch (System.OverflowException)
				{
					result = 0;
				}
				return result;
			}
		}
		public virtual uint AsUInt
		{
			get
			{
				uint result;
				try
				{
					result = System.Convert.ToUInt32(this.raw);
				}
				catch (System.OverflowException)
				{
					result = 0u;
				}
				return result;
			}
		}
		public virtual double AsDouble
		{
			get
			{
				return System.Convert.ToDouble(this.raw);
			}
		}
		public virtual bool AsBool
		{
			get
			{
				bool result;
				try
				{
					result = System.Convert.ToBoolean(this.raw);
				}
				catch (System.OverflowException)
				{
					result = false;
				}
				return result;
			}
		}
		public virtual string AsString
		{
			get
			{
				return this.raw.ToString();
			}
		}
		public virtual object AsRawString
		{
			get
			{
				return this.raw.ToString();
			}
		}
		public virtual BitStream AsBitStream
		{
			get
			{
				return new BitStream();
			}
		}
		public TelemetryValue(object raw)
		{
			this.raw = raw;
		}
	}
}
