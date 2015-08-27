using System;
namespace GroundStation
{
	public class DeviceDataValidTelemetryValue : TelemetryValue
	{
		public override string AsString
		{
			get
			{
				if (this.AsBool)
				{
					return "OK";
				}
				return "Failed";
			}
		}
		public override bool AsBool
		{
			get
			{
				return System.Convert.ToInt32(this.raw) > 0;
			}
		}
		public DeviceDataValidTelemetryValue(object raw) : base(raw)
		{
		}
	}
}
