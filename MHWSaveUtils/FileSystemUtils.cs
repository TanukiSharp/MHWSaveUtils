using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace MHWSaveUtils
{
    public struct SaveDataInfo
    {
        public string UserId { get; }
        public string SaveDataFullFilename { get; }

        public SaveDataInfo(string userId, string saveDataFullFilename)
        {
            if (userId == null)
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException($"Argument '{nameof(userId)}' must not be empty.");
            if (saveDataFullFilename == null)
                throw new ArgumentNullException(nameof(saveDataFullFilename));
            if (string.IsNullOrWhiteSpace(saveDataFullFilename))
                throw new ArgumentException($"Argument '{nameof(saveDataFullFilename)}' must not be empty.");

            UserId = userId;
            SaveDataFullFilename = saveDataFullFilename;
        }

        public bool IsEmpty
        {
            get
            {
                return UserId == null;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is SaveDataInfo data)
                return data.UserId == UserId && data.SaveDataFullFilename == SaveDataFullFilename;
            return false;
        }

        public override int GetHashCode()
        {
            return $"{UserId};{SaveDataFullFilename}".GetHashCode();
        }

        public static bool operator ==(SaveDataInfo left, SaveDataInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SaveDataInfo left, SaveDataInfo right)
        {
            return !(left == right);
        }
    }

    public static class FileSystemUtils
    {
        // Code copied from this repository: git@github.com:Nexusphobiker/MHWSaveEditor.git

        public const string GameId = "582010";
        public const string GameSaveDataFilename = "SAVEDATA1000";

        public static string SteamPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null);

        public static IEnumerable<SaveDataInfo> EnumerateSaveDataInfo()
        {
            if (SteamPath == null)
                yield break;

            string userDataPath = Path.Combine(SteamPath, "userdata");

            foreach (string userDirectory in Directory.GetDirectories(userDataPath))
            {
                foreach (string gameDirectory in Directory.GetDirectories(userDirectory))
                {
                    if (Path.GetFileName(gameDirectory) != GameId)
                        continue;

                    string saveDataFullFilename = Path.GetFullPath(Path.Combine(gameDirectory, "remote", GameSaveDataFilename));

                    if (File.Exists(saveDataFullFilename) == false)
                        continue;

                    string userId = Path.GetFileName(userDirectory);
                    yield return new SaveDataInfo(userId, saveDataFullFilename);
                }
            }
        }
    }

    public class SaveDataChangedEventArgs : EventArgs
    {
        public string SaveDataFullFilename { get; }
        public CancellationToken CancellationToken { get; }

        public SaveDataChangedEventArgs(string saveDataFullFilename, CancellationToken cancellationToken)
        {
            SaveDataFullFilename = saveDataFullFilename;
            CancellationToken = cancellationToken;
        }
    }

    public sealed class SaveDataFileMonitor : IDisposable
    {
        private readonly int delay;
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;

        private FileSystemWatcher watcher;

        public string SaveDataFullFilename { get; }
        public event EventHandler<SaveDataChangedEventArgs> SaveDataFileChanged;

        public SaveDataFileMonitor(SaveDataInfo saveDataInfo, int delay = 500)
            : this(saveDataInfo.SaveDataFullFilename, delay)
        {
        }

        public SaveDataFileMonitor(string saveDataFullFilename, int delay = 500)
        {
            if (saveDataFullFilename == null)
                throw new ArgumentNullException(nameof(saveDataFullFilename));

            if (File.Exists(saveDataFullFilename) == false)
                throw new ArgumentException($"File '{saveDataFullFilename}' does not exist");

            SaveDataFullFilename = saveDataFullFilename;
            this.delay = delay;

            watcher = new FileSystemWatcher(
                Path.GetDirectoryName(saveDataFullFilename),
                Path.GetFileName(saveDataFullFilename)
            )
            {
                IncludeSubdirectories = false
            };

            watcher.Renamed += OnSaveDataChanged;

            watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                watcher = null;
            }
        }

        private CancellationTokenSource cancellationTokenSource;

        private async void OnSaveDataChanged(object sender, RenamedEventArgs e)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            cancellationTokenSource = new CancellationTokenSource();

            if (delay > 0)
            {
                try
                {
                    await Task.Delay(delay, cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }

            var evt = new SaveDataChangedEventArgs(SaveDataFullFilename, cancellationTokenSource.Token);

            if (synchronizationContext != null)
                synchronizationContext.Send(_ => SaveDataFileChanged?.Invoke(this, evt), null);
            else
                SaveDataFileChanged?.Invoke(this, evt);
        }
    }
}
