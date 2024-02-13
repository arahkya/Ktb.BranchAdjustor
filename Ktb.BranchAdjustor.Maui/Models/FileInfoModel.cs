using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ktb.BranchAdjustor.Maui.Models;
using Ktb.BranchAdjustor.Maui.Services;

namespace Ktb.BranchAdjustor.Models
{
    public class FileInfoModel : INotifyPropertyChanged
    {
        private decimal progress;
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
        public decimal Progress
        {
            get => progress;
            set
            {
                progress = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
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

        public IAsyncRelayCommand SelectFileCommand { get; set; }

        private const string fileNameDefault = "Select File";
        private readonly ExcelFileReader excelFileReader;
        private readonly DataTableToDisputeEntityConverter dataTableToDisputeEntityConverter;

        public FileInfoModel(ExcelFileReader excelFileReader, DataTableToDisputeEntityConverter dataTableToDisputeEntityConverter)
        {
            SelectFileCommand = new AsyncRelayCommand(SelectFileCommandHandler);

            FileName = fileNameDefault;

            WorkerNumber = 7;
            this.excelFileReader = excelFileReader;
            this.dataTableToDisputeEntityConverter = dataTableToDisputeEntityConverter;

            WeakReferenceMessenger.Default.Register<ApplicationStateModel>(this, (fileInfoModel, appState) =>
            {
                ((FileInfoModel)fileInfoModel).Progress = appState.Progress;
            });
        }

        private async Task SelectFileCommandHandler()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                WeakReferenceMessenger.Default.Send(new ApplicationStateModel
                {
                    Status = "Restart"
                });
            }

            string fileName = await PickFileAsync();

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            FileName = fileName;

            DataTable disputeDataTable = await excelFileReader.ReadAsync(fileName);
            IEnumerable<DisputeEntity> disputes = await dataTableToDisputeEntityConverter.ConvertAsync(disputeDataTable);

            TotalDispute = disputes.Count();
            TotalBranch = disputes.Max(p => p.BranchNumber);
            BranchRange = $"00000-{TotalBranch.ToString("00000")}";
            BranchPerWorker = TotalBranch / WorkerNumber;
            DisputePerWorker = TotalDispute / WorkerNumber;

            WeakReferenceMessenger.Default.Send(disputes);
            // WeakReferenceMessenger.Default.Send(new ApplicationStateModel
            // {
            //     Status = "Done"
            // });
        }

        private async Task<string> PickFileAsync()
        {
            try
            {
                FileResult? fileResult = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select File",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, [ "xlsx" ] },
                        { DevicePlatform.MacCatalyst, [ "xlsx" ] }
                    })
                });

                if (fileResult == null) return string.Empty;

                Debug.WriteLine($"Select file: {FileName}");

                return fileResult.FullPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return string.Empty;
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