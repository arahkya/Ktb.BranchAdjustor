using Ktb.BranchAdjustor.Maui.Entities;

namespace Ktb.BranchAdjustor.Maui.Services
{
    public class BranchDistributor
    {
        private readonly IEnumerable<DisputeEntity> disputeEntities;
        private readonly Range branchRange;
        private readonly int totalWorker;

        public BranchDistributor(IEnumerable<DisputeEntity> disputeEntities, Range branchRange, int totalWorker)
        {
            this.disputeEntities = disputeEntities;
            this.branchRange = branchRange;
            this.totalWorker = totalWorker;
        }

        public IEnumerable<BranchDistributedEntity> Distribute()
        {
            BranchDistributedEntity[] branchDistributedEntities = new BranchDistributedEntity[totalWorker];

            int avgBranchPerWorker = branchRange.End.Value / totalWorker;

            for (int j = 0; j < branchDistributedEntities.Length; j++)
            {
                int branchStart = (j == 0) ? branchRange.Start.Value : branchDistributedEntities[j - 1].BranchEnd + 1;
                int branchEnd = (j == (branchDistributedEntities.Length - 1)) ? branchRange.End.Value : branchStart + avgBranchPerWorker;
                
                branchDistributedEntities[j] = new()
                {
                    BranchStart = (j == 0) ? 0 : branchStart,
                    BranchEnd = branchEnd,
                    TotalDispute = disputeEntities.Count(p => p.BranchNumber >= branchStart && p.BranchNumber <= branchEnd)
                };
            }

            return branchDistributedEntities;
        }
    }
}