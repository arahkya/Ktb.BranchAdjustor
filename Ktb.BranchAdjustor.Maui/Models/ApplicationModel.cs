using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using Ktb.BranchAdjustor.Maui.Entities;
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

                int minBranch = disputeGroupByBranch.First().Key;
                int maxBranch = disputeGroupByBranch.Last().Key;
                int totalDispute = disputes.Count();

                FileInfo.BranchRange = $"{string.Format(branchFormat, minBranch)}-{string.Format(branchFormat, maxBranch)}";
                FileInfo.TotalBranch = disputeGroupByBranch.Count();
                FileInfo.TotalDispute = totalDispute;
                FileInfo.BranchPerWorker = disputeGroupByBranch.Count() / FileInfo.WorkerNumber;
                FileInfo.DisputePerWorker = totalDispute / FileInfo.WorkerNumber;

                BranchDistributor branchDistributor = new(disputes, new Range(minBranch, maxBranch), FileInfo.WorkerNumber);

                foreach (BranchDistributedEntity entity in branchDistributor.Distribute())
                {
                    BranchDistributedEntities.Add(entity);
                }
            });

            IsBusy = false;
        }
    }
}