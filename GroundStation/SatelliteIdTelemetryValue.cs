using System;
namespace GroundStation
{
	public class SatelliteIdTelemetryValue : TelemetryValue
	{
		public override string AsString
		{
			get
			{
				switch (this.AsInt)
				{
				case 0:
					return "FC1 - EM";
				case 1:
					return "FUNcube-2 on UKube";
				case 2:
					return "FUNcube-1";
				default:
					return "Unknown Satellite";
				}
			}
		}
		public SatelliteIdTelemetryValue(object raw) : base(raw)
		{
		}
	}
}
