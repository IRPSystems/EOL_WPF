using EOL.Models;
using ScriptHandler.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;
using Org.BouncyCastle.Bcpg.Sig;
using Services.Services;
using System.Collections.ObjectModel;
using ScriptHandler.Interfaces;
using ScriptHandler.Models.ScriptSteps;

namespace EOL.Services
{
    public class CSVWriter
    {
		#region Properties and Fields

		public string _csvFilePath { get; set; }

        private List<string> _headers;

		#endregion Properties and Fields

		#region Constructor

		public CSVWriter()
        {
            _headers = new List<string>();
        }

		#endregion Constructor

		#region Methods

		public void WriteTestResult(
            RunResult testResult,
            List<GeneratedProjectData> projectsList)
        {
            if (string.IsNullOrEmpty(_csvFilePath))
                return;

            if(testResult.StopReason != "PASSED")
                testResult.StopReason = GetFailedStepDescription(testResult.FailedStep);

			// Use reflection to get properties
			var properties = typeof(RunResult).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                               .Where(p => p.PropertyType == typeof(string))
                                               .ToList();

            // Standard headers
            var standardHeaders = properties.Select(p => p.Name).ToList();

            // Collect headers if not already done
            if (File.Exists(_csvFilePath) == false)
            {
                _headers = GetHeaders(projectsList);
				_headers.InsertRange(0, standardHeaders);


				using (var writer = new StreamWriter(_csvFilePath, append: false))
                {
                    // Write headers
                    writer.WriteLine(string.Join(",", _headers));
                }
            }

            // Write the row data
            using (var writer = new StreamWriter(_csvFilePath, append: true))
            {
                var rowValues = new List<string>();

                foreach (var property in properties)
                {
                    rowValues.Add(property.GetValue(testResult)?.ToString() ?? "");
                }

                List<string> values = GetValues(projectsList);
                foreach(string value in values)
                {
					rowValues.Add(value);
				}

				writer.WriteLine(string.Join(",", rowValues));
            }
        }

        private List<string> GetHeaders(
			List<GeneratedProjectData> projectsList)
        {
            List<string> headers = new List<string>();

            foreach (GeneratedProjectData project in projectsList)
            {
                foreach(GeneratedScriptData scriptData in project.TestsList)
                {
                    List<string> scriptHeaders = 
                        GetScriptHeaders(scriptData.ScriptItemsList);
                    headers.AddRange(scriptHeaders);
				}
            }

    

            return headers;
        }

        private List<string> GetScriptHeaders(ObservableCollection<IScriptItem> scriptItemsList)
        {
            List<string> headers = new List<string>();
            foreach (IScriptItem item in scriptItemsList)
            {
                if(item is ISubScript subScript)
                {
					List<string> subScriptHeaders = 
                        GetScriptHeaders(subScript.Script.ScriptItemsList);
                    headers.AddRange(subScriptHeaders);
                    continue;
                }

                if(!(item is ScriptStepBase stepBase)) 
                    continue;

                if(stepBase.EOLReportsSelectionData.IsSaveToReport == false)
                    continue;

                List<string> itemHeaders = stepBase.GetReportHeaders();
                headers.AddRange(itemHeaders);

            }

            return headers;
		}

        private List<string> GetValues(
			List<GeneratedProjectData> projectsList)
		{
			List<string> values = new List<string>();

			foreach (GeneratedProjectData project in projectsList)
			{
				foreach (GeneratedScriptData scriptData in project.TestsList)
				{
					List<string> scriptValues =
						GetScriptValues(scriptData.ScriptItemsList);
					values.AddRange(scriptValues);
				}
			}

			return values;
		}

		private List<string> GetScriptValues(ObservableCollection<IScriptItem> scriptItemsList)
		{
			List<string> values = new List<string>();
			foreach (IScriptItem item in scriptItemsList)
			{
				if (item is ISubScript subScript)
				{
					List<string> subScriptValues =
						GetScriptValues(subScript.Script.ScriptItemsList);
					values.AddRange(subScriptValues);
					continue;
				}

				if (!(item is ScriptStepBase stepBase))
					continue;

				if (stepBase.EOLReportsSelectionData.IsSaveToReport == false)
					continue;

				List<string> itemValues = stepBase.GetReportValues();
				values.AddRange(itemValues);

			}

			return values;
		}



        private string GetFailedStepDescription(ScriptStepBase failedStep)
        {
            if (failedStep == null)
                return null;

            string description = string.Empty;

            List<string> header = failedStep.GetReportHeaders();
            if(header != null && header.Count > 0)
            {
                description = FixStringService.GetFixedString(header[0]);
            }


			description += FixStringService.GetFixedString(failedStep.ErrorMessage);
            description = $"\"{description}\"";

			return description;
        }

        

		#endregion Methods
	}
}
