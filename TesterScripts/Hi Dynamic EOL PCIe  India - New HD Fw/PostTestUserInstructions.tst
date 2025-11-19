{
  "$type": "ScriptHandler.Models.TestData, ScriptHandler",
  "Name": "PostTestUserInstructions",
  "ScriptItemsList": {
    "$type": "System.Collections.ObjectModel.ObservableCollection`1[[ScriptHandler.Interfaces.IScriptItem, ScriptHandler]], System.ObjectModel",
    "$values": [
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeNotification, ScriptHandler",
        "Notification": "Check Power Supply Output Is OFF!",
        "NotificationLevel": 1,
        "UserTitle": "PS Off Verification",
        "Name": "Notification 1",
        "IsPass": false,
        "PassNextId": 0,
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
      }
    ]
  },
  "IsPass": null
}