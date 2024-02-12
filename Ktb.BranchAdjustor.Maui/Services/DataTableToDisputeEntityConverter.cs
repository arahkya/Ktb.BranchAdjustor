using System.Data;
using CommunityToolkit.Mvvm.Messaging;
using Ktb.BranchAdjustor.Maui.Models;

namespace Ktb.BranchAdjustor.Maui.Services
{
    public class DataTableToDisputeEntityConverter : IRecipient<DataTable>
    {
        private readonly ExcelSheetConfigurationOption excelSheetConfigurationOption;

        public DataTableToDisputeEntityConverter(ExcelSheetConfigurationOption excelSheetConfigurationOption)
        {
            this.excelSheetConfigurationOption = excelSheetConfigurationOption;
        }

        public void Receive(DataTable dataTable)
        {
            List<DisputeEntity> disputeEntities = [];

            foreach (DataRow dataRow in dataTable.Rows)
            {
                string branchCodeText = dataRow[excelSheetConfigurationOption.BranchColumnName].ToString() ?? string.Empty;
                string createDateText = dataRow[excelSheetConfigurationOption.CreateDateColumnName].ToString() ?? string.Empty;
                string terminalCode = dataRow[excelSheetConfigurationOption.TermIdColumnName].ToString() ?? string.Empty;

                if (!int.TryParse(branchCodeText, null, out int branchCode)) throw new Exception($"Cannot parse Branch Code to Integer ({branchCodeText})");

                if (!DateTime.TryParse(createDateText, null, out DateTime createDate)) throw new Exception($"Cannot parse CreateDate to DateTime ({createDateText})");

                disputeEntities.Add(new DisputeEntity(branchCode, createDate, terminalCode));
            }

            WeakReferenceMessenger.Default.Send(new ApplicationStateModel
            {
                Status = "Transform Raw Data to Dispute Info"
            });
            WeakReferenceMessenger.Default.Send(new BranchContextModel
            {
                BranchRange = new Range(0, disputeEntities.Max(p => p.BranchNumber))
            });
            WeakReferenceMessenger.Default.Send(disputeEntities);
        }

        public async Task<IEnumerable<DisputeEntity>> ConvertAsync(DataTable dataTable)
        {
            WeakReferenceMessenger.Default.Send(new ApplicationStateModel
            {
                Status = "Transform Raw Data to Dispute Info"
            });
            
            return await Task.Factory.StartNew(() =>
            {
                List<DisputeEntity> disputeEntities = [];

                foreach (DataRow dataRow in dataTable.Rows)
                {
                    string branchCodeText = dataRow[excelSheetConfigurationOption.BranchColumnName].ToString() ?? string.Empty;
                    string createDateText = dataRow[excelSheetConfigurationOption.CreateDateColumnName].ToString() ?? string.Empty;
                    string terminalCode = dataRow[excelSheetConfigurationOption.TermIdColumnName].ToString() ?? string.Empty;

                    if (!int.TryParse(branchCodeText, null, out int branchCode)) throw new Exception($"Cannot parse Branch Code to Integer ({branchCodeText})");

                    if (!DateTime.TryParse(createDateText, null, out DateTime createDate)) throw new Exception($"Cannot parse CreateDate to DateTime ({createDateText})");

                    disputeEntities.Add(new DisputeEntity(branchCode, createDate, terminalCode));
                }

                return disputeEntities;
            });
        }
    }
}