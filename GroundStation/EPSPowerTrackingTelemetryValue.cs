using System;
namespace GroundStation
{
	public class EPSPowerTrackingTelemetryValue : TelemetryValue
	{
		public override string AsString
		{
			get
			{
				switch (this.AsUInt)
				{
				case 0u:
					return "HW default";
				case 1u:
					return "MPPT";
				case 2u:
					return "SW fixed";
				default:
					return "Unknown (" + this.AsUInt + ")";
				}
			}
		}
		public EPSPowerTrackingTelemetryValue(object raw) : base(raw)
		{
		}
	}
}
