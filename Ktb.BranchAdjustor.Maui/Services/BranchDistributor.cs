using CommunityToolkit.Mvvm.Messaging;
using Ktb.BranchAdjustor.Maui.Models;

namespace Ktb.BranchAdjustor.Maui.Services
{
    public class BranchDistributor : IRecipient<List<DisputeEntity>>, IRecipient<WorkerContextModel>, IRecipient<BranchContextModel>
    {
        private Range branchRange;
        private int totalWorker;

        public delegate void OnProgressChanged(decimal progress, string message);

        public event OnProgressChanged? ProgressChanged;

        private int CalculateBranchEnd(int branchStart, List<DisputeEntity> disputes)
        {
            int adjustBranchStart = branchStart;
            int adjustBranchEnd = adjustBranchStart + 1;
            int disputeCount = 0;
            int disputeTotal = disputes.Count();
            int avgDisputeForWorker = disputeTotal / totalWorker;

            while (disputeCount < avgDisputeForWorker)
            {
                if (adjustBranchEnd == branchRange.End.Value) break;

                disputeCount = disputes.Count(p => p.BranchNumber >= adjustBranchStart && p.BranchNumber <= adjustBranchEnd);

                adjustBranchEnd++;

                string message = $"BranchStart: {adjustBranchStart}, BranchEnd: {adjustBranchEnd}, Dispute: {disputeCount}";
                ProgressChanged?.Invoke(Convert.ToDecimal(adjustBranchEnd) / Convert.ToDecimal(branchRange.End.Value), message);

                System.Diagnostics.Debug.WriteLine($"BranchStart: {adjustBranchStart}, BranchEnd: {adjustBranchEnd}, Dispute: {disputeCount}");
            }

            return adjustBranchEnd;
        }

        public void Receive(List<DisputeEntity> disputes)
        {
            BranchDistributedEntity[] branchDistributedEntities = new BranchDistributedEntity[totalWorker];

            for (int j = 0; j < totalWorker; j++)
            {
                int branchStart = (j == 0) ? branchRange.Start.Value : branchDistributedEntities[j - 1].BranchEnd + 1;
                int branchEnd = CalculateBranchEnd(branchStart, disputes);

                BranchDistributedEntity branchDistributedEntity = new(disputes, branchStart, branchEnd, branchRange.End.Value);

                branchDistributedEntities[j] = branchDistributedEntity;

                WeakReferenceMessenger.Default.Send(branchDistributedEntity);
            }
        }

        public void Receive(WorkerContextModel context)
        {
            this.totalWorker = context.TotalWorker;
        }

        public void Receive(BranchContextModel branchRange)
        {
            this.branchRange = branchRange.BranchRange;
        }
    }
}