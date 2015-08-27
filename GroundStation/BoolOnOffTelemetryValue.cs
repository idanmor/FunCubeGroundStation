using System;
namespace GroundStation
{
	public class BoolOnOffTelemetryValue : TelemetryValue
	{
		public override string AsString
		{
			get
			{
				if (this.AsBool)
				{
					return "ON";
				}
				return "OFF";
			}
		}
		public override bool AsBool
		{
			get
			{
				return System.Convert.ToInt32(this.raw) > 0;
			}
		}
		public BoolOnOffTelemetryValue(object raw) : base(raw)
		{
		}
	}
}
