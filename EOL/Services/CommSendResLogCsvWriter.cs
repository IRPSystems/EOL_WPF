using DeviceCommunicators.Models;
using System;
using System.IO;
using System.Text;
using System.Windows.Shapes;
using Path = System.IO.Path;
using Services.Services;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Linq;

namespace EOL.Services
{
    public class CommSendResLogCsvWriter
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string fileNamePrefix = "CommSendResponseLog";
        string destinationFolder;
        string fullPath;
        StringBuilder csvLine = new();

        public CommSendResLogCsvWriter()
        {
            destinationFolder = Path.Combine(documentsPath, "Logs");
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            
        }

        public void CreatLog(string sn)
        {
            fullPath = Path.Combine(destinationFolder, fileNamePrefix + " - " + sn + "_" + DateTime.Now.ToString(("yyyy-MM-dd_HH-mm-ss")) + ".csv");
            csvLine.AppendLine("StepName,Tool,Parameter,Device,SendCommand,ReceivedValue,ErrorMsg,NumberOfTries");
            File.AppendAllText(fullPath, csvLine.ToString());
            csvLine.Clear();
        }

        public void WriteLog(List<CommSendResLog> logs)
        {
            logs = TrimSendResLogs(logs);
            csvLine.Clear();
            foreach (var log in logs)
            {
                var logValues = new string[]
                {                   
                log.StepName,
                log.Tool,
                log.ParamName,
                log.Device,
                log.SendCommand,
                log.ReceivedValue,
                log.CommErrorMsg,
                log.NumberOfTries.ToString()
                };

                for (int i = 0; i < logValues.Length; i++)
                {
                    logValues[i] = CsvHelperTool.RemoveCsvSpecialCharacters(logValues[i]);
                }
                csvLine.AppendLine(string.Join(",", logValues));
            }


            File.AppendAllText(fullPath, csvLine.ToString());
        }

        private List<CommSendResLog> TrimSendResLogs(List<CommSendResLog> allLogs)
        {
            // 1) Identify the last 5 distinct step names by scanning from the end (newest).
            var last5DistinctSteps = new HashSet<string>();
            for (int i = allLogs.Count - 1; i >= 0; i--)
            {
                string stepName = allLogs[i].StepName;
                if (!last5DistinctSteps.Contains(stepName))
                {
                    last5DistinctSteps.Add(stepName);
                    if (last5DistinctSteps.Count == 5)
                        break; // Found 5 step names, no need to continue.
                }
            }

            // 2) Filter the entire list to keep logs whose StepName is in the last5DistinctSteps
            var trimmed = allLogs
                .Where(log => last5DistinctSteps.Contains(log.StepName))
                .ToList();

            // 3) Return the trimmed logs (still in chronological order).
            return trimmed;
        }
    }
}
