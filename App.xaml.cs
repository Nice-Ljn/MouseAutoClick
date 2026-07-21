﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using MouseRecorderWpf.Views;

namespace MouseRecorderWpf;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        if (!IsRunningAsAdmin())
        {
            RunAsAdmin();
            Shutdown();
            return;
        }

        base.OnStartup(e);
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }

    private static bool IsRunningAsAdmin()
    {
        using (var identity = WindowsIdentity.GetCurrent())
        {
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    private static void RunAsAdmin()
    {
        try
        {
            var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var assemblyDir = System.IO.Path.GetDirectoryName(assemblyPath);
            var exePath = System.IO.Path.Combine(assemblyDir, "MouseRecorderWpf.exe");

            if (System.IO.File.Exists(exePath))
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(processInfo);
            }
            else
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{assemblyPath}\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    WorkingDirectory = assemblyDir
                };
                Process.Start(processInfo);
            }
        }
        catch
        {
            System.Windows.MessageBox.Show("需要管理员权限才能正常运行，请右键以管理员身份运行", "权限不足", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
