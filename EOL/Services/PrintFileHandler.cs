using DeviceCommunicators.TSCPrinter;
using ScriptHandler.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EOL.Services
{
    public class PrintFileHandler
    {
        #region Properties
        private readonly string commonSnVarIndicator = "{SN}";
        private string PrintFileDesign;
        #endregion
        public bool OpenPrintFile(out string error)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory; // Get the application directory
            string[] prnFiles = Directory.GetFiles(appDirectory, "*.prn");
            error = "No Error";

            if (prnFiles.Length > 0)
            {
                string firstPrnFilePath = prnFiles[0];
                string inputString = null;

                try
                {
                    // Read the file to extract encoding information
                    using (StreamReader reader = new StreamReader(firstPrnFilePath))
                    {
                        // Read the file line by line to find the encoding
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.StartsWith("CODEPAGE", StringComparison.OrdinalIgnoreCase))
                            {
                                // Define the regular expression pattern to match numbers
                                string pattern = @"\d+";
                                // Use Regex.Match to find the first occurrence of the pattern
                                Match match = Regex.Match(line, pattern);
                                // Check if a match is found
                                if (match.Success)
                                {
                                    if (int.TryParse(match.Value, out int codePage))
                                    {
                                        // Get the encoding using the code page number
                                        Encoding encoding = Encoding.GetEncoding(codePage);

                                        // Read the file using the specified encoding.
                                        inputString = File.ReadAllText(firstPrnFilePath, encoding);

                                        if (!inputString.Contains(commonSnVarIndicator))
                                        {
                                            error = "Print file deviates from expected conventions\r\n" +
                                                    "Expected {SN} variable convention missing\r\n";
                                            return false;
                                        }

                                        // Now, you have the contents of the .prn file in the inputString variable.

                                        PrintFileDesign = RearrangePrnContent(inputString);
                                        break; // Exit the loop after finding the encoding
                                    }
                                    else
                                    {
                                        error = "Parse method error";
                                        return false;
                                    }
                                }
                                else
                                {
                                    error = "Parse method error";
                                    return false;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = "Print file parsing error:\r\n\r\n" + ex.Message;
                    return false;
                }
            }
            else
            {
                error = "Print file missing";
                return false;
            }
            return true;
        }

        public string BuildPrinterCmd(ScriptStepEOLPrint scriptStepEOLPrint)
        {
            string partNumber = scriptStepEOLPrint.PartNumber;
            string printerDynamicCmd = PrintFileDesign;
            try
            {
                DateTime currentDate = DateTime.Now;
                string date = currentDate.ToString("dd-MM-yyyy");
                // Dictionary to store variable replacements
                var replacements = new Dictionary<string, string>
                {
                    { "{SN}", scriptStepEOLPrint.SerialNumber },
                    { "{PN}", scriptStepEOLPrint.PartNumber },
                    { "{CM PN}", scriptStepEOLPrint.CustomerPartNumber },
                    { "{SP}", scriptStepEOLPrint.Spec },
                    { "{HW}", scriptStepEOLPrint.HW_Version }, 
                    { "{SW}", scriptStepEOLPrint.MCU_Version },
                    { "{date}", date }
                };

                // Replace variables in the input string using regular expressions
                foreach (var replacement in replacements)
                {
                    if (replacement.Value != null)
                    {
                        string pattern = Regex.Escape(replacement.Key);
                        printerDynamicCmd = Regex.Replace(printerDynamicCmd, pattern, replacement.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, such as file not found or permission issues.
                MessageBox.Show("Printer Label Parsing Error: " + ex.Message);
                return null;
            }

            WriteToTxt(printerDynamicCmd.ToString());

            return printerDynamicCmd;
        }

        private string RearrangePrnContent(string originalContent)
        {
            // Define the marker for the end of a line
            string lineEndMarker = "\r\n";

            // Initialize a StringBuilder to store the rearranged content
            StringBuilder rearrangedContent = new StringBuilder();

            // Initialize a StringBuilder to store the bitmap sections
            StringBuilder bitmapSections = new StringBuilder();

            // Find the start index of the bitmap section
            int startIndex = originalContent.IndexOf("BITMAP");

            if (startIndex == -1)
            {
                // Handle case when bitmap section start index is not found
                Console.WriteLine("Bitmap section not found");
                return originalContent; // Return original content without rearrangement
            }

            // Iterate through the content to cut out bitmap sections
            int currentIndex = startIndex;
            while (currentIndex != -1)
            {
                // Find the end index of the current bitmap section
                int endIndex = originalContent.IndexOf(lineEndMarker, currentIndex);

                if (endIndex == -1)
                {
                    // Handle case when end of bitmap section is not found
                    Console.WriteLine("End of bitmap section not found");
                    return originalContent; // Return original content without rearrangement
                }

                // Extract the bitmap section
                string bitmapSection = originalContent.Substring(startIndex, endIndex - startIndex);

                // Append the bitmap section to the bitmapSections StringBuilder
                bitmapSections.AppendLine(bitmapSection);

                // Move to the next potential bitmap section
                currentIndex = originalContent.IndexOf("BITMAP", endIndex);
                startIndex = currentIndex;
            }

            // Remove bitmap sections from the original content
            originalContent = originalContent.Replace(bitmapSections.ToString(), "");

            // Rearrange the remaining content
            string[] lines = originalContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.Length > 0)
                {
                    int tabCount = line.Length - trimmedLine.Length;
                    string tabs = new string('\t', tabCount);
                    rearrangedContent.Append(tabs);
                    rearrangedContent.AppendLine(trimmedLine);
                    if (trimmedLine.Contains("CLS"))
                    {
                        // Append bitmap sections under the line with 'CLS'
                        rearrangedContent.AppendLine(bitmapSections.ToString());
                    }
                }
            }

            // Write the rearranged content to a text file
            WriteToTxt(rearrangedContent.ToString());

            return rearrangedContent.ToString();
        }

        private static void WriteToTxt(string content)
        {
            string path = Environment.CurrentDirectory + "/" + "Post.txt";
            File.WriteAllText(path, content);
        }
    }
}
