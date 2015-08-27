using System;
namespace GroundStation
{
	public class PaCurrentTelemetryValue : MultiplierOffsetTelemetryValue
	{
		public PaCurrentTelemetryValue(object raw) : base(0.5496, 2.5425, raw)
		{
		}
	}
}
