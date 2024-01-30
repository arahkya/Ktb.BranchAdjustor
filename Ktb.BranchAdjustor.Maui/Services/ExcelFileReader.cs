using System.Data;
using System.Text;
using ExcelDataReader;

namespace Ktb.BranchAdjustor.Maui.Services
{
    public class ExcelFileReader
    {
        private readonly string excelFilePath;

        private readonly string sheetName;

        public ExcelFileReader(string excelFilePath, string sheetName)
        {
            this.excelFilePath = excelFilePath;
            this.sheetName = sheetName;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public DataTable Read()
        {
            using FileStream excelFileStream = File.OpenRead(excelFilePath);
            using IExcelDataReader excelReader = ExcelReaderFactory.CreateReader(excelFileStream);

            ExcelDataSetConfiguration excelReaderConfig = new()
            {
                ConfigureDataTable = (cfg) => new ExcelDataTableConfiguration { UseHeaderRow = true }
            };

            DataSet dataSet = excelReader.AsDataSet(excelReaderConfig);   
            DataTable dataTable = dataSet.Tables[sheetName] ?? throw new Exception("Read Excel file sheet");

            return dataTable;
        }
    }
}