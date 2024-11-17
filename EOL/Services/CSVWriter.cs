﻿using EOL.Models;
using ScriptHandler.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;

namespace EOL.Services
{
    public class CSVWriter
    {
		#region Properties and Fields

		public string _csvFilePath { get; set; }

        private List<string> _headers;
        private bool _headersWritten;

		#endregion Properties and Fields

		#region Constructor

		public CSVWriter()
        {
            _headers = new List<string>();
            _headersWritten = false;
        }

		#endregion Constructor

		#region Methods

		public void WriteTestResult(RunResult testResult)
        {
            if (string.IsNullOrEmpty(_csvFilePath))
                return;

            // Use reflection to get properties
            var properties = typeof(RunResult).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                               .Where(p => p.PropertyType == typeof(string))
                                               .ToList();

            // Standard headers
            var standardHeaders = properties.Select(p => p.Name).ToList();

            // Collect headers if not already done
            if (!_headersWritten)
            {
                _headers = GetHeaders(testResult.Steps);
				_headers.InsertRange(0, standardHeaders);


				using (var writer = new StreamWriter(_csvFilePath, append: false))
                {
                    // Write headers
                    writer.WriteLine(string.Join(",", _headers));
                    _headersWritten = true;
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

                List<string> values = GetValues(testResult.Steps);
                foreach(string value in values)
                {
					rowValues.Add(value);
				}

				writer.WriteLine(string.Join(",", rowValues));
            }
        }

        private List<string> GetHeaders(List<EOLStepSummeryData> Steps)
        {
            List<string> headers = new List<string>();

            foreach (EOLStepSummeryData step in Steps)
            {
                if (step.Step.EOLReportsSelectionData.IsSaveToReport == false)
                    continue;

                string testName = GetFixedString(step.TestName);
				string subScriptName = GetFixedString(step.SubScriptName);
				string parentStepDescription = GetFixedString(step.ParentStepDescription);

				string description = $"{testName} -- ";
                
                if(subScriptName != testName)
                    description += $"{subScriptName} -- ";


				if (string.IsNullOrEmpty(parentStepDescription) == false)
                    description += $"{parentStepDescription} -- ";

                description += GetFixedString(step.Description);

                headers.Add(description);

            }

            return headers;
        }

        private List<string> GetValues(List<EOLStepSummeryData> Steps)
		{
			List<string> values = new List<string>();

			foreach (EOLStepSummeryData step in Steps)
			{
				if (step.Step.EOLReportsSelectionData.IsSaveToReport == false)
					continue;

				if(step.TestValue != null)
                {
                    values.Add(step.TestValue.ToString());
                    continue;
                }

                if(step.IsPass == false)
                {
					values.Add(step.ErrorDescription);
					continue;
				}

				values.Add("PASS");
			}

			return values;
		}

        private string GetFixedString(string source)
        {
            string dest = source;

			dest = dest.Replace(",", "-");
			dest = dest.Replace("\r", "");
			dest = dest.Replace("\n", " - ");

			return dest;
		}

		#endregion Methods
	}
}
