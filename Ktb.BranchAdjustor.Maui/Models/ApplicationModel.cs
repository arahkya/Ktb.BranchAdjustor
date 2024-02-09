using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using CommunityToolkit.Mvvm.Messaging;
using Ktb.BranchAdjustor.Maui.Models;
using Ktb.BranchAdjustor.Maui.Services;

namespace Ktb.BranchAdjustor.Models
{
    public class ApplicationModel : INotifyPropertyChanged
    {
        private bool isBusy;
        private decimal progress;
        private string status;

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

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<BranchDistributedEntity> BranchDistributedEntities { get; set; } = [];

        public ApplicationModel(FileInfoModel fileInfo, ExcelFileReader excelFileReader, DataTableToDisputeEntityConverter dataTableToDisputeEntityConverter, BranchDistributor distributor)
        {
            FileInfo = fileInfo;
            status = "Ready";

            WeakReferenceMessenger.Default.Register(excelFileReader);
            WeakReferenceMessenger.Default.Register(dataTableToDisputeEntityConverter);
            WeakReferenceMessenger.Default.Register<List<DisputeEntity>>(distributor);
            WeakReferenceMessenger.Default.Register<WorkerContextModel>(distributor);
            WeakReferenceMessenger.Default.Register<BranchContextModel>(distributor);
            WeakReferenceMessenger.Default.Register<BranchDistributedEntity>(this, (appModel, branchDistEntity) =>
            {
                ((ApplicationModel)appModel).BranchDistributedEntities.Add(branchDistEntity);
            });
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
    }
}