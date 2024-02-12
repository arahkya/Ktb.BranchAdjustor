using System.Data;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;
using ExcelDataReader;
using Ktb.BranchAdjustor.Maui.Models;

namespace Ktb.BranchAdjustor.Maui.Services
{
    public class ExcelFileReader : IRecipient<FileResult>
    {
        private readonly ExcelSheetConfigurationOption excelSheetConfigurationOption;

        public ExcelFileReader(ExcelSheetConfigurationOption excelSheetConfigurationOption)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            this.excelSheetConfigurationOption = excelSheetConfigurationOption;
        }

        public void Receive(FileResult message)
        {
            string excelFilePath = message.FullPath;

            WeakReferenceMessenger.Default.Send(new ApplicationStateModel
            {
                Status = "Read Excel File"
            });

            using FileStream excelFileStream = File.OpenRead(excelFilePath);
            using IExcelDataReader excelReader = ExcelReaderFactory.CreateReader(excelFileStream);

            ExcelDataSetConfiguration excelReaderConfig = new()
            {
                ConfigureDataTable = (cfg) => new ExcelDataTableConfiguration { UseHeaderRow = true }
            };

            DataSet dataSet = excelReader.AsDataSet(excelReaderConfig);
            DataTable dataTable = dataSet.Tables[excelSheetConfigurationOption.SheetName] ?? throw new Exception("Read Excel file sheet");

            WeakReferenceMessenger.Default.Send(dataTable);
        }

        public async Task<DataTable> ReadAsync(string excelFilePath)
        {
            return await Task.Factory.StartNew(() =>
            {
                WeakReferenceMessenger.Default.Send(new ApplicationStateModel
                {
                    Status = "Read Excel File"
                });

                using FileStream excelFileStream = File.OpenRead(excelFilePath);
                using IExcelDataReader excelReader = ExcelReaderFactory.CreateReader(excelFileStream);

                ExcelDataSetConfiguration excelReaderConfig = new()
                {
                    ConfigureDataTable = (cfg) => new ExcelDataTableConfiguration { UseHeaderRow = true }
                };

                DataSet dataSet = excelReader.AsDataSet(excelReaderConfig);
                DataTable dataTable = dataSet.Tables[excelSheetConfigurationOption.SheetName] ?? throw new Exception("Read Excel file sheet");

                return dataTable;
            });
        }
    }
}