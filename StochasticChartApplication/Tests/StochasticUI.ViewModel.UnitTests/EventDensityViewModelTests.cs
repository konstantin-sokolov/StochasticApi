using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EventApi.Models;
using EventsApi.Contracts;
using FluentAssertions;
using Moq;
using NLog;
using NUnit.Framework;
using StochasticUi.ViewModel;
using StochasticUi.ViewModel.Renders;
using StochasticUi.ViewModel.Scale;

namespace StochasticUI.ViewModel.UnitTests
{
    [TestFixture]
    public class EventDensityViewModelTests
    {
        private Mock<IScaler> _scalerMock;
        private Mock<IDensityApi> _densityApiMock;
        private Mock<ILogger> _loggerMock;
        private Mock<ITimeLineRender> _timeLineRenderMock;
        private Mock<IChartRender> _chartRenderMock;

        [OneTimeSetUp]
        public void Init()
        {
            _scalerMock = new Mock<IScaler>();
            _densityApiMock = new Mock<IDensityApi>();
            _loggerMock = new Mock<ILogger>();
            _timeLineRenderMock = new Mock<ITimeLineRender>();
            _chartRenderMock = new Mock<IChartRender>();
            _scalerMock.Setup(t => t.GetCurrentScaleInfo()).Returns(new ScaleInfo() {CurrentStart = 50, CurrentStop = 200, CurrentWidth = 150});
        }

        [Test]
        public void CheckCanMoveRightOrLeft_ShouldBeFalse()
        {
            _scalerMock.SetupGet(s => s.CanMoveRight).Returns(false);
            _scalerMock.SetupGet(s => s.CanMoveLeft).Returns(true);

            var vm = new EventDensityViewModel(_scalerMock.Object, _densityApiMock.Object, _loggerMock.Object, _timeLineRenderMock.Object, _chartRenderMock.Object);
            vm.CanMoveRight.Should().BeFalse();
            vm.CanMoveLeft.Should().BeTrue();
        }

        [Test]
        public async Task CheckChangeWidth_ShouldCallRender()
        {
            _timeLineRenderMock.Setup(t => t.RenderDataAsync(It.IsAny<double>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult((ImageSource)new BitmapImage())); ;
            var vm = new EventDensityViewModel(_scalerMock.Object, _densityApiMock.Object, _loggerMock.Object, _timeLineRenderMock.Object, _chartRenderMock.Object);
            vm.ChangeWidth(100);
            await Task.Delay(300);
            _timeLineRenderMock.Verify(t => t.RenderDataAsync(It.IsAny<double>(),It.IsAny<long>(),It.IsAny<long>(),It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task CheckChangeWidth_CheckThrottling()
        {
            _timeLineRenderMock.Setup(t => t.RenderDataAsync(It.IsAny<double>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult((ImageSource)new BitmapImage())); ;
            var vm = new EventDensityViewModel(_scalerMock.Object, _densityApiMock.Object, _loggerMock.Object, _timeLineRenderMock.Object, _chartRenderMock.Object);
            for (int i = 0; i < 10; i++)
                vm.ChangeWidth(i*15);

            await Task.Delay(300);
            _timeLineRenderMock.Verify(t => t.RenderDataAsync(It.IsAny<double>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.AtMost(10));
        }

        [Test]
        public void CheckChangeScale()
        {
            _timeLineRenderMock.Setup(t => t.RenderDataAsync(It.IsAny<double>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult((ImageSource)new BitmapImage()));
            _chartRenderMock.Setup(t => t.RenderDataAsync(It.IsAny<IEnumerable<DensityInfo>>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult((ImageSource)new BitmapImage())); 
            _scalerMock.Setup(t => t.Scale(It.IsAny<double>(), It.IsAny<bool>()));
            _densityApiMock.Setup(t => t.GetDensityInfoAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new List<DensityInfo>
            {
                new DensityInfo() {EventsCount = 1, Start = 10, StartIndex = 0, Stop = 100, StopIndex = 1}
            }));

            var vm = new EventDensityViewModel(_scalerMock.Object, _densityApiMock.Object, _loggerMock.Object, _timeLineRenderMock.Object, _chartRenderMock.Object);
            vm.ChangeWidth(100);
            vm.ChangeScale(0.5, true);

            _timeLineRenderMock.Verify(t => t.RenderDataAsync(It.IsAny<double>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _chartRenderMock.Verify(t => t.RenderDataAsync(It.IsAny<IEnumerable<DensityInfo>>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _scalerMock.Verify(t => t.Scale(It.IsAny<double>(), It.IsAny<bool>()), Times.AtLeastOnce);
            _densityApiMock.Verify(t => t.GetDensityInfoAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}