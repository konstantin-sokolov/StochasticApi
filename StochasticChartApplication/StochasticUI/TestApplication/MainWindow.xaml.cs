using System.Windows;
using System.Windows.Input;

namespace TestApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //todo remove data context
        private MainWindowViewModel _dataContext;
        public MainWindow()
        {
            InitializeComponent();
            _dataContext = new MainWindowViewModel();
            this.DataContext = _dataContext;
        }

        private void Image_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //todo change to behaviour
            _dataContext.ChangeScale(e.Delta, e.GetPosition(EventsImage));
        }
    }
}
