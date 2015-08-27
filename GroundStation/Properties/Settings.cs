using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace GroundStation
{
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0"), System.Runtime.CompilerServices.CompilerGenerated]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}
		[DefaultSettingValue("False"), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public bool AudioMonitorEnabled
		{
			get
			{
				return (bool)this["AudioMonitorEnabled"];
			}
			set
			{
				this["AudioMonitorEnabled"] = value;
			}
		}
		[DefaultSettingValue("-1"), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public string AudioInputDevice
		{
			get
			{
				return (string)this["AudioInputDevice"];
			}
			set
			{
				this["AudioInputDevice"] = value;
			}
		}
		[DefaultSettingValue("-1"), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public string AudioOutputDevice
		{
			get
			{
				return (string)this["AudioOutputDevice"];
			}
			set
			{
				this["AudioOutputDevice"] = value;
			}
		}
		[DefaultSettingValue("http://data.funcube.org.uk"), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public string WarehouseUrl
		{
			get
			{
				return (string)this["WarehouseUrl"];
			}
			set
			{
				this["WarehouseUrl"] = value;
			}
		}
		[DefaultSettingValue(""), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public string WarehouseSiteId
		{
			get
			{
				return (string)this["WarehouseSiteId"];
			}
			set
			{
				this["WarehouseSiteId"] = value;
			}
		}
		[DefaultSettingValue(""), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public string WarehouseAuthCode
		{
			get
			{
				return (string)this["WarehouseAuthCode"];
			}
			set
			{
				this["WarehouseAuthCode"] = value;
			}
		}
		[DefaultSettingValue("False"), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public string WarehouseStreamData
		{
			get
			{
				return (string)this["WarehouseStreamData"];
			}
			set
			{
				this["WarehouseStreamData"] = value;
			}
		}
		[DefaultSettingValue("145925000"), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public uint DongleFrequency
		{
			get
			{
				return (uint)this["DongleFrequency"];
			}
			set
			{
				this["DongleFrequency"] = value;
			}
		}
		[DefaultSettingValue("True"), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public bool LoggingEnabled
		{
			get
			{
				return (bool)this["LoggingEnabled"];
			}
			set
			{
				this["LoggingEnabled"] = value;
			}
		}
		[DefaultSettingValue(""), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public string LoggingPath
		{
			get
			{
				return (string)this["LoggingPath"];
			}
			set
			{
				this["LoggingPath"] = value;
			}
		}
		[DefaultSettingValue("500"), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public int RecPlaybackSpeed
		{
			get
			{
				return (int)this["RecPlaybackSpeed"];
			}
			set
			{
				this["RecPlaybackSpeed"] = value;
			}
		}
		[DefaultSettingValue(""), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public string LastRunRevision
		{
			get
			{
				return (string)this["LastRunRevision"];
			}
			set
			{
				this["LastRunRevision"] = value;
			}
		}
		[DefaultSettingValue("False"), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public bool BiasTEnabled
		{
			get
			{
				return (bool)this["BiasTEnabled"];
			}
			set
			{
				this["BiasTEnabled"] = value;
			}
		}
		[DefaultSettingValue("0"), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public double RangeMin
		{
			get
			{
				return (double)this["RangeMin"];
			}
			set
			{
				this["RangeMin"] = value;
			}
		}
		[DefaultSettingValue("48000"), UserScopedSetting, System.Diagnostics.DebuggerNonUserCode]
		public double RangeMax
		{
			get
			{
				return (double)this["RangeMax"];
			}
			set
			{
				this["RangeMax"] = value;
			}
		}
	}
}
