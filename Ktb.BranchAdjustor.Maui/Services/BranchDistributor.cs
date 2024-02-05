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

        public IEnumerable<BranchDistributedEntity> DistributeByBranch()
        {
            BranchDistributedEntity[] branchDistributedEntities = new BranchDistributedEntity[totalWorker];

            int avgBranchPerWorker = branchRange.End.Value / totalWorker;

            for (int j = 0; j < branchDistributedEntities.Length; j++)
            {
                int branchStart = (j == 0) ? branchRange.Start.Value : branchDistributedEntities[j - 1].BranchEnd + 1;
                // int branchEnd = (j == (branchDistributedEntities.Length - 1)) ? branchRange.End.Value : branchStart + avgBranchPerWorker;
                int branchEnd = (j == (branchDistributedEntities.Length - 1)) ? branchRange.End.Value : CalculateBranchEnd(branchStart);

                BranchDistributedEntity branchDistributedEntity = new(disputeEntities, branchStart, branchEnd, branchRange.End.Value);

                branchDistributedEntities[j] = branchDistributedEntity;
            }

            return branchDistributedEntities;
        }

        private int CalculateBranchEnd(int branchStart)
        {
            int adjustBranchStart = branchStart;
            int adjustBranchEnd = adjustBranchStart + 1;
            int disputeCount = 0;
            int avgDisputeForWorker = disputeEntities.Count() / totalWorker;

            while (disputeCount < avgDisputeForWorker)
            {
                disputeCount = disputeEntities.Count(p => p.BranchNumber >= adjustBranchStart && p.BranchNumber <= adjustBranchEnd);

                adjustBranchEnd++;

                System.Diagnostics.Debug.WriteLine($"BranchStart: {adjustBranchStart}, BranchEnd: {adjustBranchEnd}, Dispute: {disputeCount}");
            }

            return adjustBranchEnd;
        }
    }
}