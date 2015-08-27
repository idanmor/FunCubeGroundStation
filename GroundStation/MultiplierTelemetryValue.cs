using System;
namespace GroundStation
{
	public class MultiplierTelemetryValue : TelemetryValue
	{
		private double multiplier;
		public override double AsDouble
		{
			get
			{
				return base.AsDouble * this.multiplier;
			}
		}
		public override string AsString
		{
			get
			{
				return string.Format("{0:0.00}", this.AsDouble);
			}
		}
		public MultiplierTelemetryValue(double multiplier, object raw) : base(raw)
		{
			this.multiplier = multiplier;
		}
	}
}
