
using CommunityToolkit.Mvvm.ComponentModel;
using ScriptRunner.Services;
using System;

namespace EOL.Models
{
	public class RunData: ObservableObject
	{
		public string SerialNumber { get; set; }
		public string PartNumber { get; set; }
		public string OperatorName { get; set; }
		public int RackNum { get; set; }

		public DateTime StartTime { get; set; }
		public TimeSpan Duration { get; set; }
		public DateTime EndTime { get; set; }

		public int NumberOfTested { get; set; }
		public int NumberOfFailed { get; set; }
		public int NumberOfPassed { get; set; }

		public RunScriptService RunScript { get; set; }
	}
}
