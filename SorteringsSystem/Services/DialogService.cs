using System;
using System.Windows;
using SorteringsSystem.ViewModels;

namespace SorteringsSystem.Services
{
    // To fix CS0111, ensure that there is only one ShowDialog method with the same parameter types in DialogService.
    // If you have another ShowDialog(Window window, IDialogRequestClose viewModel) method elsewhere in this class or in a partial class file, remove or rename one of them.
    // If you intended to overload ShowDialog, change the parameter types or method name.

    // Check for duplicate ShowDialog methods in DialogService or its partial class files.
    // Remove or rename any additional ShowDialog(Window window, IDialogRequestClose viewModel) methods so only one remains.

    public class DialogService
    {
        // Only one ShowDialog method with these parameter types should exist.
        public bool? ShowDialog(Window window, IDialogRequestClose viewModel)
        {
            bool? result = null;

            void Handler(bool? r)
            {
                result = r;
                window.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        window.DialogResult = r;
                    }
                    catch (InvalidOperationException)
                    {
                        window.Close();
                    }
                }));
            }

            viewModel.RequestClose += Handler;
            try
            {
                window.DataContext = viewModel;
                var dialogResult = window.ShowDialog();
                return dialogResult ?? result;
            }
            finally
            {
                viewModel.RequestClose -= Handler;
            }
        }
    }
}