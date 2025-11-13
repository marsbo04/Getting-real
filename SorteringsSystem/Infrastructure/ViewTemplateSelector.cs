using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SorteringsSystem.ViewModels;

namespace SorteringsSystem.Infrastructure
{
    public class ViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CardTemplate { get; set; }
        public DataTemplate ListTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // Try to find the nearest ItemsControl (the control that hosts items)
            DependencyObject current = container;
            while (current != null && !(current is ItemsControl))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            // If found, check its DataContext for the MainViewModel and read IsListView
            if (current is ItemsControl itemsControl)
            {
                if (itemsControl.DataContext is MainViewModel vm)
                {
                    return vm.IsListView ? ListTemplate : CardTemplate;
                }

                // Fallback: check hosting Window's DataContext
                DependencyObject host = itemsControl;
                while (host != null && !(host is Window))
                {
                    host = VisualTreeHelper.GetParent(host);
                }

                if (host is Window window && window.DataContext is MainViewModel vmWindow)
                {
                    return vmWindow.IsListView ? ListTemplate : CardTemplate;
                }
            }

            // Default to card template if we can't find the view model
            return CardTemplate;
        }
    }
}
