
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
        public string LastComPortATE { get; set; }
        public string LastComPortNumato { get; set; }
        public string LastComPortRS232 { get; set; }
        public string LastComPortPCAN { get; set; }
        public string LastComPortPowerMet { get; set; }
        public string LastComPortPowrSup { get; set; }
        public string LastComPortDaq1 { get; set; }
        public string LastComPortDaq2 { get; set; }
        public string LastComPortUsbRelay { get; set; }
        public string LastComPortPrinter { get; set; }
        public string PcanBuadrate { get; set; }
        public string ATEBoxBuadrate { get; set; }
        public string EOLRackSN { get; set; } = "";
        public string OperatorName { get; set; }
        public string SerialNumber { get; set; }
        public string PartNumber { get; set; }
        public bool UseDefaultFirstFlashFile { get; set; }
        public bool UseDefaultSecondFlashFile { get; set; }
        public bool UseDefaultMainSeqConfigFile { get; set; }
        public bool UseDefaultSubscriptFile { get; set; }
        public bool UseDefaultMonitorLogFile { get; set; }
        public bool isConATEBox { get; set; }
        public bool isConNumato { get; set; }
        public bool isConPM { get; set; }
        public bool isConPS { get; set; }
        public bool isConPCAN { get; set; }
        public bool isConRS232 { get; set; }
        public bool isConDaq1 { get; set; }
        public bool isConDaq2 { get; set; }
        public bool isConUsbRelay { get; set; }
        public bool isConPrinter { get; set; }
        public bool FlashPowerATEBox { get; set; }
        public bool FlashPowerEA_PS { get; set; }
        public bool FlashPowerUsbRelay { get; set; }
        public bool isRecordMonitor { get; set; } = true;
        public bool preTestFlash { get; set; }
        public bool isStandAloneFlasher { get; set; }
        public bool isDecryptFile { get; set; }
        public bool isTogglePower { get; set; }
        public bool isSofIgnore { get; set; }
        public bool isSofSkipPS { get; set; }
        public bool isAdminGBVisible { get; set; }
        public bool isLoopTest { get; set; }
        public bool isPrintLabel { get; set; }
        public bool isAdmin { get; set; }
        public int AmountOfCycles { get; set; } = 5;
        public int UIPref { get; set; } = 0;
        public int ProjectBaudRate { get; set; } = 500;
        public UInt32 FirstFileUdsRx { get; set; }
        public UInt32 FirstFileUdsTx { get; set; }
        public UdsSequence FirstFileUdsSequence { get; set; }
        public UInt32 SecondFileUdsRx { get; set; }
        public UInt32 SecondFileUdsTx { get; set; }
        public UdsSequence SecondFileUdsSequence { get; set; }
        public string AutoConfigPref { get; set; }
    }
}
