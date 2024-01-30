using System.Data;
using Ktb.BranchAdjustor.Maui.Entities;

namespace Ktb.BranchAdjustor.Maui.Services
{
    public class DataTableToDisputeEntityConverter
    {
        private readonly DataTable dataTable;
        private readonly string branchCodeColumnName;
        private readonly string createDateColumnName;
        private readonly string terminalCodeColumnName;

        public DataTableToDisputeEntityConverter(DataTable dataTable, string branchCodeColumnName, string createDateColumnName, string terminalCodeColumnName)
        {
            this.dataTable = dataTable;
            this.branchCodeColumnName = branchCodeColumnName;
            this.createDateColumnName = createDateColumnName;
            this.terminalCodeColumnName = terminalCodeColumnName;
        }

        public IEnumerable<DisputeEntity> Convert()
        {
            foreach (DataRow dataRow in dataTable.Rows)
            {
                string branchCodeText = dataRow[branchCodeColumnName].ToString() ?? string.Empty;
                string createDateText = dataRow[createDateColumnName].ToString() ?? string.Empty;
                string terminalCode = dataRow[terminalCodeColumnName].ToString() ?? string.Empty;

                if (!int.TryParse(branchCodeText, null, out int branchCode)) throw new Exception($"Cannot parse Branch Code to Integer ({branchCodeText})");

                if (!DateTime.TryParse(createDateText, null, out DateTime createDate)) throw new Exception($"Cannot parse CreateDate to DateTime ({createDateText})");

                yield return new DisputeEntity(branchCode, createDate, terminalCode);
            }
        }
    }
}