using System.ComponentModel;
using System.Windows.Input;
using Ktb.BranchAdjustor.Maui.Models;

namespace Ktb.BranchAdjustor.Maui.Entities
{
    public class BranchDistributedEntity : INotifyPropertyChanged
    {
        public int Index { get; set; }

        public int MaxBranchLimit { get; set; }

        private int branchStart;

        public int BranchStart
        {
            get => branchStart;
            set
            {
                branchStart = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BranchStart)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBranch)));
            }
        }

        private int branchEnd;

        public int BranchEnd
        {
            get => branchEnd;
            set
            {
                bool isIncrease = branchEnd < value;

                branchEnd = value;

                int currentTotalDispute = totalDispute;
                Recalculate();

                while (TotalDispute == currentTotalDispute)
                {
                    if (isIncrease)
                    {
                        branchEnd++;
                    }
                    else
                    {
                        branchEnd--;
                    }
                    Recalculate();
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BranchEnd)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBranch)));
            }
        }

        private int totalDispute;

        public int TotalDispute
        {
            get => totalDispute;
            set
            {
                totalDispute = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalDispute)));
            }
        }

        public int TotalBranch => BranchEnd - BranchStart;

        public delegate void BranchAdjustHandler(ChangeBranchContextModel changeBranchContextModel);

        public BranchAdjustHandler? BranchAdjust;
        private readonly IEnumerable<DisputeEntity> disputes;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand AdjustBranchCommand { get; set; }

        public BranchDistributedEntity(IEnumerable<DisputeEntity> disputes)
        {
            AdjustBranchCommand = new Command(AdjustBranchCommandHandler);
            this.disputes = disputes;
        }

        private void AdjustBranchCommandHandler(object args)
        {
            ChangeBranchContextModel changeContext = (ChangeBranchContextModel)args;
            BranchAdjust?.Invoke(changeContext);
        }

        private void Recalculate()
        {
            TotalDispute = disputes.Count(p => p.BranchNumber >= branchStart && p.BranchNumber <= branchEnd);
        }
    }
}