using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using Entities.Models;
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
using WatsConstants;
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
        private const string ReportName = "ReportWats";
        private const string fileType = ".xml";

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

        public void SaveRunResultToXml(Reports reports, string backUpPath, string sn)
        {
            string filePath = Path.Combine(WatsReportPath, ReportName + fileType);

            XmlSerializer serializer = new XmlSerializer(typeof(Reports));

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, reports );
            }
            string backupFilePath = Path.Combine(backUpPath, ReportName + " - " + sn + "_" + DateTime.Now.ToString(("yyyy-MM-dd_HH-mm-ss")) + fileType);

            using (StreamWriter writer = new StreamWriter(backupFilePath))
            {
                serializer.Serialize(writer, reports);
            }

            // Create a backup of the file 

        }

        public Step HandleStep(ScriptStepBase stepbase)
        {
            string Status = ReturnStatus(stepbase.IsPass, stepbase.IsExecuted , stepbase.IsError);

            Step step = new Step
            {
                Group = "Main",
                Name = stepbase.UserTitle ?? string.Empty ,
                Status = Status,
                TotalTime = stepbase.ExecutionTime.TotalSeconds,
                StepType = StepTypes.Action,
            };

            if (!stepbase.IsPass && stepbase.IsExecuted)
            {
                step.StepCausedUUTFailure = 1;
                if (stepbase.IsError == true)
                {
                    step.StepErrorMessage = stepbase.ErrorMessage;
                    step.StepErrorCode = 1;
                    step.StepErrorCodeFormat = "Error";
                }
            }

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
                case ScriptStepGetRegisterValues getregistervalues:
                    return HandleScriptStepGetRegisterValues(getregistervalues, step);
                case ScriptStepEOLCalibrate calibrate:
                    return HandleScriptStepEOLCalibrate(calibrate, step);
                case ScriptStepEOLFlash flash:
                    return HandleScriptStepEOLFlash(flash, step);
                //case ScriptStepEOLPrint print:
                //    return HandleScriptStepEOLPrint(print);

                // Add cases for other step types
                default:
                    {
                        try
                        {
                            if(Status == StatusCodes.Failed || Status == StatusCodes.Error)
                            {
                                return step;
                            }

                            return null;
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

                return step;
            }
            catch(Exception ex)
            {
                LogException(ex, nameof(HandleScriptStepEOLCalibrate));
                throw;
            }
        }


        private Step HandleScriptStepCompareBit(ScriptStepCompareBit comparebit, Step step)
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


                return step;
            }
            catch(Exception ex)
            {
                LogException(ex, nameof(HandleScriptStepCompareBit));
                throw;
            }
        }

        private Step HandleScriptStepGetRegisterValues(ScriptStepGetRegisterValues getregistervalues, Step step)
        {
            try
            {


                step.PassFails = new List<PassFail>();

                if (getregistervalues.IsExecuted)
                {
                    step.StepType = StepTypes.ET_PFT;
                    PassFail passFail = new PassFail
                    {
                        Name = getregistervalues.FaultList,
                        Status = step.Status
                    };
                    step.PassFails.Add(passFail);
                }


                return step;
            }
            catch (Exception ex)
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

        private Step HandleScriptStepEOLFlash(ScriptStepEOLFlash flash, Step step)
        {
            try
            {
                step.PassFails = new List<PassFail>();
                string fileName = Path.GetFileName(flash.FilePath);

                if (flash.IsExecuted)
                {
                    step.StepType = StepTypes.ET_PFT;
                    PassFail passFail = new PassFail
                    {
                        Name = fileName,
                        Status = step.Status
                    };
                    step.PassFails.Add(passFail);
                }

                return step;
            }
            catch (Exception ex)
            {
                LogException(ex, nameof(HandleScriptStepCompareBit));
                throw;
            }
        }

        private Step HandleScriptStepCompare(ScriptStepCompare compare , Step step)
        {
            try
            {
                step.NumericLimits = new List<NumericLimit>();
                step.StringValues = new List<StringValue>();

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
                    if (compare.ValueLeft is DeviceParameterData paramleft)
                    { 
                       // Use reflection to check if the class has a DropdownParameter property
                       var dropdownProperty = paramleft.GetType().GetProperty("DropDown");
                        if (dropdownProperty != null)
                        {
                            var dropdownValue = dropdownProperty.GetValue(paramleft);
                            if (dropdownValue != null && paramleft.Value is string strvalue)
                            {
                                foreach (var item in (List<DropDownParamData>)dropdownValue)
                                {
                                    if (item.Value == compare.ValueRight.ToString())
                                    {
                                        step.StepType = StepTypes.ET_SVT;
                                        StringValue stringvalue = new StringValue
                                        {
                                            Name = paramleft.Name + " (" + paramleft.DeviceType.ToString() + ")",
                                            Value = strvalue,
                                            CompOperator = compOperator,
                                            Status = step.Status,
                                            StringLimit = item.Name
                                        };
                                        step.StringValues.Add(stringvalue);
                                        break;
                                    }
                                }
                            }

                        }
                    }
                    else if (compare.ValueRight is string stringValue)
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
                    else if (compare.ValueRight is DeviceParameterData paramright)
                    {
                        step.StepType = StepTypes.ET_MNLT;
                        NumericLimit numericlimitparam = CreateNumericLimit(
                            compare.Parameter.Name + " (" + compare.Parameter.DeviceType.ToString() + ")",
                            Convert.ToDouble(paramright.Value),
                            Convert.ToDouble(paramright.Value),
                            Convert.ToDouble(compare.ValueLeft),
                            compare.Parameter.Units,
                            compOperator,
                            step.Status
                        );
                        step.NumericLimits.Add(numericlimitparam);
                        NumericLimit numericlimitcompared = CreateNumericLimit(
                            paramright.Name + " (" + paramright.DeviceType.ToString() + ")",
                            0,
                            0,
                            Convert.ToDouble(paramright.Value),
                            compare.Parameter.Units,
                            CompOperator.LOG,
                            step.Status
                        );
                        step.NumericLimits.Add(numericlimitcompared);
                    }

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
                    double parametervalue = (compareWithTolerance.IsUseParamFactor == true) ? Convert.ToDouble(compareWithTolerance.Parameter.Value) * compareWithTolerance.ParamFactor : Convert.ToDouble(compareWithTolerance.Parameter.Value);
                    string numericStatus = step.Status;
                    if (step.Status == StatusCodes.Error)
                        numericStatus = StatusCodes.Failed;

                    if (compareWithTolerance.CompareValue is string fixedvalue)
                    {
                        double lowerToleranceValue = CalculateLowerToleranceValue(Convert.ToDouble(fixedvalue), compareWithTolerance);
                        double upperToleranceValue = CalculateUpperToleranceValue(Convert.ToDouble(fixedvalue), compareWithTolerance);

                        step.StepType = StepTypes.ET_NLT;
                        NumericLimit numericlimit = CreateNumericLimit(
                            compareWithTolerance.Parameter.Name + " (" +compareWithTolerance.Parameter.DeviceType.ToString() + ")",
                            lowerToleranceValue,
                            upperToleranceValue,
                            parametervalue,
                            compareWithTolerance.Parameter.Units,
                            CompOperator.GELE,
                            numericStatus
                        );
                        step.NumericLimits.Add(numericlimit);
                    }
                    else if (compareWithTolerance.CompareValue is DeviceParameterData param)
                    {

                        double lowerToleranceValue = CalculateLowerToleranceValue(Convert.ToDouble(param.Value), compareWithTolerance);
                        double upperToleranceValue = CalculateUpperToleranceValue(Convert.ToDouble(param.Value), compareWithTolerance);

                        step.StepType = StepTypes.ET_MNLT;
                        NumericLimit numericlimitparam = CreateNumericLimit(
                            compareWithTolerance.Parameter.Name + " (" + compareWithTolerance.Parameter.DeviceType.ToString() + ")",
                            lowerToleranceValue,
                            upperToleranceValue,
                            parametervalue,
                            compareWithTolerance.Parameter.Units,
                            CompOperator.GELE,
                            numericStatus
                        );
                        step.NumericLimits.Add(numericlimitparam);
                        NumericLimit numericlimitcompared = CreateNumericLimit(
                            param.Name + " (" + param.DeviceType.ToString() + ")",
                            0,
                            0,
                            Convert.ToDouble(param.Value),
                            compareWithTolerance.Parameter.Units,
                            CompOperator.LOG,
                            numericStatus
                        );
                        step.NumericLimits.Add(numericlimitcompared);
                    }
                    else
                    {
                        double lowerToleranceValue = CalculateLowerToleranceValue(Convert.ToDouble(compareWithTolerance.CompareValue), compareWithTolerance);
                        double upperToleranceValue = CalculateUpperToleranceValue(Convert.ToDouble(compareWithTolerance.CompareValue), compareWithTolerance);

                        step.StepType = StepTypes.ET_NLT;
                        NumericLimit numericlimit = CreateNumericLimit(
                            compareWithTolerance.Parameter.Name + " (" + compareWithTolerance.Parameter.DeviceType.ToString() + ")",
                            lowerToleranceValue,
                            upperToleranceValue,
                            parametervalue,
                            compareWithTolerance.Parameter.Units,
                            CompOperator.GELE,
                            numericStatus
                        );
                        step.NumericLimits.Add(numericlimit);
                    }
                }

                //if (!compareWithTolerance.IsPass && compareWithTolerance.IsExecuted )
                //{
                //    step.StepCausedUUTFailure = 1;
                //    if (compareWithTolerance.IsError)
                //    {
                //        step.StepErrorMessage = compareWithTolerance.ErrorMessage;
                //        step.StepErrorCode = 1;
                //        step.StepErrorCodeFormat = "Error";
                //    }
                //}
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

        private string ReturnStatus(bool ispass , bool isexecuted , bool? iserror)
        {           
            return ((ispass == true && isexecuted == true) ? StatusCodes.Passed : (isexecuted == true) ? ((iserror == true) ? StatusCodes.Error : StatusCodes.Failed ): StatusCodes.Skipped);
        }

        private double CalculateLowerToleranceValue(double fixedValue, ScriptStepCompareWithTolerance compareWithTolerance)
        {
            double value = Convert.ToDouble(fixedValue);
            if (compareWithTolerance.IsUseCompareValueFactor)
            {
                value *= compareWithTolerance.CompareValueFactor;
            }

            return compareWithTolerance.IsPercentageTolerance
                ? value - (compareWithTolerance.Tolerance/100 * value)
                : value - compareWithTolerance.Tolerance;
        }

        private double CalculateUpperToleranceValue(double fixedValue, ScriptStepCompareWithTolerance compareWithTolerance)
        {
            double value = Convert.ToDouble(fixedValue);
            if (compareWithTolerance.IsUseCompareValueFactor)
            {
                value *= compareWithTolerance.CompareValueFactor;
            }

            return compareWithTolerance.IsPercentageTolerance
                ? value + (compareWithTolerance.Tolerance/100 * value)
                : value + compareWithTolerance.Tolerance;
        }


    }

}
