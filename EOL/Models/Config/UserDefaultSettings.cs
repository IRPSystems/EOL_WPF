using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOL_Tester.Classes
{
    static class UserDefaultSettings
    {
        // Declare variables corresponding to the custom user.config settings
        public static string ReportsSavingPath { get; set; }
        public static string TechLogDirectory { get; set; }
        public static string MonitorLogSavingPath { get; set; }
        public static string DefaultMainSeqConfigFile { get; set; }
        public static string DefaultSubscriptFile { get; set; }
        public static string FirstFlashFilePath { get; set; }
        public static string SecondFlashFilePath { get; set; }
        public static string DefaultMonitorLogFile { get; set; }
        public static string DefaultPowerMeterPort { get; set; }
        public static string LastComPortATE { get; set; }
        public static string LastComPortNumato { get; set; }
        public static string LastComPortRS232 { get; set; }
        public static string LastComPortPCAN { get; set; }
        public static string LastComPortPowerMet { get; set; }
        public static string LastComPortPowrSup { get; set; }
        public static string LastComPortDaq1 { get; set; }
        public static string LastComPortDaq2 { get; set; }
        public static string LastComPortUsbRelay { get; set; }
        public static string LastComPortPrinter { get; set; }
        public static string PcanBuadrate { get; set; }
        public static string ATEBoxBuadrate { get; set; }
        public static string EOLRackSN { get; set; } = "";
        public static string OperatorName { get; set; }
        public static string SerialNumber { get; set; }
        public static string PartNumber { get; set; }
        public static bool UseDefaultFirstFlashFile { get; set; }
        public static bool UseDefaultSecondFlashFile { get; set; }
        public static bool UseDefaultMainSeqConfigFile { get; set; }
        public static bool UseDefaultSubscriptFile { get; set; }
        public static bool UseDefaultMonitorLogFile { get; set; }
        public static bool isConATEBox { get; set; }
        public static bool isConNumato { get; set; }
        public static bool isConPM { get; set; }
        public static bool isConPS { get; set; }
        public static bool isConPCAN { get; set; }
        public static bool isConRS232 { get; set; }
        public static bool isConDaq1 { get; set; }
        public static bool isConDaq2 { get; set; }
        public static bool isConUsbRelay { get; set; }
        public static bool isConPrinter { get; set; }
        public static bool FlashPowerATEBox { get; set; }
        public static bool FlashPowerEA_PS { get; set; }
        public static bool FlashPowerUsbRelay { get; set; }
        public static bool isUseMonitorLog { get; set; } = true;
        public static bool preTestFlash { get; set; }
        public static bool isStandAloneFlasher { get; set; }
        public static bool isDecryptFile { get; set; }
        public static bool isTogglePower { get; set; }
        public static bool isSofIgnore { get; set; }
        public static bool isSofSkipPS { get; set; }
        public static bool isAdminGBVisible { get; set; }
        public static bool isLoopTest { get; set; }
        public static bool isPrintLabel { get; set; }
        public static bool isAdmin { get; set; }
        public static int AmountOfCycles { get; set; } = 5;
        public static int UIPref { get; set; } = 0;
        public static string AutoConfigPref { get; set; }
    }
}
