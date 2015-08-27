using System;
namespace GroundStation
{
	public class SunSensorTelemetryValue : TelemetryValue
	{
		private struct SunsensorCalibrationValue
		{
			public int adcvalue;
			public double luxvalue;
			public SunsensorCalibrationValue(int adc, double lux)
			{
				this.adcvalue = adc;
				this.luxvalue = lux;
			}
		}
		private double luxvalue_measured;
		public override double AsDouble
		{
			get
			{
				return this.luxvalue_measured;
			}
		}
		public override string AsString
		{
			get
			{
				return this.luxvalue_measured.ToString("F2");
			}
		}
		public SunSensorTelemetryValue(object raw) : base(raw)
		{
			SunSensorTelemetryValue.SunsensorCalibrationValue[] array = new SunSensorTelemetryValue.SunsensorCalibrationValue[23];
			int num = (int)System.Convert.ToUInt16(raw);
			array[0] = new SunSensorTelemetryValue.SunsensorCalibrationValue(0, 0.0);
			array[1] = new SunSensorTelemetryValue.SunsensorCalibrationValue(9, 2.158);
			array[2] = new SunSensorTelemetryValue.SunsensorCalibrationValue(19, 2.477);
			array[3] = new SunSensorTelemetryValue.SunsensorCalibrationValue(23, 2.64);
			array[4] = new SunSensorTelemetryValue.SunsensorCalibrationValue(33, 2.8);
			array[5] = new SunSensorTelemetryValue.SunsensorCalibrationValue(50, 2.881);
			array[6] = new SunSensorTelemetryValue.SunsensorCalibrationValue(56, 2.889);
			array[7] = new SunSensorTelemetryValue.SunsensorCalibrationValue(100, 3.04);
			array[8] = new SunSensorTelemetryValue.SunsensorCalibrationValue(133, 3.14);
			array[9] = new SunSensorTelemetryValue.SunsensorCalibrationValue(200, 3.25);
			array[10] = new SunSensorTelemetryValue.SunsensorCalibrationValue(265, 3.346);
			array[11] = new SunSensorTelemetryValue.SunsensorCalibrationValue(333, 3.475);
			array[12] = new SunSensorTelemetryValue.SunsensorCalibrationValue(400, 3.5825);
			array[13] = new SunSensorTelemetryValue.SunsensorCalibrationValue(467, 3.69);
			array[14] = new SunSensorTelemetryValue.SunsensorCalibrationValue(533, 3.81);
			array[15] = new SunSensorTelemetryValue.SunsensorCalibrationValue(600, 3.92);
			array[16] = new SunSensorTelemetryValue.SunsensorCalibrationValue(666, 4.03);
			array[17] = new SunSensorTelemetryValue.SunsensorCalibrationValue(700, 4.079);
			array[18] = new SunSensorTelemetryValue.SunsensorCalibrationValue(732, 4.13);
			array[19] = new SunSensorTelemetryValue.SunsensorCalibrationValue(800, 4.245);
			array[20] = new SunSensorTelemetryValue.SunsensorCalibrationValue(866, 4.325);
			array[21] = new SunSensorTelemetryValue.SunsensorCalibrationValue(933, 4.38);
			array[22] = new SunSensorTelemetryValue.SunsensorCalibrationValue(984, 4.42);
			for (int i = 0; i < array.Length - 1; i++)
			{
				if (num > array[i].adcvalue && num <= array[i + 1].adcvalue)
				{
					double num2 = (double)(array[i + 1].adcvalue - array[i].adcvalue);
					double num3 = array[i + 1].luxvalue - array[i].luxvalue;
					this.luxvalue_measured = array[i].luxvalue + (double)(num - array[i].adcvalue) * (num3 / num2);
					return;
				}
				if (num > array[array.Length - 1].adcvalue)
				{
					this.luxvalue_measured = array[array.Length - 1].luxvalue;
					return;
				}
			}
		}
	}
}
