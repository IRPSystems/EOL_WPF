using DeviceCommunicators.Models;
using EOL.Models;
using Microsoft.Xaml.Behaviors.Media;
using Newtonsoft.Json.Linq;
using ScriptHandler.Enums;
using ScriptHandler.Interfaces;
using ScriptHandler.Models;
using ScriptHandler.Models.ScriptSteps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using WatsReportModels;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;

namespace EOL.Services
{
    public class RunResultToWatsConverter
    {

        public GeneratedProjectData ProjectData;

        private string Location = String.Empty;

        private const string WatsReportPath = "C:\\ProgramData\\Virinco\\WATS\\WatsStandardXmlFormat";
        //private const string WatsReportPath = "C:\\ProgramData\\Virinco\\WATS";

        private const string errorLogPath = "C:\\ProgramData\\Virinco\\WATS\\ErrorLog.txt";
        private const string ReportName = "ReportWats.xml";

        public RunResultToWatsConverter()
        {
            //Location = GetLocalIPAddress();

        }

        private void LogException(Exception ex, string methodName)
        {
            string logMessage = $"{DateTime.Now}: Exception in {methodName} - {ex.Message}\n";

            Directory.CreateDirectory(Path.GetDirectoryName(errorLogPath));
            File.AppendAllText(errorLogPath, logMessage);
        }

        public void SaveRunResultToXml(Reports reports)
        {
            string filePath = Path.Combine(WatsReportPath, ReportName);

            XmlSerializer serializer = new XmlSerializer(typeof(Reports));

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, reports );
            }
        }

        public Step HandleStep(ScriptStepBase stepbase)
        {
            string Status = ReturnStatus(stepbase.IsPass, stepbase.IsExecuted);

            Step step = new Step
            {
                Group = "Main",
                Name = stepbase.UserTitle ?? string.Empty ,
                Status = Status,
                TotalTime = stepbase.ExecutionTime.TotalSeconds,
                StepType = StepTypes.Action,
            };

            switch (stepbase)
            {
                case ScriptStepCompareWithTolerance compareWithTolerance:
                    return HandleScriptStepCompareWithTolerance(compareWithTolerance , step);
                case ScriptStepCompare compare:
                    return HandleScriptStepCompare(compare, step);
                case ScriptStepEOLSendSN sendSN:
                    return HandleScriptStepEOLSendSN(sendSN , step);
                case ScriptStepCompareBit comparebit:
                    return HandleScriptStepCompareBit(comparebit, step);
                case ScriptStepEOLCalibrate calibrate:
                    return HandleScriptStepEOLCalibrate(calibrate, step);
                //case ScriptStepEOLFlash flash:
                //    return HandleScriptStepEOLFlash(flash);
                //case ScriptStepEOLPrint print:
                //    return HandleScriptStepEOLPrint(print);

                // Add cases for other step types
                default:
                    {
                        try
                        {
                            if (!stepbase.IsPass && stepbase.IsExecuted)
                            {
                                step.StepCausedUUTFailure = 1;
                                step.StepErrorMessage = stepbase.ErrorMessage;
                            }

                            return step;
                        }
                        catch(Exception ex)
                        {
                            LogException(ex, nameof(HandleStep) + " - " + stepbase.GetType().Name);
                            throw;
                        }
                    }

            }
        }



        private Step HandleScriptStepEOLCalibrate(ScriptStepEOLCalibrate calibrate,Step step)
        {
            try
            {
                step.NumericLimits = new List<NumericLimit>();


                if (calibrate.IsExecuted)
                {
                    step.StepType = StepTypes.ET_MNLT;
                    NumericLimit numericLimitmcu = CreateNumericLimit(
                        calibrate.McuParam.Name + " (" + calibrate.McuParam.DeviceType.ToString() + ")",
                        0,
                        0,
                        calibrate.AvgMcuRead,
                        calibrate.McuParam.Units,
                        CompOperator.LOG,
                        step.Status
                        );
                    step.NumericLimits.Add(numericLimitmcu);

                    NumericLimit numericLimitref = CreateNumericLimit(
                        calibrate.RefSensorParam.Name + " (" + calibrate.RefSensorParam.DeviceType.ToString() + ")",
                        0,
                        0,
                        calibrate.AvgRefSensorRead,
                        calibrate.McuParam.Units,
                        CompOperator.LOG,
                        step.Status
                        );
                    step.NumericLimits.Add(numericLimitref);

                    NumericLimit numericlimitGain = CreateNumericLimit(
                        calibrate.GainParam.Name + " (" + calibrate.GainParam.DeviceType.ToString() + ")",
                        calibrate.GainMinLimit,
                        calibrate.GainMaxLimit,
                        calibrate.NewGain,
                        calibrate.GainParam.Units,
                        CompOperator.GELE,
                        step.Status
                        );
                    step.NumericLimits.Add(numericlimitGain);

                }

                if (!calibrate.IsPass && calibrate.IsExecuted)
                {
                    step.StepCausedUUTFailure = 1;
                    step.StepErrorMessage = calibrate.ErrorMessage;
                }
                return step;
            }
            catch(Exception ex)
            {
                LogException(ex, nameof(HandleScriptStepEOLCalibrate));
                throw;
            }
        }


        private Step HandleScriptStepCompareBit(ScriptStepCompareBit comparebit,Step step)
        {
            try 
            {


                step.PassFails = new List<PassFail>();

                if (comparebit.IsExecuted)
                {
                    step.StepType = StepTypes.ET_PFT;
                    PassFail passFail = new PassFail
                    {
                        Name = comparebit.FaultName,
                        Status = step.Status
                    };
                    step.PassFails.Add(passFail);
                }

                if (!comparebit.IsPass && comparebit.IsExecuted)
                {
                    step.StepCausedUUTFailure = 1;
                    step.StepErrorMessage = comparebit.ErrorMessage;
                }
                return step;
            }
            catch(Exception ex)
            {
                LogException(ex, nameof(HandleScriptStepCompareBit));
                throw;
            }
          }


        private Step HandleScriptStepEOLSendSN(ScriptStepEOLSendSN sendSN, Step step)
        {
            try
            {
                step.PassFails = new List<PassFail>();

                if (sendSN.IsExecuted)
                {
                    step.StepType = StepTypes.ET_PFT;
                    PassFail passFail = new PassFail
                    {
                        Name = sendSN.SerialNumber ,
                        Status = step.Status
                    };
                    step.PassFails.Add(passFail);
                }

                if (!sendSN.IsPass && sendSN.IsExecuted)
                {
                    step.StepCausedUUTFailure = 1;
                    step.StepErrorMessage = sendSN.ErrorMessage;
                }
                return step;
            }
            catch (Exception ex)
            {
                LogException(ex, nameof(HandleScriptStepEOLSendSN));
                throw;
            }
           
        }

        private Step HandleScriptStepEOLPrint(ScriptStepEOLPrint print)
        {
            throw new NotImplementedException();
        }

        private Step HandleScriptStepEOLFlash(ScriptStepEOLFlash flash)
        {
            throw new NotImplementedException();
        }

        private Step HandleScriptStepCompare(ScriptStepCompare compare , Step step)
        {
            try
            {
                step.NumericLimits = new List<NumericLimit>();


                string compOperator = compare.Comparation switch
                {
                    ComparationTypesEnum.Equal => CompOperator.Equal,
                    ComparationTypesEnum.NotEqual => CompOperator.NotEqual,
                    ComparationTypesEnum.Larger => CompOperator.Larger,
                    ComparationTypesEnum.LargerEqual => CompOperator.LargerEqual,
                    ComparationTypesEnum.Smaller => CompOperator.Smaller,
                    ComparationTypesEnum.SmallerEqual => CompOperator.SmallerEqual,
                    ComparationTypesEnum.BetweenRange => CompOperator.GELE,
                    ComparationTypesEnum.Tolerance => CompOperator.GELE,
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (compare.IsExecuted)
                {
                    if (compare.ValueRight is string stringValue)
                    {

                        step.StepType = StepTypes.ET_NLT;
                        NumericLimit numericlimit = CreateNumericLimit(
                            compare.Parameter.Name + " (" + compare.Parameter.DeviceType.ToString() + ")",
                            Convert.ToDouble(stringValue),
                            Convert.ToDouble(stringValue),
                            Convert.ToDouble(compare.ValueLeft),
                            compare.Parameter.Units,
                            compOperator,
                            step.Status
                        );
                        step.NumericLimits.Add(numericlimit);
                    }
                    else if (compare.ValueRight is double Value)
                    {
                        step.StepType = StepTypes.ET_NLT;
                        NumericLimit numericlimit = CreateNumericLimit(
                            compare.Parameter.Name + " (" + compare.Parameter.DeviceType.ToString() + ")",
                            Convert.ToDouble(Value),
                            Convert.ToDouble(Value),
                            Convert.ToDouble(compare.ValueLeft),
                            compare.Parameter.Units,
                            compOperator,
                            step.Status
                        );
                        step.NumericLimits.Add(numericlimit);
                    }
                    else if (compare.ValueRight is DeviceParameterData param)
                    {
                        step.StepType = StepTypes.ET_MNLT;
                        NumericLimit numericlimitparam = CreateNumericLimit(
                            compare.Parameter.Name + " (" + compare.Parameter.DeviceType.ToString() + ")",
                            Convert.ToDouble(param.Value),
                            Convert.ToDouble(param.Value),
                            Convert.ToDouble(compare.ValueLeft),
                            compare.Parameter.Units,
                            compOperator,
                            step.Status
                        );
                        step.NumericLimits.Add(numericlimitparam);
                        NumericLimit numericlimitcompared = CreateNumericLimit(
                            param.Name + " (" + param.DeviceType.ToString() + ")",
                            0,
                            0,
                            Convert.ToDouble(param.Value),
                            compare.Parameter.Units,
                            CompOperator.LOG,
                            step.Status
                        );
                        step.NumericLimits.Add(numericlimitcompared);
                    }

                }

                if (!compare.IsPass && compare.IsExecuted)
                {
                    step.StepCausedUUTFailure = 1;
                    step.StepErrorMessage = compare.ErrorMessage;
                }
                return step;
            }
            catch(Exception ex)
            {
                LogException(ex, nameof(HandleScriptStepCompare));
                throw;
            }
        }

        private Step HandleScriptStepCompareWithTolerance(ScriptStepCompareWithTolerance compareWithTolerance,Step step)
        {

            try
            {

                step.NumericLimits = new List<NumericLimit>();

                if (compareWithTolerance.IsExecuted)
                {

                    if (compareWithTolerance.CompareValue is string fixedvalue)
                    {
                        double lowerToleranceValue = CalculateToleranceValue(Convert.ToDouble(fixedvalue), compareWithTolerance);
                        double upperToleranceValue = CalculateUpperToleranceValue(Convert.ToDouble(fixedvalue), compareWithTolerance);

                        step.StepType = StepTypes.ET_NLT;
                        NumericLimit numericlimit = CreateNumericLimit(
                            compareWithTolerance.Parameter.Name + " (" +compareWithTolerance.Parameter.DeviceType.ToString() + ")",
                            lowerToleranceValue,
                            upperToleranceValue,
                            Convert.ToDouble(compareWithTolerance.Parameter.Value),
                            compareWithTolerance.Parameter.Units,
                            CompOperator.GELE,
                            step.Status
                        );
                        step.NumericLimits.Add(numericlimit);
                    }
                    else if (compareWithTolerance.CompareValue is DeviceParameterData param)
                    {

                        double lowerToleranceValue = CalculateToleranceValue(Convert.ToDouble(param.Value), compareWithTolerance);
                        double upperToleranceValue = CalculateUpperToleranceValue(Convert.ToDouble(param.Value), compareWithTolerance);

                        step.StepType = StepTypes.ET_MNLT;
                        NumericLimit numericlimitparam = CreateNumericLimit(
                            compareWithTolerance.Parameter.Name + " (" + compareWithTolerance.Parameter.DeviceType.ToString() + ")",
                            lowerToleranceValue,
                            upperToleranceValue,
                            Convert.ToDouble(compareWithTolerance.Parameter.Value),
                            compareWithTolerance.Parameter.Units,
                            CompOperator.GELE,
                            step.Status                           
                        );
                        step.NumericLimits.Add(numericlimitparam);
                        NumericLimit numericlimitcompared = CreateNumericLimit(
                            param.Name + " (" + param.DeviceType.ToString() + ")",
                            0,
                            0,
                            Convert.ToDouble(param.Value),
                            compareWithTolerance.Parameter.Units,
                            CompOperator.LOG,
                            step.Status
                        );
                        step.NumericLimits.Add(numericlimitcompared);
                    }
                }

                if (!compareWithTolerance.IsPass && compareWithTolerance.IsExecuted)
                {
                    step.StepCausedUUTFailure = 1;
                    step.StepErrorMessage = compareWithTolerance.ErrorMessage;
                }
                return step;
            }
            catch(Exception ex)
            {
                LogException(ex, nameof(HandleScriptStepCompareWithTolerance));
                throw;
            }
        }


        private NumericLimit CreateNumericLimit(string name, double lowLimit, double highLimit, double numericValue, string units, string compOperator, string status)
        {
            return new NumericLimit
            {
                Name = name,
                LowLimit = Math.Round(lowLimit, 3),
                HighLimit = Math.Round(highLimit, 3),
                NumericValue = Math.Round(numericValue, 3),
                Units = units,
                CompOperator = compOperator,
                Status = status
            };

        }

        private string ReturnStatus(bool ispass , bool isexecuted)
        {
            return ((ispass == true && isexecuted == true) ? "Passed" : (isexecuted == true) ? "Failed" : "Skipped");
        }

        private double CalculateToleranceValue(double fixedValue, ScriptStepCompareWithTolerance compareWithTolerance)
        {
            double value = Convert.ToDouble(fixedValue);
            if (compareWithTolerance.IsUseParamFactor)
            {
                value *= compareWithTolerance.ParamFactor;
            }

            return compareWithTolerance.IsPercentageTolerance
                ? value - (compareWithTolerance.Tolerance * value)
                : value - compareWithTolerance.Tolerance;
        }

        private double CalculateUpperToleranceValue(double fixedValue, ScriptStepCompareWithTolerance compareWithTolerance)
        {
            double value = Convert.ToDouble(fixedValue);
            if (compareWithTolerance.IsUseParamFactor)
            {
                value *= compareWithTolerance.ParamFactor;
            }

            return compareWithTolerance.IsPercentageTolerance
                ? value + (compareWithTolerance.Tolerance * value)
                : value + compareWithTolerance.Tolerance;
        }
        public static class StepTypes
        {
            public const string SequenceCall = "SequenceCall";
            public const string Action = "Action";
            public const string ET_NLT = "ET_NLT";
            public const string ET_MNLT = "ET_MNLT";
            public const string ET_A = "ET_A";
            public const string ET_MSVT = "ET_MSVT";
            public const string ET_PFT = "ET_PFT";
            public const string ET_MPFT = "ET_MPFT";
        }
        public static class CompOperator
        {
            public const string Equal = "EQ"; // Equal
            public const string NotEqual = "NE"; // Not Equal
            public const string Larger = "GT"; // Greater than
            public const string Smaller = "LT"; // Less than
            public const string LargerEqual = "GE"; // Greater or equal
            public const string SmallerEqual = "LE"; // Less or equal
            public const string GTLT = "GTLT"; // Greater than low limit, less than high limit
            public const string GELE = "GELE"; // Greater or equal than low limit, less or equal than high limit
            public const string GELT = "GELT"; // Greater or equal than low limit, less than high limit
            public const string GTLE = "GTLE"; // Greater than low limit, less or equal than high limit
            public const string LTGT = "LTGT"; // Less than low limit, greater than high limit
            public const string LEGE = "LEGE"; // Less or equal than low limit, greater or equal than high limit
            public const string LEGT = "LEGT"; // Less or equal than low limit, greater than high limit
            public const string LTGE = "LTGE"; // Less than low limit, greater or equal than high limit
            public const string LOG = "LOG"; // Logging values (no comparison)
            public const string CASESENSIT = "CASESENSIT"; // Case sensitive
            public const string IGNORECASE = "IGNORECASE"; // Ignore case / not CASESENSIT
        }

    }

}
