using Microsoft.Extensions.DependencyInjection;
using SupportToolApp.Services;

namespace SupportToolApp;

public partial class App : Application
{
	private readonly CredentialService _creds;

	public App(CredentialService creds)
	{
		InitializeComponent();
		_creds = creds;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell(_creds))
		{
			Width  = 1400,
			Height = 820,
			MinimumWidth  = 1100,
			MinimumHeight = 650
		};
	}
}