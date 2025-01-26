{
  "$type": "ScriptHandler.Models.TestData, ScriptHandler",
  "Name": "Digital ",
  "ScriptItemsList": {
    "$type": "System.Collections.ObjectModel.ObservableCollection`1[[ScriptHandler.Interfaces.IScriptItem, ScriptHandler]], System.ObjectModel",
    "$values": [
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "Sub Script 1",
        "ParentScriptName": "Digital Inputs",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Digital Input",
        "UserTitle": null,
        "IsPass": false,
        "PassNextId": 2,
        "FailNextId": 0,
        "ID": 1,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": true,
          "IsSaveToPdfExecTable": true,
          "IsSaveToPdfDynTable": false,
          "IsSaveToCustomerVer": false
        }
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "Sub Script 2",
        "ParentScriptName": "Digital ",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Digital Output",
        "UserTitle": null,
        "IsPass": false,
        "PassNextId": 0,
        "FailNextId": 0,
        "ID": 2,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": true,
          "IsSaveToPdfExecTable": true,
          "IsSaveToPdfDynTable": false,
          "IsSaveToCustomerVer": false
        }
      }
    ]
  },
  "IsPass": null
}