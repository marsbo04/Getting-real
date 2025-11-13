using System;

namespace SorteringsSystem.ViewModels
{
    public interface IDialogRequestClose
    {
        event Action<bool?>? RequestClose;
    }
}