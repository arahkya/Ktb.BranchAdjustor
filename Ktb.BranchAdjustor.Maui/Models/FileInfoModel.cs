using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ktb.BranchAdjustor.Maui.Models;

namespace Ktb.BranchAdjustor.Models
{
    public class FileInfoModel : INotifyPropertyChanged
    {
        private string fileName = string.Empty;
        private int workerNumber;
        private int totalBranch;
        private string branchRange = string.Empty;
        private int branchPerWorker;
        private int disputePerWorker;
        private int totalDispute;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
            }
        }
        public int WorkerNumber
        {
            get => workerNumber;
            set
            {
                workerNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WorkerNumber)));
            }
        }
        public int TotalBranch
        {
            get => totalBranch;
            set
            {
                totalBranch = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBranch)));
            }
        }
        public string BranchRange
        {
            get => branchRange;
            set
            {
                branchRange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BranchRange)));
            }
        }
        public int BranchPerWorker
        {
            get => branchPerWorker;
            set
            {
                branchPerWorker = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BranchPerWorker)));
            }
        }
        public int DisputePerWorker
        {
            get => disputePerWorker;
            set
            {
                disputePerWorker = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisputePerWorker)));
            }
        }
        public int TotalDispute
        {
            get => totalDispute;
            set
            {
                totalDispute = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalDispute)));
            }
        }

        public IRelayCommand SelectFileCommand { get; set; }


        private const string fileNameDefault = "Select File";

        public FileInfoModel()
        {
            SelectFileCommand = new RelayCommand(async () => await SelectFileCommandHandler());

            FileName = fileNameDefault;

            WorkerNumber = 7;
        }

        private async Task SelectFileCommandHandler()
        {
            System.Diagnostics.Debug.WriteLine("Enter select file function.");

            try
            {
                FileResult? fileResult = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select File",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.MacCatalyst, [ "xlsx" ] }
                    })
                });

                if (fileResult == null) return;

                FileName = fileResult.FullPath;

                WeakReferenceMessenger.Default.Send(new WorkerContextModel { TotalWorker = WorkerNumber });
                WeakReferenceMessenger.Default.Send(fileResult);

                System.Diagnostics.Debug.WriteLine($"Select file: {FileName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public void Reset()
        {
            FileName = fileNameDefault;
            BranchRange = string.Empty;
            TotalBranch = 0;
            TotalDispute = 0;
            BranchPerWorker = 0;
            DisputePerWorker = 0;
        }
    }
}