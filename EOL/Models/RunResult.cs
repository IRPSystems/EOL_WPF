using ScriptHandler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOL.Models
{
    public class RunResult
    {
        public string SerialNumber { get; set; }
        public string PartNumber { get; set; }
        public string OperatorName { get; set; }
        public string RackNumber { get; set; }
        public string TestStatus { get; set; }
        public string StopReason { get; set; }
        public string StartTimeStamp { get; set; }
        public string EndTimeStamp { get; set; }
        public string PackageVersion { get; set; }
        public string AppVersion { get; set; }

        public List<EOLStepSummeryData> Steps { get; set; }
        public ScriptStepBase FailedStep { get; set; }

        public RunResult()
        {
            Steps = new List<EOLStepSummeryData>();
        }
    }
}
