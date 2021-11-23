using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Suplanus.Sepla.Application;

namespace Example.LicenseClient
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public static Window Window { get; set; }

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
      Window = new MainWindow();
      InitEplan();
      Window.ShowDialog();
    }

    private void InitEplan()
    {
      string binPath = Starter.GetEplanInstallations()
                              .Last(obj => obj.EplanVariant
                                              .Equals("Electric P8") && obj.EplanVersion.StartsWith("2.7"))
                              .EplanPath;
      binPath = Path.GetDirectoryName(binPath);

      Starter.PinToEplan(binPath); // Don't forget
      EplanOffline eplanOffline = new EplanOffline(binPath, "API");
      eplanOffline.StartWpf(Window);
    }
  }
}
