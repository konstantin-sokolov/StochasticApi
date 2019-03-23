using Microsoft.Practices.Prism.Mvvm;

namespace StochasticUi.ViewModel.GeneratorArgs
{
    public abstract class BaseGeneratorArgsViewModel:BindableBase
    {
        private string _collectionSize;

        public string CollectionSize
        {
            get => _collectionSize;
            set
            {
                if (long.TryParse(value, out _))
                    SetProperty(ref _collectionSize, value);
            }
        }

        public long GetCollectionSize()
        {
            return long.Parse(CollectionSize);
        }

        public abstract object[] GetParameters();
    }
}
