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
            }
        }

        private int branchEnd;

        public int BranchEnd
        {
            get => branchEnd;
            set
            {
                branchEnd = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BranchEnd)));
            }
        }

        public int TotalDispute { get; set; }
        public int TotalBranch => BranchEnd - BranchStart;

        public delegate void BranchAdjustHandler(ChangeBranchContextModel changeBranchContextModel);

        public BranchAdjustHandler? BranchAdjust;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand AdjustBranchCommand { get; set; }

        public BranchDistributedEntity()
        {
            AdjustBranchCommand = new Command(AdjustBranchCommandHandler);
        }

        private void AdjustBranchCommandHandler(object args)
        {
            ChangeBranchContextModel changeContext = (ChangeBranchContextModel)args;

            BranchAdjust?.Invoke(changeContext);
        }
    }
}