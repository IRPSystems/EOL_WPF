{
  "$type": "ScriptHandler.Models.TestData, ScriptHandler",
  "Name": "Flash Customer FW",
  "ScriptItemsList": {
    "$type": "System.Collections.ObjectModel.ObservableCollection`1[[ScriptHandler.Interfaces.IScriptItem, ScriptHandler]], System.ObjectModel",
    "$values": [
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "ScriptNodeSubScript 6",
        "ParentScriptName": "Flash Customer FW",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 30,
        "TimeoutUnite": 2,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Pre Flash Operator Guide",
        "UserTitle": null,
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
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeEOLFlash, ScriptHandler",
        "Name": "EOL Flash 3",
        "FlashFilePath": "ggg",
        "RXId": null,
        "TXId": null,
        "Customer": 3,
        "NumOfFlashFile": 0,
        "SourceModeGroupName": "EOLSourceMode_Flash - ggg - ID:2",
        "IsEolSource": true,
        "IsToolSource": false,
        "MCU_Used": false,
        "MCU2_Used": true,
        "UserTitle": "Flash",
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
        "Name": "Sub Script 2",
        "ParentScriptName": "Flash Customer FW",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 30,
        "TimeoutUnite": 2,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Post Flash Validation",
        "UserTitle": null,
        "IsPass": false,
        "PassNextId": 4,
        "FailNextId": 0,
        "ID": 3,
        "EOLReportsSelectionData": {
          "$type": "ScriptHandler.Models.EOLReportsSelectionData, ScriptHandler",
          "IsSaveToReport": false,
          "IsSaveToPdfExecTable": false,
          "IsSaveToPdfDynTable": false
        }
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "ScriptNodeSubScript 7",
        "ParentScriptName": "Flash Customer FW",
        "ContinueUntilType": 0,
        "Repeats": 1,
        "Timeout": 30,
        "TimeoutUnite": 2,
        "IsStopOnFail": true,
        "IsStopOnPass": false,
        "IsInfinity": false,
        "SelectedScriptName": "Post Flash Operator Guide",
        "UserTitle": null,
        "IsPass": false,
        "PassNextId": -1,
        "FailNextId": 0,
        "ID": 4,
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