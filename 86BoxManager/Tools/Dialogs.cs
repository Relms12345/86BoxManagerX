using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Dto;
using System.Linq;
using Avalonia.Platform.Storage;
using MsBox.Avalonia.Controls;
using MsBox.Avalonia.ViewModels;
using MsBox.Avalonia.Windows;
using StartLoc = Avalonia.Controls.WindowStartupLocation;

namespace _86BoxManager.Tools
{
    internal static class Dialogs
    {
        public static ButtonResult ShowMessageBox(string msg, Icon icon,
            ButtonEnum buttons = ButtonEnum.Ok, string title = "Attention")
        {
            var parent = Program.Root;
            var loc = parent == null ? StartLoc.CenterScreen : StartLoc.CenterOwner;
            var opts = new MessageBoxStandardParams
            {
                ButtonDefinitions = buttons,
                ContentTitle = title,
                ContentMessage = msg,
                Icon = icon,
                CanResize = false,
                WindowStartupLocation = loc,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            
            var window = MessageBoxManager.GetMessageBoxStandard(opts);
            
            var _viewModelType = window.GetType().GetField("_viewModel", BindingFlags.Instance | BindingFlags.NonPublic);
            var _viewModel = (MsBoxStandardViewModel)_viewModelType?.GetValue(window);
            
            var _viewType = window.GetType().GetField("_view", BindingFlags.Instance | BindingFlags.NonPublic);
            var _view = (MsBoxStandardView)_viewType?.GetValue(window);
            
            _viewModel.SetFullApi(_view);
            var win = new MsBoxWindow
            {
                Content = _view,
                DataContext = _viewModel
            };
            win.Closed += _view.CloseWindow;
            var tcs = new TaskCompletionSource<ButtonResult>();

            _view.SetCloseAction(() =>
            {
                tcs.TrySetResult(_view.GetButtonResult());
                win.Close();
            });

            win.Show();

            var buttonResult = tcs.Task;
            
            //var raw = parent != null ? window.ShowWindowDialogAsync(parent) : window.ShowAsync();
            
            if (Application.Current is var app)
            {
                var flags = BindingFlags.NonPublic | BindingFlags.Instance;
                if (parent?.Icon is { } wi)
                    win.Icon = wi;
                app.Run(win);
            }
            var res = buttonResult.GetAwaiter().GetResult();
            return res;
        }

        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        public static async Task RunDialog(this Window parent, Window dialog, Action func = null)
        {
            dialog.WindowStartupLocation = StartLoc.CenterOwner;
            dialog.Icon = parent.Icon;

            var raw = dialog.ShowDialog(parent);
            await raw;
            func?.Invoke();
            (dialog as IDisposable)?.Dispose();
        }

        public static async Task<string> SelectFolder(string title, string dir, Window parent)
        {
            var storage = parent.StorageProvider;
            
            var file = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
                { Title = title, AllowMultiple = false, SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(dir) });
            
            string result = null;

            if (file.Count > 0)
            {
                result = file[0].Path.AbsolutePath;
            }

            return result;
        }

        public static async Task<string> SaveFile(string title, string dir, string filter,
            Window parent, string ext = null)
        {
            var dialog = new SaveFileDialog
            {
                Title = title, Directory = dir, DefaultExtension = ext
            };

            if (filter != null)
            {
                var tmp = filter.Split('|', 2);
                dialog.Filters = new List<FileDialogFilter>
                {
                    new() { Name = tmp.First(), Extensions = new List<string> { tmp.Last() } }
                };
            }

            string result = null;
            var raw = dialog.ShowAsync(parent);
            var res = await raw;

            if (!string.IsNullOrWhiteSpace(res))
            {
                result = res;
            }
            return result;
        }
    }
}