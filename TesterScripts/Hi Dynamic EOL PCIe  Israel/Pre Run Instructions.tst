{
  "$type": "ScriptHandler.Models.TestData, ScriptHandler",
  "Name": "Pre Run Instructions",
  "ScriptItemsList": {
    "$type": "System.Collections.ObjectModel.ObservableCollection`1[[ScriptHandler.Interfaces.IScriptItem, ScriptHandler]], System.ObjectModel",
    "$values": [
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeNotification, ScriptHandler",
        "Name": "ScriptNodeNotification 4",
        "Notification": "Connect phase's & power cables\r\n",
        "NotificationLevel": 2,
        "UserTitle": "Power Cables User Instrution",
        "IsPass": false,
        "PassNextId": 2,
        "FailNextId": 0,
        "ID": 1
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeNotification, ScriptHandler",
        "Name": "ScriptNodeNotification 5",
        "Notification": "Toggle the WatchDog switches on!",
        "NotificationLevel": 2,
        "UserTitle": "WatchDog User Instrution",
        "IsPass": false,
        "PassNextId": 3,
        "FailNextId": 0,
        "ID": 2
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeNotification, ScriptHandler",
        "Name": "ScriptNodeNotification 3",
        "Notification": "Connect Ground Cable",
        "NotificationLevel": 2,
        "UserTitle": "Ground Cable User Instrution",
        "IsPass": false,
        "PassNextId": 4,
        "FailNextId": 0,
        "ID": 3
      },
      {
        "$type": "ScriptHandler.Models.ScriptNodes.ScriptNodeSubScript, ScriptHandler",
        "Name": "Sub Script 2",
        "ParentScriptName": "Pre Run Instructions",
        "ContinueUntilType": 0,
        "Repeats": 3,
        "Timeout": 0,
        "TimeoutUnite": 0,
        "IsStopOnFail": false,
        "IsStopOnPass": true,
        "IsInfinity": false,
        "SelectedScriptName": "InterLock Verification",
        "UserTitle": "InterLock Verification",
        "IsPass": false,
        "PassNextId": -1,
        "FailNextId": 0,
        "ID": 4
      }
    ]
  },
  "IsPass": null
}