using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SorteringsSystem.ViewModels;

namespace SorteringsSystem.Views
{
    public class ViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? CardTemplate { get; set; }
        public DataTemplate? ListTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
          
            DependencyObject current = container;
            while (current != null && !(current is ItemsControl))
            {
                current = VisualTreeHelper.GetParent(current);
            }

          
            if (current is ItemsControl itemsControl)
            {
                if (itemsControl.DataContext is MainViewModel vm)
                {
                    return vm.IsListView ? ListTemplate ?? CardTemplate! : CardTemplate!;
                }

              
                DependencyObject host = itemsControl;
                while (host != null && !(host is Window))
                {
                    host = VisualTreeHelper.GetParent(host);
                }

                if (host is Window window && window.DataContext is MainViewModel vmWindow)
                {
                    return vmWindow.IsListView ? ListTemplate ?? CardTemplate! : CardTemplate!;
                }
            }

           
            return CardTemplate!;
        }
    }
}