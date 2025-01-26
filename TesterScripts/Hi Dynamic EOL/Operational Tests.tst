{
  "$type": "ScriptHandler.Models.TestData, ScriptHandler",
  "Name": "Operational Tests",
  "ScriptItemsList": {
    "$type": "System.Collections.ObjectModel.ObservableCollection`1[[ScriptHandler.Interfaces.IScriptItem, ScriptHandler]], System.ObjectModel",
    "$values": [
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "Sub Script 1",
        "ParentScriptName": "Operational Tests",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Min. Voltage Operation",
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
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeDelay, ScriptHandler",
        "Name": "Delay 4",
        "Interval": 5,
        "IntervalUnite": 2,
        "UserTitle": "Delay",
        "IsPass": false,
        "PassNextId": 3,
        "FailNextId": 0,
        "ID": 2,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": false,
          "IsSaveToPdfExecTable": false,
          "IsSaveToPdfDynTable": false,
          "IsSaveToCustomerVer": false
        }
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeDelay, ScriptHandler",
        "Name": "ScriptNodeDelay 4",
        "Interval": 5,
        "IntervalUnite": 2,
        "UserTitle": "Delay",
        "IsPass": false,
        "PassNextId": 4,
        "FailNextId": 0,
        "ID": 3,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": false,
          "IsSaveToPdfExecTable": false,
          "IsSaveToPdfDynTable": false,
          "IsSaveToCustomerVer": false
        }
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "ScriptNodeSubScript 2",
        "ParentScriptName": "Operational Tests",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Max. Voltage Operation",
        "UserTitle": null,
        "IsPass": false,
        "PassNextId": 5,
        "FailNextId": 0,
        "ID": 4,
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
        "Name": "ScriptNodeSubScript 5",
        "ParentScriptName": "Operational Tests",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Peak Operation",
        "UserTitle": null,
        "IsPass": false,
        "PassNextId": 6,
        "FailNextId": 0,
        "ID": 5,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": true,
          "IsSaveToPdfExecTable": true,
          "IsSaveToPdfDynTable": false,
          "IsSaveToCustomerVer": false
        }
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeDelay, ScriptHandler",
        "Name": "Delay 6",
        "Interval": 2,
        "IntervalUnite": 2,
        "UserTitle": "Delay",
        "IsPass": false,
        "PassNextId": 0,
        "FailNextId": 0,
        "ID": 6,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": true,
          "IsSaveToPdfExecTable": true,
          "IsSaveToPdfDynTable": true,
          "IsSaveToCustomerVer": false
        }
      }
    ]
  },
  "IsPass": null
}