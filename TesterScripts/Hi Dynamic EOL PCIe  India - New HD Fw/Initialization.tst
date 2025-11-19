{
  "$type": "ScriptHandler.Models.TestData, ScriptHandler",
  "Name": "Initialization",
  "ScriptItemsList": {
    "$type": "System.Collections.ObjectModel.ObservableCollection`1[[ScriptHandler.Interfaces.IScriptItem, ScriptHandler]], System.ObjectModel",
    "$values": [
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSetParameter, ScriptHandler",
        "Parameter": {
          "$type": "DeviceCommunicators.EvvaDevice.Evva_ParamData, DeviceCommunicators",
          "DropDown": {
            "$type": "System.Collections.Generic.List`1[[Entities.Models.DropDownParamData, Entities]], System.Private.CoreLib",
            "$values": [
              {
                "$type": "Entities.Models.DropDownParamData, Entities",
                "Name": "OFF",
                "Value": "0"
              },
              {
                "$type": "Entities.Models.DropDownParamData, Entities",
                "Name": "ON",
                "Value": "1"
              }
            ]
          },
          "Command": "Recording/Monitoring",
          "Name": "Recording/Monitoring on/off",
          "Units": null,
          "Value": null,
          "DeviceType": 11,
          "ToolTip": null,
          "CommunicationTimeout": 0,
          "IsInCANBus": false
        },
        "Value": 1,
        "ValueDropDwonIndex": 1,
        "SwitchRelayValue": null,
        "SwitchRelayChannel": 0,
        "ExtraData": {
          "$type": "ScriptHandler.Models.ExtraDataForParameter, ScriptHandler",
          "Rigol_Channel": 0,
          "Rigol_Slot": 0,
          "Rigol_Range": 0,
          "MX180TP_Channel": 0,
          "Ni6002_ExpectedRPM": 0,
          "Ni6002_IOPort": 0,
          "Ni6002_Line": 0,
          "NI6002_NumofCounts": 0,
          "NIDAQShuntResistor": 0.0,
          "AteCommand": 0,
          "NIThermistorIndex": 0,
          "AteCommandDropDwonIndex": 0,
          "Zimmer_Channel": 0,
          "NumatoGPIOPort": 0,
          "NumatoGPIODropDwonIndex": 0,
          "DBCInterval": 0,
          "DBCIntervalUnite": 1
        },
        "ValueParameter": null,
        "IsWarning": true,
        "IsFault": false,
        "IsCriticalFault": false,
        "SafetyOfficerErrorLevel": 1,
        "UserTitle": "Start Recording Monitor",
        "Name": "Set Parameter 4",
        "IsPass": false,
        "PassNextId": 2,
        "FailNextId": 0,
        "ID": 1,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": true,
          "IsSaveToPdfExecTable": true,
          "IsSaveToPdfDynTable": true,
          "IsSaveToCustomerVer": false,
          "IsSaveToWats": true
        }
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "ParentScriptName": "Initialization",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "ATE Init",
        "UserTitle": "ATE Init",
        "Name": "Sub Script 10",
        "IsPass": false,
        "PassNextId": 3,
        "FailNextId": 0,
        "ID": 2,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": true,
          "IsSaveToPdfExecTable": true,
          "IsSaveToPdfDynTable": true,
          "IsSaveToCustomerVer": false,
          "IsSaveToWats": true
        }
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "ParentScriptName": "Initialization",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Check Ambient Temperature",
        "UserTitle": "Check Ambient Temperature",
        "Name": "Sub Script 2",
        "IsPass": false,
        "PassNextId": 4,
        "FailNextId": 0,
        "ID": 3,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": true,
          "IsSaveToPdfExecTable": true,
          "IsSaveToPdfDynTable": true,
          "IsSaveToCustomerVer": false,
          "IsSaveToWats": true
        }
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "ParentScriptName": "Initialization",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Zero Current Check",
        "UserTitle": "Zero Current Check",
        "Name": "Sub Script 3",
        "IsPass": false,
        "PassNextId": 0,
        "FailNextId": 0,
        "ID": 4,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": true,
          "IsSaveToPdfExecTable": true,
          "IsSaveToPdfDynTable": true,
          "IsSaveToCustomerVer": false,
          "IsSaveToWats": true
        }
      }
    ]
  },
  "IsPass": null
}