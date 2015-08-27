using System;
namespace GroundStation
{
	public class MultiplierPowerTelemetryValue : TelemetryValue
	{
		private double multiplier;
		private double power;
		public override double AsDouble
		{
			get
			{
				return this.multiplier * System.Math.Pow(base.AsDouble, this.power);
			}
		}
		public override string AsString
		{
			get
			{
				return string.Format("{0:0.00}", this.AsDouble);
			}
		}
		public MultiplierPowerTelemetryValue(double multiplier, double power, object raw) : base(raw)
		{
			this.multiplier = multiplier;
			this.power = power;
		}
	}
}
