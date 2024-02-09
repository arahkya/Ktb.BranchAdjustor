using Ktb.BranchAdjustor.Maui.Models;

namespace Ktb.BranchAdjustor.Maui.Services
{
    public class BranchDistributor
    {
        private readonly IEnumerable<DisputeEntity> disputeEntities;
        private readonly Range branchRange;
        private readonly int totalWorker;

        public delegate void OnProgressChanged(decimal progress, string message);

        public event OnProgressChanged? ProgressChanged;

        public BranchDistributor(IEnumerable<DisputeEntity> disputeEntities, Range branchRange, int totalWorker)
        {
            this.disputeEntities = disputeEntities;
            this.branchRange = branchRange;
            this.totalWorker = totalWorker;
        }

        public IEnumerable<BranchDistributedEntity> DistributeByBranch(CancellationToken cancellationToken)
        {
            BranchDistributedEntity[] branchDistributedEntities = new BranchDistributedEntity[totalWorker];

            for (int j = 0; j < totalWorker; j++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                int branchStart = (j == 0) ? branchRange.Start.Value : branchDistributedEntities[j - 1].BranchEnd + 1;
                int branchEnd = CalculateBranchEnd(branchStart, cancellationToken);

                BranchDistributedEntity branchDistributedEntity = new(disputeEntities, branchStart, branchEnd, branchRange.End.Value);

                branchDistributedEntities[j] = branchDistributedEntity;

                yield return branchDistributedEntity;
            }
        }

        private int CalculateBranchEnd(int branchStart, CancellationToken cancellationToken)
        {
            int adjustBranchStart = branchStart;
            int adjustBranchEnd = adjustBranchStart + 1;
            int disputeCount = 0;
            int disputeTotal = disputeEntities.Count();
            int avgDisputeForWorker = disputeTotal / totalWorker;

            while (disputeCount < avgDisputeForWorker && !cancellationToken.IsCancellationRequested)
            {
                if (adjustBranchEnd == branchRange.End.Value) break;

                disputeCount = disputeEntities.Count(p => p.BranchNumber >= adjustBranchStart && p.BranchNumber <= adjustBranchEnd);

                adjustBranchEnd++;

                string message = $"BranchStart: {adjustBranchStart}, BranchEnd: {adjustBranchEnd}, Dispute: {disputeCount}";
                ProgressChanged?.Invoke(Convert.ToDecimal(adjustBranchEnd) / Convert.ToDecimal(branchRange.End.Value), message);

                System.Diagnostics.Debug.WriteLine($"BranchStart: {adjustBranchStart}, BranchEnd: {adjustBranchEnd}, Dispute: {disputeCount}");
            }

            return adjustBranchEnd;
        }
    }
}