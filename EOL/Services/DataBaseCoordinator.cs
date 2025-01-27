using AutoMapper;
using EOL.Models;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using ScriptHandler.Models;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.util;
using TestersDB_Lib;
using TestersDB_Lib.Models;
namespace EOL.Services
{
    public class DataBaseCoordinator
    {
        private readonly DatabaseHandler DatabaseHandler;

        private List<TestersDB_Lib.Models.TestResult> testResultList = new List<TestersDB_Lib.Models.TestResult>();
        private List<TestDescription> testDescriptionList = new List<TestDescription>();
        private TestRun testRun = new TestRun();
        private UUT uut = new UUT();
        private TesterConfig testerConfig = new TesterConfig();
        private BOM Bom = new BOM();

        public IMapper _mapper { get; set; }


        public DataBaseCoordinator(DatabaseHandler databaseHandler, IMapper mapper)
        {
            _mapper = mapper;
            DatabaseHandler = databaseHandler;
        }

        public async Task SaveRunResultToDatabase(RunResult singleTestResult)
        {
            RunResult runresult = new RunResult();
            ResetOrmVariables();
            runresult = singleTestResult;
            int index = 0;
            foreach (EOLStepSummeryData step in runresult.Steps)
            {
                var mappedTestDescription = _mapper.Map<TestDescription>(step);
                if (mappedTestDescription == null)
                {
                    DatabaseHandler.LogMessage("Failed to map TestDescription\r\n");
                    return;
                }
                testDescriptionList.Add(mappedTestDescription);

                var mappedTestResult = _mapper.Map<TestersDB_Lib.Models.TestResult>(step);
                testResultList.Add(mappedTestResult);

                testResultList[index].TestDescriptionID = testDescriptionList[index].TestDescriptionID;
                index++;
            }
            testRun = _mapper.Map<TestRun>(runresult);
            uut = _mapper.Map<UUT>(runresult);
            testerConfig = _mapper.Map<TesterConfig>(runresult);

            AddUnkownData();

            if (!ValidateData())
            {
                DatabaseHandler.LogMessage("Failed to validate data");
                return;
            }
           
           await SaveToDatabase();
        }

        private void AddUnkownData()
        {
            Bom = new BOM
            {
                Part_Number = "PN-12345_eol",
                Description = "High-Performance Widget_eol",
                Manufacturer = "WidgetCorp_eol"
            };

            uut.UUT_Type = "HD";

            testerConfig.Station = "EOL";
            testerConfig.SequenceVer = "v1.0_eol";
            testerConfig.SequenceID = 10;
            testerConfig.Package = "temp_eol";

            testRun.SequenceID = 10;
        }

        public async Task SaveToDatabase()
        {
            Bom = await Task.Run(() => DatabaseHandler.AddOrGetEntity(Bom, B => B.Part_Number == Bom.Part_Number)); // Assuming Serial_Number is unique for UUT

            uut.BOM_ID = Bom.BOM_ID;
            uut = await Task.Run(() => DatabaseHandler.AddOrGetEntity(uut, u => u.Serial_Number == uut.Serial_Number)); // Assuming Serial_Number is unique for UUT

            testerConfig = await Task.Run(() => DatabaseHandler.AddOrGetEntity(testerConfig, tc => tc.ID == testerConfig.ID, false)); // Assuming ID is unique for TesterConfig

            var updatedTestDescriptions = new List<TestDescription>();
            // Save TestDescriptions
            foreach (TestDescription testDesc in testDescriptionList)
            {               
                // Check if the TestDescription exists based on the TestDescriptionID
                Expression<Func<TestDescription, bool>> existsPredicate = td => td.TestDescriptionID.Trim() == testDesc.TestDescriptionID;
                var existingentity = await Task.Run(() => DatabaseHandler.AddOrGetEntity(testDesc, existsPredicate));
                updatedTestDescriptions.Add(existingentity);
            }
            testDescriptionList = updatedTestDescriptions;

            testRun.UUT_ID = uut.id;
            testRun.TesterConfigID = testerConfig.ID;
            testRun = await Task.Run(() => DatabaseHandler.AddOrGetEntity(testRun, tr => tr.ID != 0 && tr.ID == testRun.ID ,false)); // Adjust this based on your key or relevant field


            // Save TestResults
            foreach (TestersDB_Lib.Models.TestResult testResult in testResultList)
            {
               
                testResult.RunID = testRun.ID; 

                // Check if the TestResult exists based on the TestDescriptionID (or other relevant fields)
                Expression<Func<TestersDB_Lib.Models.TestResult, bool>> existsPredicate = td => td.TestDescriptionID.Trim().Equals(testResult.TestDescriptionID.Trim(), StringComparison.OrdinalIgnoreCase);
                await Task.Run(() => DatabaseHandler.AddOrGetEntity(testResult, existsPredicate,false));
            }

            DatabaseHandler.LogMessage("Finished writing to DB ");


            //// Save TestRun
            //DatabaseHandler.AddOrGetEntity(testRun, tr => tr.ID == testRun.ID); // Adjust this based on your key or relevant field


        }

        private void ResetOrmVariables()
        {
            testResultList = new List<TestersDB_Lib.Models.TestResult>();
            testDescriptionList = new List<TestDescription>();
            testRun = new TestRun();
            uut = new UUT();
            testerConfig = new TesterConfig();
            Bom = new BOM();
        }

        private bool ValidateData()
        {
            // Validate TestRun
            if (testRun == null || string.IsNullOrEmpty(testRun.TestResult))
            {
                DatabaseHandler.LogMessage($"Failed to validate data of: {testRun}");
                return false;
            }

            // Validate UUT
            if (uut == null ||  string.IsNullOrEmpty(uut.Serial_Number))
            {
                DatabaseHandler.LogMessage($"Failed to validate data of: {uut}");
                return false;
            }

            // Validate TesterConfig
            if (testerConfig == null )
            {
                DatabaseHandler.LogMessage($"Failed to validate data of: {testerConfig}");
                return false;
            }

            // Validate BOM
            if (Bom == null || string.IsNullOrEmpty(Bom.Part_Number))
            {
                DatabaseHandler.LogMessage($"Failed to validate data of: {Bom}");
                return false;
            }

            // Validate TestDescriptions
            foreach (var testDesc in testDescriptionList)
            {
                if (testDesc == null || string.IsNullOrEmpty(testDesc.TestDescriptionID) || string.IsNullOrEmpty(testDesc.Test) 
                    || string.IsNullOrEmpty(testDesc.SubScript) || string.IsNullOrEmpty(testDesc.StepName)
                    || string.IsNullOrEmpty(testDesc.ReferenceDevice) || string.IsNullOrEmpty(testDesc.Method))
                {
                    DatabaseHandler.LogMessage($"Failed to validate data of: {testDesc}");
                    testDescriptionList.Remove(testDesc);
                }
            }

            // Validate TestResults
            foreach (var testResult in testResultList)
            {
                if (testResult == null || string.IsNullOrEmpty(testResult.TestDescriptionID) || string.IsNullOrEmpty(testResult.Result))
                {
                    DatabaseHandler.LogMessage($"Failed to validate data of: {testResult}");
                    testResultList.Remove(testResult);
                }
            }

            return true;
        }
    }
}
