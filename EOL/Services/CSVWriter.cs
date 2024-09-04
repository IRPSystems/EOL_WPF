using EOL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EOL.Services
{
    public class CSVWriter
    {
        public string _csvFilePath { get; set; }

        private List<string> _headers;
        private bool _headersWritten;

        public CSVWriter()
        {
            _headers = new List<string>();
            _headersWritten = false;
        }

        public void WriteTestResult(TestResult testResult)
        {
            // Use reflection to get properties
            var properties = typeof(TestResult).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                               .Where(p => p.PropertyType == typeof(string))
                                               .ToList();

            // Standard headers
            var standardHeaders = properties.Select(p => p.Name).ToList();

            // Collect headers if not already done
            if (!_headersWritten)
            {
                var stepHeaders = testResult.Steps.Select(s => s.StepDescription).ToList();
                _headers = standardHeaders.Concat(stepHeaders).ToList();

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

                foreach (var header in _headers.Skip(standardHeaders.Count))
                {
                    var step = testResult.Steps.FirstOrDefault(s => s.StepDescription == header);
                    rowValues.Add(step?.Value ?? "");
                }

                writer.WriteLine(string.Join(",", rowValues));
            }
        }
    }
}
