using System;
namespace GroundStation
{
	public class PaPowerTelemetryValue : MultiplierPowerTelemetryValue
	{
		public PaPowerTelemetryValue(object raw) : base(0.005, 2.0629, raw)
		{
		}
	}
}
