using System.Windows;

namespace Tsd.Tabulator.Wpf;

public partial class App : Application
{
    private readonly Bootstrapper _bootstrapper;

    public App()
    {
        _bootstrapper = new Bootstrapper();
    }
}
