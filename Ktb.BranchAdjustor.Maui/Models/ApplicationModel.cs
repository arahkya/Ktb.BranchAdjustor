using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Ktb.BranchAdjustor.Maui.Models;
using Ktb.BranchAdjustor.Maui.Services;

namespace Ktb.BranchAdjustor.Models
{
    public class ApplicationModel : INotifyPropertyChanged
    {
        private bool isBusy;
        private string status;

        public FileInfoModel FileInfo { get; set; }
        public BranchDistributor Distributor { get; }

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

        public ApplicationModel(FileInfoModel fileInfo, BranchDistributor distributor)
        {
            FileInfo = fileInfo;
            Distributor = distributor;
            status = "Ready";

            WeakReferenceMessenger.Default.Register<ApplicationStateModel>(this, (appModel, appState) =>
            {
                ApplicationModel applicationModel = (ApplicationModel)appModel;

                if (applicationModel.Status == "Restart")
                {
                    applicationModel.BranchDistributedEntities.Clear();
                }

                applicationModel.IsBusy = appState.Status != "Done";
                applicationModel.Status = appState.Status;
            });

            WeakReferenceMessenger.Default.Register<IEnumerable<DisputeEntity>>(this, async (appModel, disputes) =>
            {
                ApplicationModel applicationModel = (ApplicationModel)appModel;

                await applicationModel.Distributor.DistributeAsync(disputes, applicationModel.FileInfo.WorkerNumber, new Range(0, applicationModel.FileInfo.TotalBranch));
            });

            WeakReferenceMessenger.Default.Register<BranchDistributedEntity>(this, (appModel, branchDistributed) =>
            {
                branchDistributed.Index = ((ApplicationModel)appModel).BranchDistributedEntities.Count;
                ((ApplicationModel)appModel).BranchDistributedEntities.Add(branchDistributed);
            });

            WeakReferenceMessenger.Default.Register<ChangeBranchContextModel>(this, (appModel, changeContext) =>
            {
                ((ApplicationModel)appModel).OnAdjustBranchHandler(changeContext);
            });
        }

        public void OnAdjustBranchHandler(ChangeBranchContextModel changeBranchContextModel)
        {
            BranchDistributedEntity currentBranch = BranchDistributedEntities[changeBranchContextModel.Index];


            BranchDistributedEntity nextBranch = BranchDistributedEntities[changeBranchContextModel.Index + 1];

            if (changeBranchContextModel.Changed == "+")
            {
                nextBranch.BranchStart = currentBranch.BranchEnd - 1;
            }
            else if (changeBranchContextModel.Changed == "-")
            {
                nextBranch.BranchStart = currentBranch.BranchEnd + 1;
            }
        }
    }
}