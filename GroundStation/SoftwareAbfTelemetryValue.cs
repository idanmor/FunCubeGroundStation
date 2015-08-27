using System;
namespace GroundStation
{
	public class SoftwareAbfTelemetryValue : TelemetryValue
	{
		public override string AsString
		{
			get
			{
				if (this.AsBool)
				{
					return "Enabled";
				}
				return "Disabled";
			}
		}
		public override bool AsBool
		{
			get
			{
				return System.Convert.ToInt32(this.raw) > 0;
			}
		}
		public SoftwareAbfTelemetryValue(object raw) : base(raw)
		{
		}
	}
}
