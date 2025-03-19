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

namespace EOL.Services
{
    public class RunResultToWatsConverter
    {

        public GeneratedProjectData ProjectData;

        private string Location = String.Empty;

        private const string WatsReportPath = "C:\\ProgramData\\Virinco\\WATS\\WatsStandardXmlFormat";
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
            switch (stepbase)
            {
                case ScriptStepCompareWithTolerance compareWithTolerance:
                    return HandleScriptStepCompareWithTolerance(compareWithTolerance);
                case ScriptStepCompare compare:
                    return HandleScriptStepCompare(compare);
                //case ScriptStepEOLFlash flash:
                //    return HandleScriptStepEOLFlash(flash);
                //case ScriptStepEOLPrint print:
                //    return HandleScriptStepEOLPrint(print);
                case ScriptStepEOLSendSN sendSN:
                    return HandleScriptStepEOLSendSN(sendSN);
                //case ScriptStepSetParameter setParameter:
                //    return HandleScriptStepSetParameter(setParameter);
                case ScriptStepCompareBit comparebit:
                    return HandleScriptStepCompareBit(comparebit);
                //case ScriptStepConverge converge:
                //    return HandleScriptStepConverge(converge);
                case ScriptStepEOLCalibrate calibrate:
                    return HandleScriptStepEOLCalibrate(calibrate);
                //case ScriptStepLoopIncrement loopIncrement:
                //    return HandleScriptStepLoopIncrement(loopIncrement);
                //case ScriptStepDelay stepDelay:
                //    return HandleScriptStepDelay(stepDelay);

                // Add cases for other step types
                default:
                    {
                        try
                        {
                            string ispassstring = ReturnStatus(stepbase.IsPass, stepbase.IsExecuted);

                            Step step = new Step
                            {
                                Group = "Main",
                                Name = stepbase.UserTitle ?? string.Empty ,
                                TotalTime = stepbase.ExecutionTime.TotalSeconds,
                                Status = ispassstring,
                                StepType = StepTypes.Action,
                                //StepErrorMessage = (step.IsExecuted && !step.IsPass) ? step.ErrorMessage : string.Empty,
                            };

                            if (!stepbase.IsPass && stepbase.IsExecuted)
                                step.StepErrorMessage = stepbase.ErrorMessage;

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

        //private Step HandleScriptStepDelay(ScriptStepDelay stepDelay)
        //{
        //    try
        //    {
        //        string ispassstring = ReturnStatus(stepDelay.IsPass, stepDelay.IsExecuted);

        //        Step step = new Step
        //        {
        //            Group = "Main",
        //            Name = stepDelay.UserTitle ?? string.Empty,
        //            Status = ispassstring,
        //            TotalTime = stepDelay.ExecutionTime.TotalSeconds,
        //            StepType = StepTypes.Action,
        //        };

        //        if (!stepDelay.IsPass && stepDelay.IsExecuted)
        //            step.StepErrorMessage = stepDelay.ErrorMessage;

        //        return step;
        //    }
        //    catch(Exception ex)
        //    {
        //        LogException(ex, nameof(HandleScriptStepDelay));
        //        throw;
        //    }
        //}

        //private Step HandleScriptStepLoopIncrement(ScriptStepLoopIncrement loopIncrement)
        //{
        //    try 
        //    {
        //        string ispassstring = ReturnStatus(loopIncrement.IsPass, loopIncrement.IsExecuted);

        //        Step step = new Step
        //        {
        //            Group = "Main",
        //            Name = loopIncrement.UserTitle ?? string.Empty,
        //            Status = ispassstring,
        //            TotalTime = loopIncrement.ExecutionTime.TotalSeconds,
        //            StepType = StepTypes.Action,
        //        };

        //        if (!loopIncrement.IsPass && loopIncrement.IsExecuted)
        //            step.StepErrorMessage = loopIncrement.ErrorMessage;

        //        return step;
        //    }
        //    catch(Exception ex)
        //    {
        //        LogException(ex, nameof(HandleScriptStepLoopIncrement));
        //        throw;
        //    }
        //}

        private Step HandleScriptStepEOLCalibrate(ScriptStepEOLCalibrate calibrate)
        {
            try
            {
                string ispassstring = ReturnStatus(calibrate.IsPass, calibrate.IsExecuted);

                Step step = new Step
                {
                    Group = "Main",
                    Name = calibrate.UserTitle ?? string.Empty,
                    Status = ispassstring,
                    TotalTime = calibrate.ExecutionTime.TotalSeconds,
                    StepType = StepTypes.Action,
                    NumericLimits = new List<NumericLimit>()
                };

                if (calibrate.IsExecuted)
                {
                    step.StepType = StepTypes.ET_MNLT;
                    NumericLimit numericLimitmcu = CreateNumericLimit(
                        calibrate.McuParam.Name,
                        0,
                        0,
                        calibrate.AvgMcuRead,
                        calibrate.McuParam.Units,
                        CompOperator.LOG,
                        ispassstring
                        );
                    step.NumericLimits.Add(numericLimitmcu);

                    NumericLimit numericLimitref = CreateNumericLimit(
                        calibrate.RefSensorParam.Name,
                        0,
                        0,
                        calibrate.AvgRefSensorRead,
                        calibrate.McuParam.Units,
                        CompOperator.LOG,
                        ispassstring
                        );
                    step.NumericLimits.Add(numericLimitref);

                    NumericLimit numericlimitGain = CreateNumericLimit(
                        calibrate.GainParam.Name,
                        calibrate.GainMinLimit,
                        calibrate.GainMaxLimit,
                        calibrate.NewGain,
                        calibrate.GainParam.Units,
                        CompOperator.GELE,
                        ispassstring
                        );
                    step.NumericLimits.Add(numericlimitGain);

                }

                if (!calibrate.IsPass && calibrate.IsExecuted)
                    step.StepErrorMessage = calibrate.ErrorMessage;

                return step;
            }
            catch(Exception ex)
            {
                LogException(ex, nameof(HandleScriptStepEOLCalibrate));
                throw;
            }
        }

        private Step HandleScriptStepConverge(ScriptStepConverge converge)
        {
            throw new NotImplementedException();
        }

        private Step HandleScriptStepCompareBit(ScriptStepCompareBit comparebit)
        {
            try {
                string ispassstring = ReturnStatus(comparebit.IsPass, comparebit.IsExecuted);

                Step step = new Step
                {
                    Group = "Main",
                    Name = comparebit.UserTitle ?? string.Empty,
                    Status = ispassstring,
                    TotalTime = comparebit.ExecutionTime.TotalSeconds,
                    StepType = StepTypes.Action,
                    PassFails = new List<PassFail>()
                };

                if (comparebit.IsExecuted)
                {
                    step.StepType = StepTypes.ET_PFT;
                    PassFail passFail = new PassFail
                    {
                        Name = comparebit.FaultName,
                        Status = ispassstring
                    };
                    step.PassFails.Add(passFail);
                }

                if (!comparebit.IsPass && comparebit.IsExecuted)
                    step.StepErrorMessage = comparebit.ErrorMessage;

                return step;
            }
            catch(Exception ex)
            {
                LogException(ex, nameof(HandleScriptStepCompareBit));
                throw;
            }
          }

        //private Step HandleScriptStepSetParameter(ScriptStepSetParameter setParameter)
        //{
        //    try
        //    {
        //        string ispassstring = ReturnStatus(setParameter.IsPass, setParameter.IsExecuted);

        //        Step step = new Step
        //        {
        //            Group = "Main",
        //            Name = setParameter.UserTitle ?? string.Empty,
        //            Status = ispassstring,
        //            TotalTime = setParameter.ExecutionTime.TotalSeconds,
        //            StepType = StepTypes.Action,
        //        };

        //        if (!setParameter.IsPass && setParameter.IsExecuted)
        //            step.StepErrorMessage = setParameter.ErrorMessage;

        //        return step;
        //    }
        //    catch(Exception ex)
        //    {
        //        LogException(ex, nameof(HandleScriptStepSetParameter));
        //        throw;
        //    }
        //}

        private Step HandleScriptStepEOLSendSN(ScriptStepEOLSendSN sendSN)
        {
            try
            {
                string ispassstring = ReturnStatus(sendSN.IsPass, sendSN.IsExecuted);

                Step step = new Step
                {
                    Group = "Main",
                    Name = sendSN.UserTitle ?? string.Empty,
                    Status = ispassstring,
                    TotalTime = sendSN.ExecutionTime.TotalSeconds,
                    StepType = StepTypes.Action,
                    PassFails = new List<PassFail>()
                };

                if (sendSN.IsExecuted)
                {
                    step.StepType = StepTypes.ET_PFT;
                    PassFail passFail = new PassFail
                    {
                        Name = sendSN.SerialNumber,
                        Status = ispassstring
                    };
                    step.PassFails.Add(passFail);
                }

                if (!sendSN.IsPass && sendSN.IsExecuted)
                    step.StepErrorMessage = sendSN.ErrorMessage;

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

        private Step HandleScriptStepCompare(ScriptStepCompare compare)
        {
            try
            {
                string ispassstring = ReturnStatus(compare.IsPass, compare.IsExecuted);
                Step step = new Step
                {
                    Group = "Main",
                    Name = compare.UserTitle ?? string.Empty,
                    Status = ispassstring,
                    TotalTime = compare.ExecutionTime.TotalSeconds,
                    NumericLimits = new List<NumericLimit>(),
                    StepType = StepTypes.Action
                };

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
                            compare.Parameter.Name,
                            Convert.ToDouble(stringValue),
                            Convert.ToDouble(stringValue),
                            Convert.ToDouble(compare.ValueLeft),
                            compare.Parameter.Units,
                            compOperator,
                            ispassstring
                        );
                        step.NumericLimits.Add(numericlimit);
                    }
                    else if (compare.ValueRight is double Value)
                    {
                        step.StepType = StepTypes.ET_NLT;
                        NumericLimit numericlimit = CreateNumericLimit(
                            compare.Parameter.Name,
                            Convert.ToDouble(Value),
                            Convert.ToDouble(Value),
                            Convert.ToDouble(compare.ValueLeft),
                            compare.Parameter.Units,
                            compOperator,
                            ispassstring
                        );
                        step.NumericLimits.Add(numericlimit);
                    }
                    else if (compare.ValueRight is DeviceParameterData param)
                    {
                        step.StepType = StepTypes.ET_MNLT;
                        NumericLimit numericlimitparam = CreateNumericLimit(
                            compare.Parameter.Name,
                            Convert.ToDouble(param.Value),
                            Convert.ToDouble(param.Value),
                            Convert.ToDouble(compare.ValueLeft),
                            compare.Parameter.Units,
                            compOperator,
                            ispassstring
                        );
                        step.NumericLimits.Add(numericlimitparam);
                        NumericLimit numericlimitcompared = CreateNumericLimit(
                            param.Name,
                            0,
                            0,
                            Convert.ToDouble(param.Value),
                            compare.Parameter.Units,
                            CompOperator.LOG,
                            ispassstring
                        );
                        step.NumericLimits.Add(numericlimitcompared);
                    }

                }

                if (!compare.IsPass && compare.IsExecuted)
                    step.StepErrorMessage = compare.ErrorMessage;

                return step;
            }
            catch(Exception ex)
            {
                LogException(ex, nameof(HandleScriptStepCompare));
                throw;
            }
        }

        private Step HandleScriptStepCompareWithTolerance(ScriptStepCompareWithTolerance compareWithTolerance)
        {

            try
            {
                string ispassstring = ReturnStatus(compareWithTolerance.IsPass, compareWithTolerance.IsExecuted);

                Step step = new Step
                {
                    Group = "Main",
                    Name = compareWithTolerance.UserTitle ?? string.Empty,
                    Status = ispassstring,
                    TotalTime = compareWithTolerance.ExecutionTime.TotalSeconds,
                    NumericLimits = new List<NumericLimit>(),
                    StepType = StepTypes.Action
                };

                if (compareWithTolerance.IsExecuted)
                {

                    if (compareWithTolerance.CompareValue is string fixedvalue)
                    {
                        double lowerToleranceValue = CalculateToleranceValue(Convert.ToDouble(fixedvalue), compareWithTolerance);
                        double upperToleranceValue = CalculateUpperToleranceValue(Convert.ToDouble(fixedvalue), compareWithTolerance);

                        step.StepType = StepTypes.ET_NLT;
                        NumericLimit numericlimit = CreateNumericLimit(
                            compareWithTolerance.Parameter.Name,
                            lowerToleranceValue,
                            upperToleranceValue,
                            Convert.ToDouble(compareWithTolerance.Parameter.Value),
                            compareWithTolerance.Parameter.Units,
                            CompOperator.GELE,
                            ispassstring
                        );
                        step.NumericLimits.Add(numericlimit);
                    }
                    else if (compareWithTolerance.CompareValue is DeviceParameterData param)
                    {

                        double lowerToleranceValue = CalculateToleranceValue(Convert.ToDouble(param.Value), compareWithTolerance);
                        double upperToleranceValue = CalculateUpperToleranceValue(Convert.ToDouble(param.Value), compareWithTolerance);

                        step.StepType = StepTypes.ET_MNLT;
                        NumericLimit numericlimitparam = CreateNumericLimit(
                            compareWithTolerance.Parameter.Name,
                            lowerToleranceValue,
                            upperToleranceValue,
                            Convert.ToDouble(compareWithTolerance.Parameter.Value),
                            compareWithTolerance.Parameter.Units,
                            CompOperator.GELE,
                            ispassstring
                        );
                        step.NumericLimits.Add(numericlimitparam);
                        NumericLimit numericlimitcompared = CreateNumericLimit(
                            param.Name,
                            0,
                            0,
                            Convert.ToDouble(param.Value),
                            compareWithTolerance.Parameter.Units,
                            CompOperator.LOG,
                            ispassstring
                        );
                        step.NumericLimits.Add(numericlimitcompared);
                    }
                }

                if (!compareWithTolerance.IsPass && compareWithTolerance.IsExecuted)
                    step.StepErrorMessage = compareWithTolerance.ErrorMessage;

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
