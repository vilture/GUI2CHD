using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace GUI2CHD
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            if (!IsDotNetRuntimeInstalled())
            {
                MessageBox.Show(
                    "Для работы программы требуется .NET 6.0 Runtime.\n\n" +
                    "Пожалуйста, установите .NET 6.0 Runtime с официального сайта Microsoft:\n" +
                    "https://dotnet.microsoft.com/download/dotnet/6.0/runtime",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static bool IsDotNetRuntimeInstalled()
        {
            try
            {
                // Проверяем наличие .NET 6.0 Runtime
                string dotnetPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "dotnet",
                    "dotnet.exe"
                );

                if (!File.Exists(dotnetPath))
                {
                    return false;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = dotnetPath,
                    Arguments = "--list-runtimes",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        return output.Contains("Microsoft.NETCore.App 6.0");
                    }
                }
            }
            catch
            {
                // В случае ошибки считаем, что runtime не установлен
                return false;
            }

            return false;
        }
    }
} 