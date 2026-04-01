using Android.App;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinDeutsch.Maui;

[Application]
public class MainApplication : MauiApplication
{
	public new static IServiceProvider? Services { get; private set; }

	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp()
	{
		MauiApp mauiApp = MauiProgram.CreateMauiApp();
		Services = mauiApp.Services;
		return mauiApp;
	}
}
