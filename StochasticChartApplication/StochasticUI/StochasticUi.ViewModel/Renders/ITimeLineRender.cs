using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StochasticUi.ViewModel.Renders
{
    public interface ITimeLineRender
    {
        Task<ImageSource> RenderDataAsync(double imageWidth, long startTicks, long ticksCounts, CancellationToken token);
    }
}
