{
  "$type": "ScriptHandler.Models.TestData, ScriptHandler",
  "Name": "Initialization",
  "ScriptItemsList": {
    "$type": "System.Collections.ObjectModel.ObservableCollection`1[[ScriptHandler.Interfaces.IScriptItem, ScriptHandler]], System.ObjectModel",
    "$values": [
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "Sub Script 10",
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
        "IsPass": false,
        "PassNextId": 2,
        "FailNextId": 0,
        "ID": 1
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "Sub Script 2",
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
        "IsPass": false,
        "PassNextId": 3,
        "FailNextId": 0,
        "ID": 2
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "Sub Script 3",
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
        "IsPass": false,
        "PassNextId": 0,
        "FailNextId": 0,
        "ID": 3
      }
    ]
  },
  "IsPass": null
}