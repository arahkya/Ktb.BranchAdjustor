namespace Ktb.BranchAdjustor.Maui.Models;

public class ChangeBranchContextModel
{
    public int Index { get; set; }
    public int Branch { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Changed { get; set; } = string.Empty;
}