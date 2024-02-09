namespace Ktb.BranchAdjustor.Maui
{
    public class ExcelSheetConfigurationOption 
    {
        public string SheetName { get; set; } = "DisputeATM";
        public string CreateDateColumnName { get; set; } = "CREATE_DATE";
        public string TermIdColumnName { get; set; } = "TERM_ID";
        public string BranchColumnName { get; set; } = "Branch";
    }
}