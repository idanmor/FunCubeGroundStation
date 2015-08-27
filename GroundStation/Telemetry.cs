using BKSystem.IO;
using GroundStation.Properties;
using System;
using System.Collections.Generic;
namespace GroundStation
{
	public class Telemetry
	{
		public enum FrameEnum
		{
			WO1,
			WO2,
			WO3,
			WO4,
			WO5,
			WO6,
			WO7,
			WO8,
			WO9,
			WO10,
			WO11,
			WO12,
			HR1,
			FM1,
			FM2,
			FM3,
			HR2,
			FM4,
			FM5,
			FM6,
			HR3,
			FM7,
			FM8,
			FM9
		}
		public enum DataIndex
		{
			SatelliteId,
			FrameId,
			PanelVoltX,
			PanelVoltY,
			PanelVoltZ,
			PanelCurrentTotal,
			BattVolt0,
			BattCurrentBus,
			RebootCount,
			EpsErrorCount,
			EpsTemp1,
			EpsTemp2,
			EpsTemp3,
			BattTemp0,
			LatchCount5_0,
			LatchCount3_3,
			ResetCause,
			PptTrackingMode,
			EpsChannelStatus,
			AsibSunSensorX1,
			AsibSunSensorY1,
			AsibSunSensorZ1,
			AsibPanelTempX1,
			AsibPanelTempX2,
			AsibPanelTempY1,
			AsibPanelTempY2,
			AsibBusVolt3_3,
			AsibBusCurrent3_3,
			AsibBusVolt5_0,
			RfReceiverDoppler,
			RfReceiverRSSI,
			RfTemp,
			RfReceiveCurrent,
			RfTransmitCurrent3_3,
			RfTransmitCurrent5_0,
			PaForwardPower,
			PaReversePower,
			PaTemperature,
			PaCurrent,
			AntTempA,
			AntTempB,
			AntDeploy1,
			AntDeploy2,
			AntDeploy3,
			AntDeploy4,
			SequenceNumber,
			DtmfCommandCount,
			DtmfLastCommand,
			DtmfCommandSuccess,
			DataValidAnts1,
			DataValidAnts2,
			DataValidMse,
			DataValidRf,
			DataValidPa,
			DataValidEps,
			DataValidBob,
			InEclipse,
			InSafeMode,
			SoftwareABF,
			HardwareABF,
			DeployWait,
			Payload,
			DecodeErrorCount,
			DecodeFrequency,
			nullvalue
		}
		private static bool logHeaderLine = true;
		private int originalpacketlength;
		private System.Collections.Generic.Dictionary<Telemetry.DataIndex, TelemetryValue> telemetryValues = new System.Collections.Generic.Dictionary<Telemetry.DataIndex, TelemetryValue>();
       
		public int FrameId
		{
			get
			{
				return this.Get(Telemetry.DataIndex.FrameId).AsInt;
			}
		}
		public uint SequenceNumber
		{
			get
			{
				return this.Get(Telemetry.DataIndex.SequenceNumber).AsUInt;
			}
		}
		public BitStream Payload
		{
			get
			{
				return this.Get(Telemetry.DataIndex.Payload).AsBitStream;
			}
		}
		public bool IsWholeOrbit
		{
			get
			{
				return this.Get(Telemetry.DataIndex.FrameId).AsInt >= 0 && this.Get(Telemetry.DataIndex.FrameId).AsInt <= 11;
			}
		}
		public bool IsHighRes
		{
			get
			{
				return this.Get(Telemetry.DataIndex.FrameId).AsInt == 12 || this.Get(Telemetry.DataIndex.FrameId).AsInt == 16 || this.Get(Telemetry.DataIndex.FrameId).AsInt == 20;
			}
		}
		public bool IsFitter
		{
			get
			{
				return this.Get(Telemetry.DataIndex.FrameId).AsInt == 13 || this.Get(Telemetry.DataIndex.FrameId).AsInt == 14 || this.Get(Telemetry.DataIndex.FrameId).AsInt == 15 || this.Get(Telemetry.DataIndex.FrameId).AsInt == 17 || this.Get(Telemetry.DataIndex.FrameId).AsInt == 18 || this.Get(Telemetry.DataIndex.FrameId).AsInt == 19 || this.Get(Telemetry.DataIndex.FrameId).AsInt == 21 || this.Get(Telemetry.DataIndex.FrameId).AsInt == 22 || this.Get(Telemetry.DataIndex.FrameId).AsInt == 23;
			}
		}
		public bool IsDebugFitter
		{
			get
			{
				if (this.IsFitter)
				{
					PayloadTelemetryValue payloadTelemetryValue = this.Get(Telemetry.DataIndex.Payload) as PayloadTelemetryValue;
					return payloadTelemetryValue != null && payloadTelemetryValue.AsBytes[0] == 255;
				}
				return false;
			}
		}
		public Telemetry(byte[] raw)
		{
           
			this.originalpacketlength = raw.Length;
			if (Telemetry.logHeaderLine)
			{
				Console.WriteLine("UTCTime,");
			}
			else if (this.originalpacketlength > 0)
			{
				Console.WriteLine(string.Format("{0} {1},", System.DateTime.UtcNow.ToShortDateString(), System.DateTime.UtcNow.ToLongTimeString()));
			}
			if (raw.Length == 0)
			{
				return;
			}
			BitStream bitStream = new BitStream();
			bitStream.Write(raw);
			bitStream.Position = 0L;
			this.Set(Telemetry.DataIndex.SatelliteId, new SatelliteIdTelemetryValue(this.Get2bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.FrameId, new FrameIdTelemetryValue(this.Get6bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.PanelVoltX, new RawTelemetryValue(this.Get16bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.PanelVoltY, new RawTelemetryValue(this.Get16bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.PanelVoltZ, new RawTelemetryValue(this.Get16bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.PanelCurrentTotal, new RawTelemetryValue(this.Get16bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.BattVolt0, new RawTelemetryValue(this.Get16bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.BattCurrentBus, new RawTelemetryValue(this.Get16bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.RebootCount, new RawTelemetryValue(this.Get16bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.EpsErrorCount, new RawTelemetryValue(this.Get16bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.EpsTemp1, new RawTelemetryValue(this.Get8bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.EpsTemp2, new RawTelemetryValue(this.Get8bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.EpsTemp3, new RawTelemetryValue(this.Get8bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.BattTemp0, new RawTelemetryValue(this.Get8bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.LatchCount5_0, new RawTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.LatchCount3_3, new RawTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.ResetCause, new EPSResetcauseTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.PptTrackingMode, new EPSPowerTrackingTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.AsibSunSensorX1, new SunSensorTelemetryValue(this.Get10bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.AsibSunSensorY1, new SunSensorTelemetryValue(this.Get10bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.AsibSunSensorZ1, new SunSensorTelemetryValue(this.Get10bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.AsibPanelTempX1, new MultiplierOffsetTelemetryValue(-0.2073, 158.239, this.Get10bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.AsibPanelTempX2, new MultiplierOffsetTelemetryValue(-0.2083, 159.227, this.Get10bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.AsibPanelTempY1, new MultiplierOffsetTelemetryValue(-0.2076, 158.656, this.Get10bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.AsibPanelTempY2, new MultiplierOffsetTelemetryValue(-0.2087, 159.045, this.Get10bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.AsibBusVolt3_3, new MultiplierTelemetryValue(4.0, this.Get10bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.AsibBusCurrent3_3, new MultiplierTelemetryValue(1.0, this.Get10bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.AsibBusVolt5_0, new MultiplierTelemetryValue(6.0, this.Get10bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.RfReceiverDoppler, new RawTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.RfReceiverRSSI, new RawTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.RfTemp, new MultiplierOffsetTelemetryValue(-0.857, 193.672, this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.RfReceiveCurrent, new RawTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.RfTransmitCurrent3_3, new RawTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.RfTransmitCurrent5_0, new RawTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.PaReversePower, new PaPowerTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.PaForwardPower, new PaPowerTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.PaTemperature, new PaTemperatureTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.PaCurrent, new PaCurrentTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.AntTempA, new AntsTemperatureTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.AntTempB, new AntsTemperatureTelemetryValue(this.Get8bitsAsUInt(bitStream)));
			this.Set(Telemetry.DataIndex.AntDeploy1, new AntsDeployTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.AntDeploy2, new AntsDeployTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.AntDeploy3, new AntsDeployTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.AntDeploy4, new AntsDeployTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.SequenceNumber, new RawTelemetryValue(this.Get24bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.DtmfCommandCount, new RawTelemetryValue(this.Get6bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.DtmfLastCommand, new RawTelemetryValue(this.Get5bitsAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.DtmfCommandSuccess, new DeviceDataValidTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.DataValidBob, new DeviceDataValidTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.DataValidEps, new DeviceDataValidTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.DataValidPa, new DeviceDataValidTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.DataValidRf, new DeviceDataValidTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.DataValidMse, new DeviceDataValidTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.DataValidAnts2, new DeviceDataValidTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.DataValidAnts1, new DeviceDataValidTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.InEclipse, new RawTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.InSafeMode, new RawTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.HardwareABF, new BoolOnOffTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.SoftwareABF, new SoftwareAbfTelemetryValue(this.Get1bitAsInt(bitStream)));
			this.Set(Telemetry.DataIndex.DeployWait, new BoolOnOffTelemetryValue(this.Get1bitAsInt(bitStream)));
			bitStream.Position = 448L;
			BitStream bitStream2 = new BitStream(1600L);
			for (int i = 0; i < 200; i++)
			{
				bitStream2.WriteByte((byte)bitStream.ReadByte());
			}
			bitStream2.Position = 0L;
			this.Set(Telemetry.DataIndex.Payload, new PayloadTelemetryValue(bitStream2));
            
			if (this.originalpacketlength > 0)
			{
				Console.WriteLine(string.Empty);
			}
             
            
			if (Settings.Default.LoggingEnabled)
			{
				Telemetry.logHeaderLine = false;
			}
		}
		public TelemetryValue Get(string key)
		{
			return this.Get((Telemetry.DataIndex)System.Enum.Parse(typeof(Telemetry.DataIndex), key));
		}
		public TelemetryValue Get(Telemetry.DataIndex key)
		{
			if (this.telemetryValues.ContainsKey(key))
			{
				return this.telemetryValues[key];
			}
			return new NullTelemetryValue();
		}
		public void Set(Telemetry.DataIndex key, TelemetryValue tv)
		{
			if (Telemetry.logHeaderLine)
			{
				Console.WriteLine(string.Format("{0},", key.ToString()));
			}
			else if (this.originalpacketlength > 0)
			{
				Console.WriteLine(string.Format("{0},", tv.AsString));
			}
			this.telemetryValues[key] = tv;
		}
		private int Get1bitAsInt(BitStream bs)
		{
			int result;
			bs.Read(out result, 0, 1);
			return result;
		}
		private int Get2bitsAsInt(BitStream bs)
		{
			int result;
			bs.Read(out result, 0, 2);
			return result;
		}
		private int Get4bitsAsInt(BitStream bs)
		{
			int result;
			bs.Read(out result, 0, 4);
			return result;
		}
		private int Get5bitsAsInt(BitStream bs)
		{
			int result;
			bs.Read(out result, 0, 5);
			return result;
		}
		private int Get6bitsAsInt(BitStream bs)
		{
			int result;
			bs.Read(out result, 0, 6);
			return result;
		}
		private sbyte Get8bitsAsInt(BitStream bs)
		{
			sbyte result;
			bs.Read(out result, 0, 8);
			return result;
		}
		private byte Get8bitsAsUInt(BitStream bs)
		{
			byte result;
			bs.Read(out result, 0, 8);
			return result;
		}
		private int Get10bitsAsInt(BitStream bs)
		{
			int result;
			bs.Read(out result, 0, 10);
			return result;
		}
		private int Get16bitsAsInt(BitStream bs)
		{
			int result;
			bs.Read(out result, 0, 16);
			return result;
		}
		private uint Get16bitsAsUInt(BitStream bs)
		{
			uint result;
			bs.Read(out result, 0, 16);
			return result;
		}
		private int Get24bitsAsInt(BitStream bs)
		{
			int result;
			bs.Read(out result, 0, 24);
			return result;
		}
	}
}
