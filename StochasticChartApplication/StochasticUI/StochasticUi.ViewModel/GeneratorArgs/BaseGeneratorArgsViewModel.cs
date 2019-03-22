using System.Runtime.InteropServices.WindowsRuntime;
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
                if (long.TryParse(value, out long tempResult))
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
