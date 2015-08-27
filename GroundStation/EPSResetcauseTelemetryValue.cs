using System;
namespace GroundStation
{
	public class EPSResetcauseTelemetryValue : TelemetryValue
	{
		public override string AsString
		{
			get
			{
				switch (this.AsUInt)
				{
				case 0u:
					return "Power On";
				case 1u:
					return "External";
				case 2u:
					return "Brown Out";
				case 3u:
					return "Watchdog";
				case 4u:
					return "JTAG";
				case 5u:
					return "Other";
				default:
					return "Unknown (" + this.AsUInt + ")";
				}
			}
		}
		public EPSResetcauseTelemetryValue(object raw) : base(raw)
		{
		}
	}
}
