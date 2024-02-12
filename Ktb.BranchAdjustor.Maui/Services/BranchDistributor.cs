using CommunityToolkit.Mvvm.Messaging;
using Ktb.BranchAdjustor.Maui.Models;

namespace Ktb.BranchAdjustor.Maui.Services
{
    public class BranchDistributor
    {
        private int CalculateBranchEnd(int branchStart, IEnumerable<DisputeEntity> disputes, int totalWorker, Range branchRange)
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

                WeakReferenceMessenger.Default.Send(new ApplicationStateModel
                {
                    Status = message,
                    Progress = disputeCount / disputes.Count()
                });
            }

            return adjustBranchEnd;
        }

        public async Task DistributeAsync(IEnumerable<DisputeEntity> disputes, int totalWorker, Range branchRange)
        {
            await Task.Factory.StartNew(() =>
            {
                WeakReferenceMessenger.Default.Send(new ApplicationStateModel
                {
                    Status = "Distribut Branch by Dispute",
                    Progress = 0
                });

                BranchDistributedEntity[] branchDistributedEntities = new BranchDistributedEntity[totalWorker];

                for (int j = 0; j < totalWorker; j++)
                {
                    int branchStart = (j == 0) ? branchRange.Start.Value : branchDistributedEntities[j - 1].BranchEnd + 1;
                    int branchEnd = CalculateBranchEnd(branchStart, disputes, totalWorker, branchRange);

                    BranchDistributedEntity branchDistributedEntity = new(disputes, branchStart, branchEnd, branchRange.End.Value);

                    branchDistributedEntities[j] = branchDistributedEntity;

                    WeakReferenceMessenger.Default.Send(branchDistributedEntity);
                }

                WeakReferenceMessenger.Default.Send(new ApplicationStateModel
                {
                    Status = "Done",
                    Progress = 0
                });
            });
        }
    }
}