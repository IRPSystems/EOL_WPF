using DeviceCommunicators.Models;
using System;
using System.IO;
using System.Text;
using System.Windows.Shapes;
using Path = System.IO.Path;
using Services.Services;

namespace EOL.Services
{
    public class CommSendResLogCsvWriter
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string fileNamePrefix = "CommSendResponseLog";
        string destinationFolder;
        string fullPath;

        public CommSendResLogCsvWriter()
        {
            destinationFolder = Path.Combine(documentsPath, "Logs");
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            
        }

        public void WriteLog(CommSendResLog log, string sn)
        {
            fullPath = Path.Combine(destinationFolder, fileNamePrefix + " - " + sn + "_" + DateTime.Now.ToString(("yyyy-MM-dd_HH-mm-ss")) + ".csv");
            var csvLine = new StringBuilder();

            csvLine.AppendLine("StepName,Tool,Parameter,Device,SendCommand,ReceivedValue,CommErrorMsg,NumberOfTries");

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

            File.AppendAllText(fullPath, csvLine.ToString());
        }
    }
}
