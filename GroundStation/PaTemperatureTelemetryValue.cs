using System;
namespace GroundStation
{
	public class PaTemperatureTelemetryValue : TelemetryValue
	{
		private static double[] lookupTable = PaTemperatureTelemetryValue.BuildLookupTable();
		public override string AsString
		{
			get
			{
				return string.Format("{0:N1}", PaTemperatureTelemetryValue.lookupTable[(int)((System.UIntPtr)this.AsUInt)]);
			}
		}
		public override double AsDouble
		{
			get
			{
				return PaTemperatureTelemetryValue.lookupTable[(int)((System.UIntPtr)this.AsUInt)];
			}
		}
		public PaTemperatureTelemetryValue(object raw) : base(raw)
		{
		}
		private static double[] BuildLookupTable()
		{
			double[,] array = new double[,]
			{

				{
					87.983,
					-1.7976931348623157E+308
				},

				{
					87.983,
					0.0
				},

				{
					55.3,
					79.0
				},

				{
					49.6,
					91.0
				},

				{
					45.3,
					103.0
				},

				{
					41.1,
					115.0
				},

				{
					37.6,
					125.0
				},

				{
					35.7,
					129.0
				},

				{
					33.6,
					137.0
				},

				{
					30.6,
					145.0
				},

				{
					27.6,
					154.0
				},

				{
					25.1,
					161.0
				},

				{
					22.6,
					169.0
				},

				{
					20.0,
					176.0
				},

				{
					17.6,
					183.0
				},

				{
					15.1,
					189.0
				},

				{
					12.6,
					196.0
				},

				{
					10.0,
					203.0
				},

				{
					7.5,
					208.0
				},

				{
					5.0,
					214.0
				},

				{
					2.4,
					220.0
				},

				{
					0.0,
					224.0
				},

				{
					-2.9,
					230.0
				},

				{
					-5.0,
					233.0
				},

				{
					-7.5,
					237.0
				},

				{
					-10.0,
					241.0
				},

				{
					-12.3,
					244.0
				},

				{
					-15.0,
					247.0
				},

				{
					-20.0,
					252.0
				},

				{
					-22.846,
					255.0
				},

				{
					-22.846,
					1.7976931348623157E+308
				}
			};
			double[] array2 = new double[256];
			for (int i = 0; i < 256; i++)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if ((double)i < array[j, 1])
					{
						double num = array[j, 0];
						double num2 = array[j, 1];
						double num3 = array[j - 1, 1] - num2;
						double num4 = array[j - 1, 0] - num;
						array2[i] = ((double)i - num2) * (num4 / num3) + num;
						break;
					}
				}
			}
			return array2;
		}
	}
}
