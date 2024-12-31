{
  "$type": "ScriptHandler.Models.TestData, ScriptHandler",
  "Name": "Position Sensor Test",
  "ScriptItemsList": {
    "$type": "System.Collections.ObjectModel.ObservableCollection`1[[ScriptHandler.Interfaces.IScriptItem, ScriptHandler]], System.ObjectModel",
    "$values": [
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "ScriptNodeSubScript 3",
        "ParentScriptName": "Position Sensor Test",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Config Manual FOC",
        "UserTitle": "Configure FOC ",
        "IsPass": false,
        "PassNextId": 2,
        "FailNextId": 0,
        "ID": 1,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": true,
          "IsSaveToPdfExecTable": false,
          "IsSaveToPdfDynTable": false
        }
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSetParameter, ScriptHandler",
        "Name": "Set Parameter 6",
        "Parameter": {
          "$type": "DeviceCommunicators.MCU.MCU_ParamData, DeviceCommunicators",
          "ValueChanged": null,
          "Value": "",
          "GroupName": "Encoder",
          "Description": "Aqb AB Fault Threshold",
          "Default": "",
          "Cmd": "aqbfltcntthr",
          "Range": {
            "$type": "System.Collections.Generic.List`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib",
            "$values": [
              0.0,
              5000.0
            ]
          },
          "Format": null,
          "Scale": 1.0,
          "Note": null,
          "Save": true,
          "DropDown": null,
          "Name": "Aqb AB Fault Threshold",
          "Units": null,
          "DeviceType": 1,
          "CommunicationTimeout": 0
        },
        "Value": 50000.0,
        "ValueDropDwonIndex": -1,
        "SwitchRelayValue": null,
        "SwitchRelayChannel": 0,
        "ExtraData": {
          "$type": "ScriptHandler.Models.ExtraDataForParameter, ScriptHandler",
          "Parameter": {
            "$type": "DeviceCommunicators.MCU.MCU_ParamData, DeviceCommunicators",
            "ValueChanged": null,
            "Value": "",
            "GroupName": "Encoder",
            "Description": "Aqb AB Fault Threshold",
            "Default": "",
            "Cmd": "aqbfltcntthr",
            "Range": {
              "$type": "System.Collections.Generic.List`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib",
              "$values": [
                0.0,
                5000.0
              ]
            },
            "Format": null,
            "Scale": 1.0,
            "Note": null,
            "Save": true,
            "DropDown": null,
            "Name": "Aqb AB Fault Threshold",
            "Units": null,
            "DeviceType": 1,
            "CommunicationTimeout": 0
          },
          "NI6002_ExpectedRPM": 0,
          "Ni6002_IOPort": 0,
          "Ni6002_Line": 0,
          "NI6002_NumofCounts": 0,
          "NIDAQShuntResistor": 0.0,
          "AteCommand": 0,
          "NIThermistorIndex": 0,
          "AteCommandDropDwonIndex": 0,
          "Zimmer_Channel": 0,
          "NumatoGPIOPort": 0,
          "NumatoGPIODropDwonIndex": 0
        },
        "ValueParameter": null,
        "IsWarning": true,
        "IsFault": false,
        "IsCriticalFault": false,
        "SafetyOfficerErrorLevel": 0,
        "UserTitle": "Set AB Fault Count Threshold to 50000",
        "IsPass": false,
        "PassNextId": 3,
        "FailNextId": 0,
        "ID": 2,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": false,
          "IsSaveToPdfExecTable": false,
          "IsSaveToPdfDynTable": false
        }
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "ScriptNodeSubScript 5",
        "ParentScriptName": "Position Sensor Test",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Config Position Sensor",
        "UserTitle": "Config Motor",
        "IsPass": false,
        "PassNextId": 4,
        "FailNextId": 0,
        "ID": 3,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": true,
          "IsSaveToPdfExecTable": false,
          "IsSaveToPdfDynTable": false
        }
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "Sub Script 1",
        "ParentScriptName": "Position Sensor Test",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Speed Estimation",
        "UserTitle": null,
        "IsPass": false,
        "PassNextId": -1,
        "FailNextId": 0,
        "ID": 4,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": true,
          "IsSaveToPdfExecTable": true,
          "IsSaveToPdfDynTable": false
        }
      }
    ]
  },
  "IsPass": null
}