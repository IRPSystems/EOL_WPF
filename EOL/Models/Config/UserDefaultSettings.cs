
using DeviceCommunicators.CANBus;
using Entities.Enums;
using FlashingToolLib.FlashingTools;
using FlashingToolLib.FlashingTools.UDS;
using LibUsbDotNet.DeviceNotify;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using static iso15765.CUdsClient;

namespace EOL.Models
{
    public class CANBusDeviceConfig
    {
        public DeviceTypesEnum DeviceType { get; set; }
        public uint NodeId { get; set; }
    }
    public class UserDefaultSettings
    {
        // Declare variables corresponding to the custom user.config settings
        public string ReportsSavingPath { get; set; }
        public string TechLogDirectory { get; set; }
        public string MonitorLogSavingPath { get; set; }
        public string DefaultMainSeqConfigFile { get; set; }
        public string DefaultSubScriptFile { get; set; }
        public string DefaultAbortScriptFile { get; set; }
        public string DefaultSafetyScriptFile { get; set; }
        public string FirstFlashFilePath { get; set; }
        public string SecondFlashFilePath { get; set; }
        public string DefaultMonitorLogScript { get; set; }
        public string DefaultPowerMeterPort { get; set; }
        public string EOLRackSN { get; set; } = "";
        public string OperatorName { get; set; }
        public string SerialNumber { get; set; }
        public string PartNumber { get; set; }
        public bool UseDefaultFirstFlashFile { get; set; }
        public bool UseDefaultSecondFlashFile { get; set; }
        public bool UseDefaultMainSeqConfigFile { get; set; }
        public bool UseDefaultSubscriptFile { get; set; }
        public bool UseDefaultMonitorLogFile { get; set; }
        public bool FlashPowerATEBox { get; set; }
        public bool FlashPowerEA_PS { get; set; }
        public bool FlashPowerUsbRelay { get; set; }
        public bool isRecordMonitor { get; set; } = true;
        public bool preTestFlash { get; set; }
        public bool isPrintLabel { get; set; }
        public bool isSofIgnore { get; set; }
        public bool isWritingtoWatsEnabled { get; set; }
        public bool isTogglePower {  get; set; }
        public int ProjectBaudRate { get; set; } = 500;
        public string FirstFlashFileCustomerProp { get; set; }
        public string SecondFlashFileCustomerProp { get; set; }
        public string AutoConfigPref { get; set; }
        public bool MCU { get; set; }
		public bool MCU_2 { get; set; }
		public bool ZimmerPowerMeter { get; set; }
		public bool NI_6002 { get; set; }
		public bool NI_6002_2 { get; set; }
		public bool Printer_TSC { get; set; }
		public bool NumatoGPIO { get; set; }
		public bool PowerSupplyEA { get; set; }
        //public List<CANBusDeviceConfig> CANBus { get; set; }
        public ObservableCollection<CANBus_DeviceData> CANBusList { get; set; }

        public string PSoC_Port1 { get; set; }
        public string PSoC_Port2 { get; set; }
        public string WatsTestCode { get; set; }

        public UserDefaultSettings()
		{
			MCU = true;
			MCU_2 = true;
			ZimmerPowerMeter = true;
			NI_6002 = true;
			NI_6002_2 = true;
			Printer_TSC = true;
			NumatoGPIO = true;
            CANBusList = new ObservableCollection<CANBus_DeviceData>();
        }
	}
}
