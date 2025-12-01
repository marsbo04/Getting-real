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
            InMemoryTaskRepository repo = new InMemoryTaskRepository(tasksPath);
            TaskController controller = new TaskController(repo);
            controller = new TaskController(repo);


            MainViewModel mainVm = new MainViewModel(controller);

            MainWindow mainWindow = new MainWindow(mainVm);
            mainWindow.Show();
        }
    }
}
