using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorting1514TestReports
{
    class Program
    {
        static void Main(string[] args)
        {

            TestReportOrganizer reports1 = new TestReportOrganizer();
            foreach (var item in reports1.TestReports)
            {
                var destRootFolder1 = item.destRootFolder;
                var destRootFolder2 = item.destRootFolder.Replace(@"C:\", @"E:\");

                if (!Directory.Exists(destRootFolder1))
                {
                    Directory.CreateDirectory(destRootFolder1);
                }

                var destPath1 = Path.Combine(destRootFolder1, item.fileName);
                File.Move(item.filePath, destPath1);
                Console.WriteLine(item.filePath + "\tgoing to " + destRootFolder1 + "\\" + item.fileName);
                
                if (!Directory.Exists(destRootFolder2))
                {
                    Directory.CreateDirectory(destRootFolder2);
                }

                var destPath2 = Path.Combine(destRootFolder2, item.fileName);
                File.Copy(destPath1, destPath2);
                Console.WriteLine(destPath1 + "\tgoing to " + destRootFolder2 + "\\" + item.fileName);
                Console.WriteLine("--");

            }



            //Console.WriteLine("\n-----------------------------------");

            //DateTime dDate;
            //string sDate = "2018-06-01";
            //bool dateParseSuccess = DateTime.TryParse(sDate, out dDate);
            //string sDate2 = String.Format("{0:M/d/yy}", dDate);
            //Console.WriteLine("date = " + sDate2);

            //string path1 = @"C:\TestReport";
            //string path2 = path1.Replace(@"C:\", @"D:\");
            //Console.WriteLine("path2 = " + path2);
        }
    }

    public class TestReportInfo
    {
        public string filePath;
        public string fileName;
        public string destRootFolder;
        public string status;

        public TestReportInfo()
        {
            filePath = "";
            fileName = "";
            destRootFolder = "";
            status = "FAIL";
        }
        public TestReportInfo(string inputFilePath, string inputFileName, string inputDestRootFolder, string inputStatus)
        {
            filePath = inputFilePath;
            fileName = inputFileName;
            destRootFolder = inputDestRootFolder;
            status = inputStatus;
        }
    }
    public class TestReportOrganizer
    {

        private string mRootFolder = @"C:\TestReports";
        private string mReportFileExtension = "*.html";
        private string[] mPartNumbers = { "1514102", "1514112" };
        private TestReportInfo[] mTestreports;
        private string mRegexDatePattern = @"(?<=-)([12]\d{3}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01]))";

        private bool IsValidReport(string inputReportFileName, out TestReportInfo testReportInfo)
        {
            bool reportIsValid = false;
            string sDate = "";
            string sDateMdyy = "";

            testReportInfo = new TestReportInfo();
            if (!IsValidPN(inputReportFileName)) return reportIsValid;

            sDate = ExtractDateFromFileName(inputReportFileName);
            if (sDate == "") return reportIsValid;

            sDateMdyy = ConvertDateToMdyy(sDate);
            if (sDateMdyy == "") return reportIsValid;

            testReportInfo.destRootFolder = mRootFolder + "\\" + sDateMdyy;
            testReportInfo.fileName = inputReportFileName;
            reportIsValid = true;

            return reportIsValid;
        }

        private bool IsValidPN(string inputReportFileName)
        {
            bool pnIsValid = false;

            foreach (var item in mPartNumbers)
            {
                if (inputReportFileName.Contains(item.Trim()))
                {
                    pnIsValid = true;
                    break;
                }
            }
            return pnIsValid;

        }
        private string ExtractDateFromFileName(string inputFileName)
        {
            string tmp = "";
            Match match = Regex.Match(inputFileName, mRegexDatePattern);
            if (match.Success)
            {
                tmp = match.Value;
            }

            string[] tmp1 = tmp.Split('-');
            if (tmp1.Length < 3) return "";
            int year = int.Parse(tmp1[0]);
            if (year < 2016 || year > 2030) tmp = "";

            return tmp;
        }

        private string ConvertDateToMdyy(string inputDate)
        {
            string tmp = "";
            DateTime dDate;
            if (DateTime.TryParse(inputDate, out dDate)) tmp = String.Format("{0:M/d/yy}", dDate);
            tmp = tmp.Replace('/', '.');
            return tmp;
        }

        private string GetReportStatus(string inputFilePath)
        {
            string tmp = "";
            string text = System.IO.File.ReadAllText(inputFilePath);
            if (text.Contains("FAIL"))
            {
                tmp = "FAIL";
                return tmp;
            }
            else if (text.Contains("PASS"))
            {
                tmp = "PASS";
                return tmp;
            }

            return tmp;

        }


        private bool IsFileinUse(string inputFilePath)
        {
            FileInfo file = new FileInfo(inputFilePath);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }

        private void CreateListOfValidReportFiles()
        {
            List<TestReportInfo> lstOfMatchingFiles = new List<TestReportInfo>();
            string[] tmp = Directory.GetFiles(mRootFolder, mReportFileExtension, SearchOption.TopDirectoryOnly);
            TestReportInfo testReportInfo = new TestReportInfo();

            foreach (var item in tmp)
            {
                if (IsValidReport(Path.GetFileName(item), out testReportInfo))
                {
                    testReportInfo.filePath = item;
                    testReportInfo.status = GetReportStatus(testReportInfo.filePath);
                    testReportInfo.destRootFolder = testReportInfo.destRootFolder + "\\" + testReportInfo.status;
                    if (testReportInfo.status != "")
                    {
                        if (!IsFileinUse(item))
                        {
                            lstOfMatchingFiles.Add(testReportInfo);
                        }
                    }
                }
            }
            mTestreports = lstOfMatchingFiles.ToArray();

        }
        public TestReportOrganizer()
        {
            CreateListOfValidReportFiles();
        }

        public TestReportOrganizer(string rootFolderForTestReports, string reportFileExtension, string[] validPartNumbers)
        {
            mRootFolder = rootFolderForTestReports;
            mReportFileExtension = reportFileExtension.Trim();
            mPartNumbers = validPartNumbers;
            CreateListOfValidReportFiles();
        }
        public TestReportInfo[] TestReports
        {
            get
            {

                return mTestreports;
            }
        }

    }
}
