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
          "IsSaveToReport": false,
          "IsSaveToPdfExecTable": false,
          "IsSaveToPdfDynTable": false
        }
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "Sub Script 2",
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
        "Name": "Sub Script 1",
        "ParentScriptName": "Position Sensor Test",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Position Sensor",
        "UserTitle": null,
        "IsPass": false,
        "PassNextId": -1,
        "FailNextId": 0,
        "ID": 3,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": false,
          "IsSaveToPdfExecTable": false,
          "IsSaveToPdfDynTable": false
        }
      }
    ]
  },
  "IsPass": null
}