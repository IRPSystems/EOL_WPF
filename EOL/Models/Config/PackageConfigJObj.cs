using System;
using System.Collections.Generic;
using System.IO;
using FlashingToolLib.FlashingTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        public UInt32 UdsRx { get; set; }
        public UInt32 UdsTx { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public UdsSequence udsSequence { get; set; } = UdsSequence.generic;
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

        public Config()
        {
            MainSeq = new Section();
            ProjectSeq = new Section();
            MonitorLog = new Section();
            FirstFlashFile = new Section();
            SecondFlashFile = new Section();
            OtherPreferences = new OtherPreferences();
            FlashPreferences = new FlashPreferences();
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