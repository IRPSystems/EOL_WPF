using System;
using System.Collections.Generic;
using System.IO;
using FlashingToolLib.FlashingTools;
using FlashingToolLib.FlashingTools.UDS;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static iso15765.CUdsClient;

namespace EOL.Models.Config
{

    public class Section
    {
        public string SubfolderName { get; set; } = "";
        public string FileExtension { get; set; } = "";
        public bool IsUsed { get; set; }
    }

    public class OtherPreferences
    {
        public int ProjectBaudRate { get; set; } = 500;
        public bool MonitorLog { get; set; } = true;
        public bool PrintLabel { get; set; } = false;
    }

    public class FlashArguments
    {
        public ECustomer Customer { get; set; }
    }

    public class DevicesList
    {
        public bool MCU { get; set; }
		public bool MCU_2 { get; set; }
        public bool ZimmerPowerMeter { get; set; }
        public bool NI_6002 { get; set; }
        public bool NI_6002_2 { get; set; }
        public bool Printer_TSC { get; set; }
        public bool NumatoGPIO { get; set; }
		public bool PowerSupplyEA { get; set; }

		public DevicesList()
        {
            MCU = true;
			MCU_2 = true;
			ZimmerPowerMeter = true;
			NI_6002 = true;
			NI_6002_2 = true;
			Printer_TSC = true;
			NumatoGPIO = true;
			PowerSupplyEA = true;

		}
    }

    public class FlashPreferences
    {
        public bool PreRunFlash { get; set; } = true;
        public bool IsTogglePower { get; set; } = false;
        public bool AteBoxFlashPower { get; set; } = false;
        public bool PsFlashPower { get; set; } = false;
        public bool RelayFlashPower { get; set; } = false;
        public FlashArguments FirstFileArguments { get; set; } = new FlashArguments();
        public FlashArguments SecondFileArguments { get; set; } = new FlashArguments();
    }

    public class Config
    {
        public Section MainSeq { get; set; }
        public Section ProjectSeq { get; set; }
        public Section MonitorLog { get; set; }
        public Section FirstFlashFile { get; set; }
        public Section SecondFlashFile { get; set; }
        public OtherPreferences OtherPreferences { get; set; }
        public FlashPreferences FlashPreferences { get; set; }
        public DevicesList DevicesList { get; set; }

        public Config()
        {
            MainSeq = new Section();
            ProjectSeq = new Section();
            MonitorLog = new Section();
            FirstFlashFile = new Section();
            SecondFlashFile = new Section();
            OtherPreferences = new OtherPreferences();
            FlashPreferences = new FlashPreferences();
            DevicesList = new DevicesList();

        }
    }


    public class PackageConfig
    {
        public Dictionary<string, Config> Configurations { get; set; } = new Dictionary<string, Config>();

        public Config GetConfiguration(string configType)
        {
            if (Configurations.ContainsKey(configType))
            {
                return Configurations[configType];
            }
            throw new Exception($"Configuration '{configType}' not found");
        }
    }

    public class PackageJsonFileGenerator
    {
        public static void GenerateJsonFile()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PackageConfig.json");
            // Create a sample PackageConfig with one configuration for demonstration purposes.
            PackageConfig packageConfig = new PackageConfig();
            Config sampleConfig = new Config();
            packageConfig.Configurations.Add("SampleConfig", sampleConfig);

            // Serialize the PackageConfig object to JSON.
            string json = JsonConvert.SerializeObject(packageConfig, Formatting.Indented);

            // Write the JSON to a file.
            File.WriteAllText(filePath, json);

            Console.WriteLine($"JSON file generated at: {filePath}");
        }
    }
}