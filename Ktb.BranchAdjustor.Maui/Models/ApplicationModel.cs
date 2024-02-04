using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using Ktb.BranchAdjustor.Maui.Entities;
using Ktb.BranchAdjustor.Maui.Models;
using Ktb.BranchAdjustor.Maui.Services;

namespace Ktb.BranchAdjustor.Models
{
    public class ApplicationModel : INotifyPropertyChanged
    {
        private bool isBusy;

        public FileInfoModel FileInfo { get; set; }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsIdle)));
            }
        }

        public bool IsIdle
        {
            get => !isBusy;
        }

        public ObservableCollection<BranchDistributedEntity> BranchDistributedEntities { get; set; } = [];

        public ApplicationModel()
        {
            FileInfo = new(async (fileName) => await LoadFileCommandHandler(fileName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private async Task LoadFileCommandHandler(string fileName)
        {
            IsBusy = true;

            BranchDistributedEntities.Clear();

            await Task.Factory.StartNew(() =>
            {
                string sheetName = "DisputeATM";
                string branchCodeColumnName = "Branch";
                string createDateColumnName = "CREATE_DATE";
                string terminalCodeColumnName = "TERM_ID";

                ExcelFileReader reader = new(fileName, sheetName);
                DataTable disputeDataTable = reader.Read();

                DataTableToDisputeEntityConverter disputeConverter = new(disputeDataTable, branchCodeColumnName, createDateColumnName, terminalCodeColumnName);
                IEnumerable<DisputeEntity> disputes = disputeConverter.Convert();

                IOrderedEnumerable<IGrouping<int, DisputeEntity>> disputeGroupByBranch = disputes.GroupBy(p => p.BranchNumber).OrderBy(p => p.Key);

                const string branchFormat = "K{0:00000}";

                int minBranch = 0;
                int maxBranch = disputeGroupByBranch.Last().Key;
                int totalDispute = disputes.Count();

                FileInfo.BranchRange = $"{string.Format(branchFormat, minBranch)}-{string.Format(branchFormat, maxBranch)}";
                FileInfo.TotalBranch = disputeGroupByBranch.Count();
                FileInfo.TotalDispute = totalDispute;
                FileInfo.BranchPerWorker = disputeGroupByBranch.Last().Key / FileInfo.WorkerNumber;
                FileInfo.DisputePerWorker = totalDispute / FileInfo.WorkerNumber;

                BranchDistributor branchDistributor = new(disputes, new Range(minBranch, maxBranch), FileInfo.WorkerNumber);
                int index = 0;
                foreach (BranchDistributedEntity entity in branchDistributor.Distribute())
                {
                    entity.Index = index;
                    entity.BranchAdjust = OnAdjustBranchHandler;
                    BranchDistributedEntities.Add(entity);

                    index++;
                }
            });

            IsBusy = false;
        }

        private void OnAdjustBranchHandler(ChangeBranchContextModel changeBranchContextModel)
        {
            if (changeBranchContextModel.Position == "End")
            {
                BranchDistributedEntity nextBranch = BranchDistributedEntities[changeBranchContextModel.Index + 1];

                if (changeBranchContextModel.Changed == "+")
                {
                    nextBranch.BranchStart++;
                }
                else if (changeBranchContextModel.Changed == "-")
                {
                    nextBranch.BranchStart--;
                }
            }
            else if (changeBranchContextModel.Position == "Start")
            {
                BranchDistributedEntity prevBranch = BranchDistributedEntities[changeBranchContextModel.Index - 1];

                if (changeBranchContextModel.Changed == "+")
                {
                    prevBranch.BranchEnd++;
                }
                else if (changeBranchContextModel.Changed == "-")
                {
                    prevBranch.BranchEnd--;
                }
            }
        }
    }
}