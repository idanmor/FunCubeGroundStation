using System;
namespace GroundStation
{
	public class FrameIdTelemetryValue : TelemetryValue
	{
		public override string AsString
		{
			get
			{
				string result;
				if (this.AsInt < 24)
				{
					Telemetry.FrameEnum asInt = (Telemetry.FrameEnum)this.AsInt;
					result = string.Format("{0} (RT+{1})", this.AsInt, asInt);
				}
				else
				{
					result = string.Format("{0} (DBG)", this.AsInt);
				}
				return result;
			}
		}
		public FrameIdTelemetryValue(object raw) : base(raw)
		{
		}
	}
}
