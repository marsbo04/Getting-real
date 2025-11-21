using System.Windows;
using SorteringsSystem.ApplicationLayer;
using SorteringsSystem.ViewModels;
using SorteringsSystem.Views;

namespace SorteringsSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Resolve Tasks.txt path inside TexstFiles and create a single repository instance.
            string tasksPath = InMemoryTaskRepository.ResolveTextFilesTasksPath();
            var repo = new InMemoryTaskRepository(tasksPath);
            var controller = new TaskController(repo);

            // Create MainViewModel with the controller so UI operations persist via the file-backed repository.
            var mainVm = new MainViewModel(controller);

            // Show main window with injected view model.
            var mainWindow = new MainWindow(mainVm);
            mainWindow.Show();
        }
    }
}
