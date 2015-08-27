using System;
namespace GroundStation
{
	public class MultiplierOffsetTelemetryValue : TelemetryValue
	{
		private double multiplier;
		private double offset;
		public override double AsDouble
		{
			get
			{
				return base.AsDouble * this.multiplier + this.offset;
			}
		}
		public override string AsString
		{
			get
			{
				return string.Format("{0:0.00}", this.AsDouble);
			}
		}
		public MultiplierOffsetTelemetryValue(double multiplier, double offset, object raw) : base(raw)
		{
			this.multiplier = multiplier;
			this.offset = offset;
		}
	}
}
