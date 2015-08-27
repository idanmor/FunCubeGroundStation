using System;
namespace GroundStation
{
	public class AntsDeployTelemetryValue : TelemetryValue
	{
		public override string AsString
		{
			get
			{
				if (this.AsBool)
				{
					return "Deployed";
				}
				return "Undeployed";
			}
		}
		public override bool AsBool
		{
			get
			{
				return System.Convert.ToInt32(this.raw) > 0;
			}
		}
		public AntsDeployTelemetryValue(object raw) : base(raw)
		{
		}
	}
}
