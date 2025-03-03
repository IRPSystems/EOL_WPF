using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace WatsReportModels
{

    [XmlRoot("Reports", Namespace = "http://wats.virinco.com/schemas/WATS/Report/wsxf")]
    public class Reports
    {
        [XmlElement("Report")]
        public Report Report { get; set; }
    }

    [XmlType(Namespace = "http://wats.virinco.com/schemas/WATS/Report/wsxf")]
    public class Report
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("Start")]
        public string Start { get; set; }

        [XmlAttribute("Start_utc")]
        public string StartUtc { get; set; }

        [XmlAttribute("Result")]
        public string Result { get; set; }

        [XmlAttribute("SN")]
        public string SerialNumber { get; set; }

        [XmlAttribute("PN")]
        public string PartNumber { get; set; }

        [XmlAttribute("Rev")]
        public string Revision { get; set; }

        [XmlAttribute("MachineName")]
        public string MachineName { get; set; }

        [XmlAttribute("Location")]
        public string Location { get; set; }

        [XmlAttribute("Purpose")]
        public string Purpose { get; set; }

        [XmlElement("UUT")]
        public UUT UUT { get; set; }

        [XmlElement("Process")]
        public Process Process { get; set; }

        //[XmlElement("MiscInfo")]
        //public List<MiscInfo> MiscInfo { get; set; }

        //[XmlElement("ReportUnitHierarchy")]
        //public List<ReportUnitHierarchy> ReportUnitHierarchy { get; set; }

        [XmlElement("Step")]
        public List<Step> Steps { get; set; }
    }

    public class UUT
    {
        [XmlAttribute("UserLoginName")]
        public string UserLoginName { get; set; }

        //[XmlAttribute("BatchSN")]
        //public string BatchSerialNumber { get; set; }

        //[XmlAttribute("TestSocketIndex")]
        //public int TestSocketIndex { get; set; }

        [XmlAttribute("ExecutionTime")]
        public double ExecutionTime { get; set; }

        //[XmlAttribute("FixtureId")]
        //public string FixtureId { get; set; }

        //[XmlAttribute("ErrorCode")]
        //public int ErrorCode { get; set; }

        [XmlAttribute("ErrorMessage")]
        public string ErrorMessage { get; set; }

        //[XmlElement("Comment")]
        //public string Comment { get; set; }
    }

    public class Process
    {
        [XmlAttribute("Code")]
        public string Code { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }
    }

    //public class MiscInfo
    //{
    //    [XmlAttribute("Typedef")]
    //    public string Typedef { get; set; }

    //    [XmlAttribute("Description")]
    //    public string Description { get; set; }

    //    [XmlAttribute("Numeric")]
    //    public double Numeric { get; set; }

    //    [XmlText]
    //    public string Text { get; set; }
    //}

    //public class ReportUnitHierarchy
    //{
    //    [XmlAttribute("PartType")]
    //    public string PartType { get; set; }

    //    [XmlAttribute("PN")]
    //    public string PartNumber { get; set; }

    //    [XmlAttribute("SN")]
    //    public string SerialNumber { get; set; }

    //    [XmlAttribute("Rev")]
    //    public string Revision { get; set; }
    //}

    public class Step
    {
        [XmlAttribute("Group")]
        public string Group { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("StepType")]
        public string StepType { get; set; }

        [XmlAttribute("Status")]
        public string Status { get; set; }

        [XmlAttribute("total_time")]
        public double TotalTime { get; set; }

        [XmlAttribute("StepCausedUUTFailure")]
        public int StepCausedUUTFailure { get; set; }

        [XmlElement("ReportText")]
        public string ReportText { get; set; }

        [XmlElement("SequenceCall")]
        public SequenceCall Sequencecall { get; set; }

        [XmlElement("NumericLimit")]
        public List<NumericLimit> NumericLimits { get; set; }

        [XmlElement("PassFail")]
        public List<PassFail> PassFails { get; set; }

        [XmlElement("StringValue")]
        public List<StringValue> StringValues { get; set; }

        [XmlElement("Chart")]
        public List<Chart> Charts { get; set; }

        [XmlElement("Attachment")]
        public List<Attachment> Attachments { get; set; }

        [XmlElement("Step")]
        public List<Step> Steps { get; set; }
    }

    public class SequenceCall
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }

        [XmlAttribute("Filename")]
        public string Filename { get; set; }

        [XmlAttribute("Filepath")]
        public string Filepath { get; set; }
    }

    public class NumericLimit
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("NumericValue")]
        public double NumericValue { get; set; }

        [XmlAttribute("LowLimit")]
        public double LowLimit { get; set; }

        [XmlAttribute("HighLimit")]
        public double HighLimit { get; set; }

        [XmlAttribute("Units")]
        public string Units { get; set; }

        [XmlAttribute("CompOperator")]
        public string CompOperator { get; set; }

        [XmlAttribute("Status")]
        public string Status { get; set; }
    }

    public class PassFail
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Status")]
        public string Status { get; set; }
    }

    public class StringValue
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("CompOperator")]
        public string CompOperator { get; set; }

        [XmlAttribute("StringLimit")]
        public string StringLimit { get; set; }

        [XmlAttribute("StringValue")]
        public string Value { get; set; }

        [XmlAttribute("Status")]
        public string Status { get; set; }
    }

    public class Chart
    {
        [XmlAttribute("idx")]
        public int Index { get; set; }

        [XmlAttribute("ChartType")]
        public string ChartType { get; set; }

        [XmlAttribute("Label")]
        public string Label { get; set; }

        [XmlAttribute("XLabel")]
        public string XLabel { get; set; }

        [XmlAttribute("XUnit")]
        public string XUnit { get; set; }

        [XmlAttribute("YLabel")]
        public string YLabel { get; set; }

        [XmlAttribute("YUnit")]
        public string YUnit { get; set; }

        [XmlElement("Series")]
        public List<Series> Series { get; set; }
    }

    public class Series
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("DataType")]
        public string DataType { get; set; }

        [XmlElement("ydata")]
        public string YData { get; set; }

        [XmlElement("xdata")]
        public string XData { get; set; }
    }

    public class Attachment
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("ContentType")]
        public string ContentType { get; set; }

        [XmlAttribute("Size")]
        public int Size { get; set; }

        [XmlText]
        public string Base64Data { get; set; }
    }
}