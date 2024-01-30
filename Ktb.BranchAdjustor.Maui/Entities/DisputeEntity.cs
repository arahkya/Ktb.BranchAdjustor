namespace Ktb.BranchAdjustor.Maui.Entities
{
    public class DisputeEntity
    {
        public int BranchNumber { get; init; }

        public DateTime CreateDate { get; init; }

        public string TerminalCode { get; init; } = string.Empty;

        public DisputeEntity(int branchNumber, DateTime createDate, string terminalCode)
        {
            BranchNumber = branchNumber;
            CreateDate = createDate;
            TerminalCode = terminalCode;
        }        
    }
}