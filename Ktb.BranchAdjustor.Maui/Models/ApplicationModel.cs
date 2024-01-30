namespace Ktb.BranchAdjustor.Models
{
    public class ApplicationModel
    {
        public FileInfoModel FileInfo { get; set; }

        public ApplicationModel()
        {
            FileInfo = new();
        }
    }
}