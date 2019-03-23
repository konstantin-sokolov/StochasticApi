namespace StochasticUi.ViewModel.Scale
{
    public interface IScaler
    {
        bool CanMoveRight { get; }
        bool CanMoveLeft { get; }

        void MoveLeft();
        void MoveRight();

        void Scale(double centerRelativePos, bool decrease);

        ScaleInfo GetCurrentScaleInfo();
    }
}