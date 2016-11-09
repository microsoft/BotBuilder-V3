using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    /// <summary>
    /// Starts and stops Azure Storage Emulator for use in unit or integration tests
    /// </summary>
    internal static class StorageEmulatorRunner
    {
        //Azure storage emulator process name varies by version and architecture, taking one of the names below
        private static readonly string emulatorProcessNameV1 = "AZURES~1";
        private static readonly string emulatorProcessNameV2 = "AzureStorageEmulator";

        private static readonly string emulatorExecutableFileName = "AzureStorageEmulator.exe";
        private static readonly string azureSdkSubDirectory = @"{0}\Microsoft SDKs\Azure\Storage Emulator";

        private static bool isRunning;

        /// <summary>
        /// Starts Azure Storage Emulator if it has not been started already
        /// </summary>
        public static void Start()
        {
            if (isRunning)
            {
                return;
            }

            Process[] azureStorageProcesses = Process.GetProcesses();
            if(azureStorageProcesses.Any(p => IsStorageEmulator(p)))
            {
                isRunning = true;
                return;
            }

            var azureSdkDirectory = string.Format(azureSdkSubDirectory, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            var executableFullFilePath = Path.Combine(azureSdkDirectory, emulatorExecutableFileName);

            if (!File.Exists(executableFullFilePath))
            {
                throw new FileNotFoundException(string.Format("Failed to find Azure Storage Emulator at {0}. Make sure Azure Storage Emulator is installed", executableFullFilePath));
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = executableFullFilePath,
                Arguments = "start"
            };

            using (Process emulatorProcess = Process.Start(processStartInfo))
            {
                emulatorProcess.WaitForExit();
                isRunning = true;
            }
        }

        /// <summary>
        /// Stops Azure Storage Emulator
        /// </summary>
        public static void Stop()
        {
            Process[] azureStorageProcesses = Process.GetProcesses();
            var emulatorProcess = azureStorageProcesses.SingleOrDefault(p => IsStorageEmulator(p));

            if (emulatorProcess != null)
            {
                emulatorProcess.Kill();
                isRunning = false;
            }
        }

        private static bool IsStorageEmulator(Process p)
        {
            return p.ProcessName.StartsWith(emulatorProcessNameV1, StringComparison.InvariantCultureIgnoreCase) 
                || p.ProcessName.StartsWith(emulatorProcessNameV2, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
