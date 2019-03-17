using System.Diagnostics;
using System.Windows;

namespace StochasticUi.ViewModel
{
    public class ScaleInfo:IScaleInfo
    {
        public bool CanMoveRight { get; } = true;
        public bool CanMoveLeft { get; } = true;
        public void Scale(int delta, Point center)
        {
            Trace.WriteLine("Scale");
        }

        public void MoveLeft()
        {
            Trace.WriteLine("MoveLeft");
        }

        public void MoveRight()
        {
            Trace.WriteLine("MoveRight");
        }
    }
}
