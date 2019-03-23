using System.Windows;
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
                dataContext.ChangeScale(e.GetPosition(EventsImage).X / EventsImage.ActualWidth, e.Delta > 0);

        }

        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is EventDensityViewModel dataContext)
                dataContext.ChangeWidth(e.NewSize.Width);
        }

        private void EventDensityView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is EventDensityViewModel dataContext)
                dataContext.ChangeWidth(ActualWidth);
        }
    }
}
