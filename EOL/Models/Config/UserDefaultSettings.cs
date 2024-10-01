
using FlashingToolLib.FlashingTools;
using System;
using System.Diagnostics.Contracts;

namespace EOL.Models
{
    public class UserDefaultSettings
    {
        // Declare variables corresponding to the custom user.config settings
        public string ReportsSavingPath { get; set; }
        public string TechLogDirectory { get; set; }
        public string MonitorLogSavingPath { get; set; }
        public string DefaultMainSeqConfigFile { get; set; }
        public string DefaultSubScriptFile { get; set; }
        public string DefaultAbortScriptFile { get; set; }
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
        public bool isTogglePower {  get; set; }
        public int ProjectBaudRate { get; set; } = 500;
        public UInt32 FirstFileUdsRx { get; set; }
        public UInt32 FirstFileUdsTx { get; set; }
        public UdsSequence FirstFileUdsSequence { get; set; }
        public UInt32 SecondFileUdsRx { get; set; }
        public UInt32 SecondFileUdsTx { get; set; }
        public UdsSequence SecondFileUdsSequence { get; set; }
        public string AutoConfigPref { get; set; }



		public bool MCU { get; set; }
		public bool MCU_B2B { get; set; }
		public bool ZimmerPowerMeter { get; set; }
		public bool NI_6002 { get; set; }
		public bool NI_6002_2 { get; set; }
		public bool Printer_TSC { get; set; }
		public bool NumatoGPIO { get; set; }
	}
}
