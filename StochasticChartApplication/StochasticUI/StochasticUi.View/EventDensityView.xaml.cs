using System.Windows.Controls;
using System.Windows.Input;
using StochasticUi.ViewModel;

namespace StochasticUi.View
{
    /// <summary>
    /// Interaction logic for EventDensityView.xaml
    /// </summary>
    public partial class EventDensityView : UserControl
    {
        public EventDensityView()
        {
            InitializeComponent();
        }

        private void Image_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is EventDensityViewModel dataContext)
                dataContext.ChangeScale(e.Delta, e.GetPosition(EventsImage));

        }
    }
}
