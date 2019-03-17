using System.Windows;
using Autofac;
using EventsApi.Contracts;
using NLog;
using Prism.Autofac;
using StochasticUi.ViewModel;

namespace StochasticChartApplication.DI
{
    /// <summary>
    /// Autofac Bootstrapper 
    /// </summary>
    public class Bootstrapper : AutofacBootstrapper
    {

        /// <summary>Creates the shell or main window of the application.</summary>
        /// <returns>The shell of the application.</returns>
        /// <remarks>
        /// If the returned instance is a <see cref="T:System.Windows.DependencyObject" />, the
        /// <see cref="T:Prism.Bootstrapper" /> will attach the default <see cref="T:Prism.Regions.IRegionManager" /> of
        /// the application in its <see cref="F:Prism.Regions.RegionManager.RegionManagerProperty" /> attached property
        /// in order to be able to add regions by using the <see cref="F:Prism.Regions.RegionManager.RegionNameProperty" />
        /// attached property from XAML.
        /// </remarks>
        protected override DependencyObject CreateShell()
        {
            var mw = Container.Resolve<MainWindow>();
            mw.DataContext = Container.Resolve<MainWindowViewModel>();
            return mw;
        }

        /// <summary>Initializes the shell.</summary>
        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }

        /// <summary>
        /// Creates the <see cref="T:Autofac.ContainerBuilder" /> that will be used to create the default container.
        /// </summary>
        /// <returns>A new instance of <see cref="T:Autofac.ContainerBuilder" />.</returns>
        protected override ContainerBuilder CreateContainerBuilder()
        {
            var builder = new ContainerBuilder();

            // регистрация зависимостей в контейнере
            // должны быть здесь...
            var logger = NLog.LogManager.GetCurrentClassLogger();
            builder.RegisterInstance(logger).As<ILogger>();
            builder.RegisterType<EventApi.Implementation.EventApi>().As<IEventApi>().InstancePerLifetimeScope();
            builder.RegisterType<ScaleInfo>().As<IScaleInfo>();
            builder.RegisterType<EventDensityViewModel>();
            builder.RegisterType<MainWindowViewModel>();
            return builder;
        }
    }
}
