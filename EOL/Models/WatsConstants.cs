using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatsConstants
{
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
        public const string ET_SVT = "ET_SVT";
        public const string Attachment = "Attachment";
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
    public static class StatusCodes
    {
        public const string Passed = "Passed"; // Green
        public const string Failed = "Failed"; // Red
        public const string Skipped = "Skipped"; // Yellow
        public const string Error = "Error"; // Orange
        public const string Terminated = "Terminated"; // Blue
    }
}
