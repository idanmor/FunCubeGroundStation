using System;
namespace GroundStation
{
	public class NullTelemetryValue : TelemetryValue
	{
		public override string AsString
		{
			get
			{
				return "N/A";
			}
		}
		public NullTelemetryValue() : base(float.NaN)
		{
		}
	}
}
