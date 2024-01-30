using Ktb.BranchAdjustor.Models;

namespace Ktb.BranchAdjustor.Maui;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		BindingContext = new ApplicationModel();
	}
}

