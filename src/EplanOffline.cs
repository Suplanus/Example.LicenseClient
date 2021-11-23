﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Eplan.EplApi.System;

namespace Suplanus.Sepla.Application
{
  /// <summary>
  ///   EplanOffline class to use EPLAN Offline Application
  /// </summary>
  public class EplanOffline
  {
    /// <summary>
    ///   Returns if the EPLAN-Application is running
    /// </summary>
    public bool IsRunning
    {
      get
      {
        var running = Application != null;
        return running;
      }
    }

    /// <summary>
    ///   EPLAN Application
    /// </summary>
    public EplApplication Application;

    /// <summary>
    ///   EPLAN Bin path e.g.: C:\Program Files\EPLAN\Platform\2.6.3\Bin
    /// </summary>
    public string BinPath;

    /// <summary>
    ///   LicenseFile to start with given package
    /// </summary>
    public string LicenseFile;

    /// <summary>
    ///   SystemConfiguration with environment paths
    /// </summary>
    public string SystemConfiguration;

    /// <summary>
    ///   Init EPLAN with given bin path and license file (optional)
    /// </summary>
    /// <param name="binPath"></param>
    /// <param name="systemConfiguration"></param>
    /// <param name="licenseFile"></param>
    public EplanOffline(string binPath, string systemConfiguration, string licenseFile = null)
    {
      BinPath = binPath;
      SystemConfiguration = systemConfiguration;
      LicenseFile = licenseFile;
    }

    /// <summary>
    ///   Starts EPLAN in ConsoleApplication
    ///   You have to set Attribut <c>[STAThread]</c> for Main void
    /// </summary>
    public void StartWithoutGui()
    {
      Start();
    }

    /// <summary>
    ///   Starts EPLAN with the last version of Electric P8 and attach to (WPF) window
    /// </summary>
    /// <param name="window"></param>
    public void StartWpf(Window window)
    {
      IntPtr handle = new WindowInteropHelper(window).Handle;
      Start(handle);
    }

    /// <summary>
    ///   Starts EPLAN with the last version of Electric P8 and attach to (WPF) window
    /// </summary>
    /// <param name="window"></param>
    public Task<bool> StartWpfAsync(Window window)
    {
      IntPtr handle = new WindowInteropHelper(window).Handle;

      var tcs = new TaskCompletionSource<bool>();
      Thread thread = new Thread(() =>
      {
        Start(handle);
        tcs.SetResult(true);
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();

      tcs.Task.Wait();
      return tcs.Task;
    }

    public static Task<T> StartSTATask<T>(Func<T> func)
    {
      var tcs = new TaskCompletionSource<T>();
      Thread thread = new Thread(() =>
      {
        try
        {
          tcs.SetResult(func());
        }
        catch (Exception e)
        {
          tcs.SetException(e);
        }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      return tcs.Task;
    }

    /// <summary>
    ///   Starts the application
    /// </summary>
    /// <param name="handle"></param>
    private void Start(IntPtr handle)
    {
      if (!IsRunning)
      {
        try
        {
          EplApplication eplApplication = new EplApplication();
          eplApplication.EplanBinFolder = BinPath;
          if (!string.IsNullOrEmpty(SystemConfiguration))
          {
            eplApplication.SystemConfiguration = SystemConfiguration;
          }
          eplApplication.QuietMode = EplApplication.QuietModes.ShowAllDialogs;
          eplApplication.SetMainFrame(handle);
          if (!string.IsNullOrEmpty(LicenseFile))
          {
            eplApplication.LicenseFile = LicenseFile; // Set specific license
          }
          eplApplication.Init("", true, true);
          eplApplication.QuietMode = EplApplication.QuietModes.ShowNoDialogs;
          Application = eplApplication;
        }
        catch
        {
          Application = null;
        }
      }
    }

    /// <summary>
    ///   Starts the application without Gui
    /// </summary>
    private void Start()
    {
      if (!IsRunning)
      {
        try
        {
          EplApplication eplApplication = new EplApplication();
          eplApplication.EplanBinFolder = BinPath;
          if (!string.IsNullOrEmpty(SystemConfiguration))
          {
            eplApplication.SystemConfiguration = SystemConfiguration;
          }
          if (!string.IsNullOrEmpty(LicenseFile))
          {
            eplApplication.LicenseFile = LicenseFile; // Set specific license
          }
          eplApplication.QuietMode = EplApplication.QuietModes.ShowAllDialogs;
          eplApplication.Init("", true, true);
          eplApplication.QuietMode = EplApplication.QuietModes.ShowNoDialogs;
          Application = eplApplication;
        }
        catch
        {
          Application = null;
        }
      }
    }

    /// <summary>
    ///   Release all objects
    ///   <note type="caution">
    ///     Needed for eplan runtime exceptions, there is a known issue (T1094381), EPLAN says Microsoft should fix this
    ///     problem
    ///     Workaround: Enable native code debugging in visual studio
    ///   </note>
    /// </summary>
    public void Close()
    {
      // T1094381: There is a known problem with console applications, that visual studio not quit the debugging session, workaround: enable native code debugging in project
      if (Application != null)
      {
        try
        {
          Application.Exit();
        }
        finally
        {
          Application = null;
        }
      }
    }
  }
}