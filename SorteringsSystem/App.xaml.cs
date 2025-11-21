using System.Windows;
using SorteringsSystem.ApplicationLayer;
using SorteringsSystem.ViewModels;
using SorteringsSystem.Views;

namespace SorteringsSystem
{
   
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            
            string tasksPath = InMemoryTaskRepository.ResolveTextFilesTasksPath();
            var repo = new InMemoryTaskRepository(tasksPath);
            var controller = new TaskController(repo);

           
            var mainVm = new MainViewModel(controller);

            var mainWindow = new MainWindow(mainVm);
            mainWindow.Show();
        }
    }
}
