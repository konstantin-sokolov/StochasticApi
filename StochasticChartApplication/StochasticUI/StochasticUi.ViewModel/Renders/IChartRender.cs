using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using EventApi.Models;

namespace StochasticUi.ViewModel.Renders
{
    public interface IChartRender
    {
        Task<ImageSource> RenderDataAsync(IEnumerable<DensityInfo> densities, long startTicks, long ticksCount, CancellationToken token);
    }
}
