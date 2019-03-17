using System.Windows;

namespace StochasticUi.ViewModel
{
    public interface IScaleInfo
    {
        bool CanMoveRight { get; }
        bool CanMoveLeft { get; }

        void Scale(int delta, Point center);
        void MoveLeft();
        void MoveRight();
    }
}
