﻿using EOL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Font = iTextSharp.text.Font;
using Paragraph = iTextSharp.text.Paragraph;
using Document = iTextSharp.text.Document;
using ScriptHandler.Models;
using ScriptHandler.Interfaces;
using System.Collections.ObjectModel;
using Services.Services;
using TestersDB_Lib.Models;
using System.Linq;

namespace EOL.Services
{
    public class PDF_Creator
    {
        #region Properties and Fields

        public string TestOverallStatus { get; set; }
        public string CustomerVer { get; set; }

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
            CustomerVer = String.Empty;

            _executiveSumTable.Clear();
            _dynamicDataTable.Clear();
            //testOverallStatus = testingData.OverallTestStatus.ToString();

            #region Get file path
            //TODO
            string function = "EOL";
            _fileName = function + "_PDF_Report - " + runResult.SerialNumber + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".pdf";

            string reportDir = userDefaultSettings.ReportsSavingPath + _subFolderName;

            if (!Directory.Exists(reportDir))
            {
                Directory.CreateDirectory(reportDir);
            }

            string fullSavingPath = System.IO.Path.Combine(reportDir, _fileName);

            #endregion Get file path

            // Get headers of tables
            //_executiveSumTable.Add(ExecSumHeaderToList());
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
                eventHandler.runResult.CustomerVer = CustomerVer;
                eventHandler.TesterConfig = userDefaultSettings.AutoConfigPref;
                eventHandler.ScriptPath = userDefaultSettings.DefaultMainSeqConfigFile;
                // Assign the event handler to the writer
                writer.PageEvent = eventHandler;

                // Open the document
                document.Open();

                // Create a new table with the number of rows and columns
                //PdfPTable dataTableB = new PdfPTable(_executiveSumTable[0].Count);
                //dataTableB.WidthPercentage = 100;

                // Add information line - Exec Summary Table Name
                //Paragraph execSumTableName = new Paragraph("Executive Summary");
                //execSumTableName.Font.Size = 18;
                //execSumTableName.Font.SetStyle(Font.BOLD);

                //foreach (List<string> row in _executiveSumTable)
                //{
                //    foreach (string cellValue in row)
                //    {
                //        PdfPCell cell = new PdfPCell(new Phrase(cellValue));
                //        dataTableB.AddCell(cell);
                //    }
                //}

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
                // document.Add(execSumTableName);
                document.Add(new Paragraph(""));
                //document.Add(dataTableB);
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


                if (scriptItem is ScriptStepBase step &&
                    step.EOLReportsSelectionData != null)
                {
                    if(step.EOLReportsSelectionData.IsSaveToCustomerVer)
                        RetrieveCustomerVer(step);

                    if (step.EOLReportsSelectionData.IsSaveToPdfDynTable)
                    {
                        List<string> list = DynTabResultsToList(step);
                        if (list != null && list.Count > 0)
                            _dynamicDataTable.Add(DynTabResultsToList(step));
                    }
                }
                    //if (step.EOLReportsSelectionData.IsSaveToPdfExecTable)
                    //{
                    //    _executiveSumTable.Add(ExecSumResultsToList(step));
                    //}

                }
            }
        

    private void RetrieveCustomerVer(ScriptStepBase step)
    {
        if (step.UserTitle != null)
        {
            if (step is ScriptStepCompareWithTolerance steptolerance)
            {
                if (steptolerance.Parameter.Value != null && int.TryParse(steptolerance.Parameter.Value.ToString(), out int value))
                {
                    string formattedValue = value.ToString("D2"); // Format as two digits

                    //Split existing CustomerVer and maintain the "xx.xx.xx" pattern
                    var segments = CustomerVer.Split('.').ToList();
                    segments.Add(formattedValue);

                    // Ensure only the last three segments are kept
                    if (segments.Count > 3)
                    {
                        segments = segments.Skip(segments.Count - 3).ToList();
                    }

                    // Reconstruct CustomerVer
                    CustomerVer = string.Join('.', segments);
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
                    CheckValue(stepSummary.ParentStepDescription),
                    CheckValue(measuredValue),
                    CheckValue(TestStatus)
                });
            }

            // Return the list after the loop
            return resultList;
        }

        public List<string> DynTabResultsToList(ScriptStepBase step)
        {
            _countDynTable++;
            var resultList = new List<string>();

            try
            {

                if (step.EOLStepSummerysList == null || step.EOLStepSummerysList.Count == 0)
                    return null;


                EOLStepSummeryData stepSummary = step.EOLStepSummerysList.Find(
                    (s) => string.IsNullOrEmpty(s.ParentStepDescription));

                if (stepSummary == null)
                    return null;

                string testStatus = stepSummary.IsPass ? "Passed" : "Failed";
                //if (!step.IsExecuted)
                //    testStatus = "Not Executed";

                string description = stepSummary.Description;
                if (string.IsNullOrEmpty(stepSummary.Description))
                    description = stepSummary.ParentStepDescription;

                string testValue = "";
                if (stepSummary.TestValue != null)
                    testValue = ((double)stepSummary.TestValue).ToString("f2");

                string comparisonvalue = "";
                if (stepSummary.ComparisonValue != null)
                    comparisonvalue = ((double)stepSummary.ComparisonValue).ToString("f2");

                string minval = "";
                if (stepSummary.MinVal != null)
                    minval = ((double)stepSummary.MinVal).ToString("f2");

                string maxval = "";
                if (stepSummary.MaxVal != null)
                    maxval = ((double)stepSummary.MaxVal).ToString("f2");

                resultList.AddRange(new List<string>
                {
                    CheckValue(_countDynTable),
                    CheckValue(description),
                    CheckValue(testValue),
                    CheckValue(comparisonvalue),
                    CheckValue(stepSummary.Units),
                    CheckValue(stepSummary.Method),
                    CheckValue(minval),
                    CheckValue(maxval),
                   // CheckValue(stepSummary.Tolerance),
                    CheckValue(testStatus)
                });
            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to add summery", "Error", ex);
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
               // "Tolerance",
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
        //private string logoPath = "";
        public RunResult runResult { get; set; }
        public string ScriptPath { get; set; }
        public string TesterConfig { get; set; }

        public override void OnStartPage(PdfWriter writer, Document document)
        {

            // Add the header to each page
            Paragraph header = new Paragraph(TesterConfig + " Test Report");
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
            document.Add(PDF_Creator.CreateInfoLine("Rack No.", runResult.RackNumber));
            //document.Add(PDF_Creator.CreateInfoLine("Win App FW Ver", GeneralAppInfo.AppVersion));
            document.Add(PDF_Creator.CreateInfoLine("JSON Script Ver", Path.GetFileNameWithoutExtension(ScriptPath)));
            if (runResult.CustomerVer != string.Empty)
                document.Add(PDF_Creator.CreateInfoLine("Customer SW Version", runResult.CustomerVer));
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
    


