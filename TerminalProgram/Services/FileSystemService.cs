using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using TerminalProgramBase.Views;
using Services.Interfaces;

namespace TerminalProgramBase.Services
{
    public class FileSystemService : IFileSystemService
    {
        public async Task<string?> GetFilePath(string windowTitle, string pickerFileTypeName, IReadOnlyList<string>? patterns)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            TopLevel? topLevel = TopLevel.GetTopLevel(MainWindow.Instance);

            if (topLevel != null)
            {
                var options = new FilePickerOpenOptions
                {
                    Title = windowTitle,
                    AllowMultiple = false
                };

                if (patterns != null)
                {
                    options.FileTypeFilter = [new FilePickerFileType(pickerFileTypeName) { Patterns = patterns }];
                }

                // Start async operation to open the dialog.
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);

                if (files.Count >= 1)
                {
                    return files.First().Path.AbsolutePath;
                }
            }

            return null;
        }

        public async Task<string?> GetFolderPath(string windowTitle)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            TopLevel? topLevel = TopLevel.GetTopLevel(MainWindow.Instance);

            if (topLevel != null)
            {
                var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = windowTitle,
                    AllowMultiple = false
                });

                if (folder != null && folder.Count > 0)
                {
                    return folder.First().TryGetLocalPath();
                }
            }

            return null;
        }

        public void OpenUserManual()
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine("Documentation", "UserManual.pdf"),
                    UseShellExecute = true,
                });

                return;
            }

            else if (OperatingSystem.IsLinux())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = Path.Combine("Documentation", "UserManual.pdf"),
                    UseShellExecute = false,
                });

                return;
            }

            throw new Exception("Неподдерживаемый тип ОС.");
        }
    }
}
