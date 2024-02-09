using Microsoft.Extensions.Logging;
using DotNet.Meteor.HotReload.Plugin;
using Ktb.BranchAdjustor.Models;
using Ktb.BranchAdjustor.Maui.Services;

namespace Ktb.BranchAdjustor.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.EnableHotReload()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<ApplicationModel>();
		builder.Services.AddScoped<FileInfoModel>();
		builder.Services.AddScoped<ExcelFileReader>();
		builder.Services.AddScoped<DataTableToDisputeEntityConverter>();
		builder.Services.AddScoped<BranchDistributor>();
		builder.Services.AddScoped<ExcelSheetConfigurationOption>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
