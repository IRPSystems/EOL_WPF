﻿using System;
using System.Collections.Generic;
using FlashingToolLib.FlashingTools;
using Newtonsoft.Json;

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
        public UdsSequence udsSequence { get; set; } = UdsSequence.generic;
    
    }

    public class FlashPreferences
    {
        public bool PreRunFlash { get; set; } = true;
        public bool IsTogglePower { get; set; } = false;
        public bool AteBoxFlashPower { get; set; } = false;
        public bool PsFlashPower { get; set; } = false;
        public bool RelayFlashPower { get; set; } = false;
        public FlashArguments FirstFileArguments { get; set; }
        public FlashArguments SecondFileArguments { get; set; }
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
        public Dictionary<string, Config> Configurations { get; set; }

        public Config GetConfiguration(string configType)
        {
            if (Configurations.ContainsKey(configType))
            {
                return Configurations[configType];
            }
            throw new Exception($"Configuration '{configType}' not found");
        }
    }