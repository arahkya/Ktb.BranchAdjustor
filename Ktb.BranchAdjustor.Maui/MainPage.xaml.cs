using Ktb.BranchAdjustor.Models;

namespace Ktb.BranchAdjustor.Maui;

public partial class MainPage : ContentPage
{
	public MainPage(ApplicationModel applicationModel)
	{
		InitializeComponent();

		BindingContext = applicationModel;
	}
}

