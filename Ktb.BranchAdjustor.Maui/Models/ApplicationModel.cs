using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using Ktb.BranchAdjustor.Maui.Models;
using Ktb.BranchAdjustor.Maui.Services;

namespace Ktb.BranchAdjustor.Models
{
    public class ApplicationModel : INotifyPropertyChanged
    {
        private bool isBusy;
        private decimal progress;
        private string status;

        private CancellationTokenSource cancellationTokenSource;

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

        public decimal Progress
        {
            get => progress;
            set
            {
                progress = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }

        public string Status
        {
            get => status;
            set
            {
                status = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        public ObservableCollection<BranchDistributedEntity> BranchDistributedEntities { get; set; } = [];

        public ApplicationModel()
        {
            cancellationTokenSource = new();
            FileInfo = new(async (fileName) => await LoadFileCommandHandler(fileName), CancelLoadFileHandler);
            status = "Ready";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private async Task LoadFileCommandHandler(string fileName)
        {
            IsBusy = true;

            FileInfo.Reset();

            BranchDistributedEntities.Clear();

            await Task.Factory.StartNew(() =>
            {
                string sheetName = "DisputeATM";
                string branchCodeColumnName = "Branch";
                string createDateColumnName = "CREATE_DATE";
                string terminalCodeColumnName = "TERM_ID";

                Status = "Reading Excel File";

                ExcelFileReader reader = new(fileName, sheetName);
                DataTable disputeDataTable = reader.Read();

                DataTableToDisputeEntityConverter disputeConverter = new(disputeDataTable, branchCodeColumnName, createDateColumnName, terminalCodeColumnName);
                IEnumerable<DisputeEntity> disputes = disputeConverter.Convert();

                IOrderedEnumerable<IGrouping<int, DisputeEntity>> disputeGroupByBranch = disputes.GroupBy(p => p.BranchNumber).OrderBy(p => p.Key);

                const string branchFormat = "{0:00000}";

                int minBranch = 0;
                int maxBranch = disputeGroupByBranch.Last().Key;
                int totalDispute = disputes.Count();

                FileInfo.BranchRange = $"{string.Format(branchFormat, minBranch)}-{string.Format(branchFormat, maxBranch)}";
                FileInfo.TotalBranch = maxBranch;
                FileInfo.TotalDispute = totalDispute;
                FileInfo.BranchPerWorker = disputeGroupByBranch.Last().Key / FileInfo.WorkerNumber;
                FileInfo.DisputePerWorker = totalDispute / FileInfo.WorkerNumber;

                BranchDistributor branchDistributor = new(disputes, new Range(minBranch, maxBranch), FileInfo.WorkerNumber);
                int index = 0;

                branchDistributor.ProgressChanged += (progress, message) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Progress {progress}%");
                    Status = $"Progress {System.Math.Round(progress * 100, 2)}%, {message}";

                    Progress = progress;
                };

                foreach (BranchDistributedEntity entity in branchDistributor.DistributeByBranch(cancellationTokenSource.Token))
                {
                    entity.Index = index;
                    entity.BranchAdjust = OnAdjustBranchHandler;

                    BranchDistributedEntities.Add(entity);

                    index++;
                }

                Status = "Ready";
            });

            IsBusy = false;
        }

        private void OnAdjustBranchHandler(ChangeBranchContextModel changeBranchContextModel)
        {
            BranchDistributedEntity currentBranch = BranchDistributedEntities[changeBranchContextModel.Index];

            if (changeBranchContextModel.Position == "End")
            {
                BranchDistributedEntity nextBranch = BranchDistributedEntities[changeBranchContextModel.Index + 1];

                if (changeBranchContextModel.Changed == "+")
                {
                    nextBranch.BranchStart = currentBranch.BranchEnd + 1;
                }
                else if (changeBranchContextModel.Changed == "-")
                {
                    nextBranch.BranchStart = currentBranch.BranchEnd + 1;
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

        private void CancelLoadFileHandler()
        {
            cancellationTokenSource.Cancel();

            FileInfo.Reset();

            Progress = 0;

            IsBusy = false;

            BranchDistributedEntities.Clear();

            cancellationTokenSource = new();
        }
    }
}