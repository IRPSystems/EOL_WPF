using EOL.Models;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Documents;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Font = iTextSharp.text.Font;
using Paragraph = iTextSharp.text.Paragraph;
using Document = iTextSharp.text.Document;
using ScriptHandler.Models;
using ScriptHandler.Interfaces;
using System.Collections.ObjectModel;
using Syncfusion.DocIO.DLS;
using LibUsbDotNet.Main;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Reflection.Emit;
using Services.Services;

namespace EOL.Services
{
    public class PDF_Creator
    {
		#region Properties and Fields

		public string TestOverallStatus { get; set; }

        private string _fileName = String.Empty;

        private string _subFolderName = "\\PDF Reports";

        private int _countDynTable;

        private int _countExecTable;

        List<List<string>> _executiveSumTable = new List<List<string>>();

        List<List<string>> _dynamicDataTable = new List<List<string>>();

		#endregion Properties and Fields

		public void CreatePdf(
            ObservableCollection<GeneratedProjectData> _generatedProjectsList, 
            RunResult runResult, 
            UserDefaultSettings userDefaultSettings)
        {
            // Sample data using List<List<string>> with headers included
            _countDynTable = 0;
            _countExecTable = 0;

            _executiveSumTable.Clear();
            _dynamicDataTable.Clear();
			//testOverallStatus = testingData.OverallTestStatus.ToString();

			#region Get file path
			//TODO
			string function = "Temp";
            _fileName = function + "_PDF_Report - " + runResult.SerialNumber + ".pdf";

            string reportDir = userDefaultSettings.ReportsSavingPath + _subFolderName;

            if (!Directory.Exists(reportDir))
            {
                Directory.CreateDirectory(reportDir);
            }

            string fullSavingPath = System.IO.Path.Combine(reportDir, _fileName);

			#endregion Get file path

            // Get headers of tables
			_executiveSumTable.Add(ExecSumHeaderToList());
            _dynamicDataTable.Add(DynTabHeaderToList());

            // Add steps data to the tables
            foreach (GeneratedProjectData project in _generatedProjectsList)
            {
                foreach (GeneratedScriptData script in project.TestsList)
                {
                    AddTables(script.ScriptItemsList);
                }
            }


            //////Seperate betwweeen TestBrick and Non TestBrick with an empty line on the table
            ////List<string> emptyList = Enumerable.Repeat("-", executiveSumTable[0].Count).ToList(); // Change 5 to the desired number of empty strings
            //executiveSumTable.Add(emptyList);

            //Add extra non test brick data to end of executive table?

            try
            {
                // Create a new PDF document
                Document document = new Document();

                // Create a new PDF writer
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(fullSavingPath, FileMode.Create));

                // Create a new event handler to add content on new pages
                PageEventHandler eventHandler = new PageEventHandler();

                eventHandler.runResult = runResult;
                eventHandler.TesterConfig = userDefaultSettings.AutoConfigPref;
                eventHandler.ScriptPath = userDefaultSettings.DefaultMainSeqConfigFile;
                // Assign the event handler to the writer
                writer.PageEvent = eventHandler;

                // Open the document
                document.Open();

                // Create a new table with the number of rows and columns
                PdfPTable dataTableB = new PdfPTable(_executiveSumTable[0].Count);
                dataTableB.WidthPercentage = 100;

                // Add information line - Exec Summary Table Name
                Paragraph execSumTableName = new Paragraph("Executive Summary");
                execSumTableName.Font.Size = 18;
                execSumTableName.Font.SetStyle(Font.BOLD);

                foreach (List<string> row in _executiveSumTable)
                {
                    foreach (string cellValue in row)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(cellValue));
                        dataTableB.AddCell(cell);
                    }
                }

                // Add information line - Dynamic Table Name
                Paragraph dynamicTableName = new Paragraph("Dynamic Table");
                dynamicTableName.Font.Size = 18;
                dynamicTableName.Font.SetStyle(Font.BOLD);

                // Create a new table with the number of rows and columns
                PdfPTable dataTableC = new PdfPTable(_dynamicDataTable[0].Count);
                dataTableC.WidthPercentage = 100;

                foreach (List<string> row in _dynamicDataTable)
                {
                    foreach (string cellValue in row)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(cellValue));
                        dataTableC.AddCell(cell);
                    }
                }

                // Add Contant
                document.Add(execSumTableName);
                document.Add(new Paragraph(""));
                document.Add(dataTableB);
                document.Add(new Paragraph(""));
                document.Add(dynamicTableName);
                document.Add(new Paragraph(""));
                document.Add(dataTableC);

                // Close the document
                document.Close();

                LoggerService.Inforamtion(this, "PDF created successfully.");
            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to write to the PDF file", "Error", ex); 
            }
        }

        private void AddTables(ObservableCollection<IScriptItem> scriptItemsList)
        {
            foreach (IScriptItem scriptItem in scriptItemsList)
            {
                if (scriptItem is ISubScript subScript)
                {
                    //TODO, add sub script overall result
                    AddTables(subScript.Script.ScriptItemsList);
                }

                if (scriptItem is ScriptStepBase test)
                {
                    if(test.EOLReportsSelectionData.IsSaveToPdfExecTable)
                    {
                        _executiveSumTable.Add(ExecSumResultsToList(test));
                    }
                    if (test.EOLReportsSelectionData.IsSaveToPdfDynTable)
                    {
                        _dynamicDataTable.Add(DynTabResultsToList(test));
                    }
                }
            }
        }

        public List<string> ExecSumResultsToList(ScriptStepBase test)
        {
            _countExecTable++;
            // Declare the list before the foreach loop
            var resultList = new List<string>();

            foreach (EOLStepSummeryData stepSummary in test.EOLStepSummerysList)
            {
                string TestStatus = "Fail";
                if (test.IsPass)
                {
                    TestStatus = "PASS";
                }
                string measuredValue = stepSummary.TestValue.ToString();
                //Uncomment and adjust this part if needed based on specific conditions
                //if (measuredValue == "0" && step.testStep.processType == JsonObject.TestStep.ProcessType.TestBrick.ToString())
                //{
                //    measuredValue = "-";
                //}

                // Add elements to the list inside the loop
                resultList.AddRange(new List<string>
                {
                    CheckValue(_countExecTable.ToString()),
                    CheckValue(stepSummary.StepDescription),
                    CheckValue(measuredValue),
                    CheckValue(TestStatus)
                });
            }

            // Return the list after the loop
            return resultList;
        }

        public List<string> DynTabResultsToList(ScriptStepBase test)
        {
            _countDynTable++;
            var resultList = new List<string>();

            foreach (EOLStepSummeryData stepSummary in test.EOLStepSummerysList)
            {
                string TestStatus = stepSummary.IsPass ? "Passed" : "Failed";

                resultList.AddRange(new List<string>
                {
                    CheckValue(_countDynTable),
                    CheckValue(stepSummary.StepDescription),
                    CheckValue(stepSummary.TestValue),
                    CheckValue(stepSummary.ComparisonValue),
                    CheckValue(stepSummary.Units),
                    CheckValue(stepSummary.Method),
                    CheckValue(stepSummary.MinVal),
                    CheckValue(stepSummary.MaxVal),
                    CheckValue(stepSummary.Tolerance),
                    CheckValue(TestStatus)
                });
                    }

            return resultList;
        }

        private string CheckValue(object value)
        {
            if (value == null)
                return "-";
            else if (value is string str)
                return string.IsNullOrEmpty(str) ? "-" : str;
            else
                return value.ToString();
        }

        private static List<string> ExecSumHeaderToList()
        {
            // Get column headers
            // Get column headers
            var colList = new List<string>
            {
                "NO",
                "Description",
                "Measured Value",
                "Result"
            };
            return colList;
        }

        private static List<string> DynTabHeaderToList()
        {
            // Get column headers
            var colList = new List<string>
            {
                "NO",
                "Description",
                "Test Value",
                "Comparison Value",
                "Units",
                "Method",
                "MinVal",
                "MaxVal",
                "Tolerance",
                "Result"
            };
            return colList;
        }

        public static Paragraph CreateInfoLine(string paramName, string paramValue)
        {
            Paragraph Line = new Paragraph($"{paramName}: {paramValue}");
            Line.Font.Size = 14;
            Line.Font.SetStyle(iTextSharp.text.Font.NORMAL);
            return Line;
        }
    }


    public class PageEventHandler : PdfPageEventHelper
    {
        private string logoPath = "";
        public RunResult runResult { get; set; }
        public string ScriptPath { get; set; }
        public string TesterConfig { get; set; }

        public override void OnStartPage(PdfWriter writer, Document document)
        {

            // Add the header to each page
            Paragraph header = new Paragraph(TesterConfig + " Test Report " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            header.Alignment = Element.ALIGN_CENTER;
            header.Font.Size = 18;
            document.Add(header);

            // Add the logo at the top right corner
            // Relative path to your resource file (adjust as necessary)
            string resourceRelativePath = Path.Combine("Resources", "IRP_vertical_blue_RGB.png");

            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(ResourceImageParser.ImageToByteArray(ResourceHelper.GetBitmapFromExternalResource(resourceRelativePath)));
            logo.ScaleAbsolute(100, 50); // Set the size of the logo as needed
            logo.Alignment = iTextSharp.text.Image.UNDERLYING;
            logo.SetAbsolutePosition(document.PageSize.Width - 120, document.PageSize.Height - 60);
            document.Add(logo);
            // Add information line
            // Add information line
            document.Add(PDF_Creator.CreateInfoLine("Operator Name", runResult.OperatorName));
            document.Add(PDF_Creator.CreateInfoLine("SerialNumber", runResult.SerialNumber));
            document.Add(PDF_Creator.CreateInfoLine("PartNumber", runResult.PartNumber));
            document.Add(PDF_Creator.CreateInfoLine("Date", DateTime.Now.ToString("dd-MM-yyyy")));
            document.Add(PDF_Creator.CreateInfoLine("Time", DateTime.Now.ToString("HH:mm:ss")));
            document.Add(PDF_Creator.CreateInfoLine("Rack No.", "not working yet"));
            //document.Add(PDF_Creator.CreateInfoLine("Win App FW Ver", GeneralAppInfo.AppVersion));
            document.Add(PDF_Creator.CreateInfoLine("JSON Script Ver", Path.GetFileNameWithoutExtension(ScriptPath)));
            // Add empty space
            document.Add(new Paragraph(""));

            // Create a large square shape with text inside
            PdfPTable squareTable = new PdfPTable(1);
            squareTable.WidthPercentage = 10; // Adjust the width percentage as desired
            squareTable.HorizontalAlignment = Element.ALIGN_RIGHT;

            PdfPCell squareCell = new PdfPCell();
            squareCell.FixedHeight = 33.33f; // Adjust the fixed height as desired
            Font font;
            if (runResult.TestStatus == "Passed")
            {
                squareCell.BackgroundColor = iTextSharp.text.Color.GREEN;
                font = new Font(Font.HELVETICA, 12, Font.BOLDITALIC, iTextSharp.text.Color.BLACK);
            }
            else
            {
                squareCell.BackgroundColor = iTextSharp.text.Color.RED;
                font = new Font(Font.HELVETICA, 12, Font.BOLDITALIC, iTextSharp.text.Color.YELLOW);
            }
            Paragraph squareText = new Paragraph(runResult.TestStatus, new Font(font));
            squareCell.BorderColor = iTextSharp.text.Color.BLACK;
            squareCell.BorderWidth = 2f;


            squareText.Alignment = Element.ALIGN_CENTER;
            squareCell.AddElement(squareText);

            squareTable.AddCell(squareCell);

            // Add the square shape table
            document.Add(squareTable);

            // Add empty paragraphs for spacing
            document.Add(new Chunk("\n"));

            // Move to the next line
            document.Add(new Paragraph(""));
        }
    }
}

