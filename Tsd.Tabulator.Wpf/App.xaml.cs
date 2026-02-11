using System.Windows;
using System.Runtime.Versioning;

namespace Tsd.Tabulator.Wpf;

[SupportedOSPlatform("windows")]
public partial class App : System.Windows.Application
{
    private readonly Bootstrapper _bootstrapper;

    public App()
    {
        _bootstrapper = new Bootstrapper();
    }
}