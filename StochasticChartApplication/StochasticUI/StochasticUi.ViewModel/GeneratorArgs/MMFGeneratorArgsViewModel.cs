namespace StochasticUi.ViewModel.GeneratorArgs
{
    public class MMFGeneratorArgsViewModel : BaseGeneratorArgsViewModel
    {
        private string _filePath;

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public override object[] GetParameters()
        {
            return new object[] {FilePath};
        }
    }
}
