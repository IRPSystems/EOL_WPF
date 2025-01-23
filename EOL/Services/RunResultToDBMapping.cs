using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using TestersDB_Lib;
using EOL.ViewModels;
using EOL.Models;
using TestersDB_Lib.Models;
using TestResult = TestersDB_Lib.Models.TestResult;
using ScriptHandler.Models;
using System.Security.Cryptography;


namespace EOL.Services
{
    public class RunResultToDBMapping : Profile
    {
        public RunResultToDBMapping() 
        {
            CreateMap<EOLStepSummeryData, TestResult>()
                .ForMember(TestResult => TestResult.Result, opt => opt.MapFrom(EOLStepSummeryData => (EOLStepSummeryData.IsPass ? "Passed" : "Failed")))
                .ForMember(TestResult => TestResult.ErrorMsg, opt => opt.MapFrom(EOLStepSummeryData => EOLStepSummeryData.IsPass ? "None" : EOLStepSummeryData.ErrorDescription))
                .ForMember(TestResult => TestResult.TestMeasurement, opt => opt.MapFrom(EOLStepSummeryData => EOLStepSummeryData.TestValue))
                .ForMember(TestResult => TestResult.ReferenceMeasurement, opt => opt.MapFrom(EOLStepSummeryData => !EOLStepSummeryData.IsDynParam ? null : EOLStepSummeryData.ComparisonValue));


            CreateMap<EOLStepSummeryData, TestDescription>()
                .ForMember(TestDescription => TestDescription.Test, opt => opt.MapFrom(EOLStepSummeryData => EOLStepSummeryData.Step.TestName))
                .ForMember(TestDescription => TestDescription.SubScript, opt => opt.MapFrom(EOLStepSummeryData => EOLStepSummeryData.Step.SubScriptName))
                .ForMember(TestDescription => TestDescription.StepName, opt => opt.MapFrom(EOLStepSummeryData => string.IsNullOrEmpty(EOLStepSummeryData.Step.UserTitle) ? "None" : EOLStepSummeryData.Step.UserTitle))
                .ForMember(TestDescription => TestDescription.Units, opt => opt.MapFrom(EOLStepSummeryData => string.IsNullOrEmpty(EOLStepSummeryData.Units) ? "None" : EOLStepSummeryData.Units))
                .ForMember(TestDescription => TestDescription.Method, opt => opt.MapFrom(EOLStepSummeryData => EOLStepSummeryData.Method))
                .ForMember(TestDescription => TestDescription.CompareFixedValue, opt => opt.MapFrom(EOLStepSummeryData => (EOLStepSummeryData.IsDynParam ?  null : EOLStepSummeryData.ComparisonValue)))
                .ForMember(TestDescription => TestDescription.Min_Value, opt => opt.MapFrom(EOLStepSummeryData => EOLStepSummeryData.MinVal))
                .ForMember(TestDescription => TestDescription.Max_Value, opt => opt.MapFrom(EOLStepSummeryData => EOLStepSummeryData.MaxVal))
                .ForMember(TestDescription => TestDescription.Tolerance, opt => opt.MapFrom(EOLStepSummeryData => EOLStepSummeryData.MeasuredTolerance))
                .ForMember(TestDescription => TestDescription.ReferenceDevice, opt => opt.MapFrom(EOLStepSummeryData => EOLStepSummeryData.Reference))
                .ForMember(TestDescription => TestDescription.TestDescriptionID, opt => opt.MapFrom(EOLStepSummeryData => new CustomValueResolver().Resolve(
                (EOLStepSummeryData.Step.TestName ?? string.Empty) +
                (EOLStepSummeryData.Step.SubScriptName ?? string.Empty) +
                (EOLStepSummeryData.Step.UserTitle ?? string.Empty).Replace(" ", ""), null , null, null).Trim()));

            CreateMap<RunResult, TestRun>()
                .ForMember(TestRun => TestRun.Operator_Name, opt => opt.MapFrom(RunResult => RunResult.OperatorName))
                .ForMember(TestRun => TestRun.TestResult, opt => opt.MapFrom(RunResult => RunResult.TestStatus))
                .ForMember(TestRun => TestRun.StationID, opt => opt.MapFrom(RunResult => RunResult.RackNumber))
                .ForMember(TestRun => TestRun.FailReason, opt => opt.MapFrom(RunResult => string.IsNullOrEmpty(RunResult.FailedStep.OperatorErrorDescription) ? "None" : RunResult.FailedStep.OperatorErrorDescription));
            //.ForMember(TestRun => TestRun.Result, opt => opt.MapFrom(RunResult => RunResult.TestStatus));



            CreateMap<RunResult, UUT>()
                .ForMember(UUT => UUT.Part_Number, opt => opt.MapFrom(RunResult => RunResult.PartNumber))
                .ForMember(UUT => UUT.Serial_Number, opt => opt.MapFrom(RunResult => RunResult.SerialNumber));


            CreateMap<RunResult, TesterConfig>()
                .ForMember(TesterConfig => TesterConfig.Station, opt => opt.MapFrom(RunResult => RunResult.RackNumber));


        }
    }

    public class CustomValueResolver : IValueResolver<string, TestResult, string>
    {
        public string Resolve(string source, TestResult destination, string destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source))
                return null;

            // Generate a hash from the input string
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(source));
                return Convert.ToBase64String(hashBytes); // Return hash as Base64 string
            }
        }
    }
}
