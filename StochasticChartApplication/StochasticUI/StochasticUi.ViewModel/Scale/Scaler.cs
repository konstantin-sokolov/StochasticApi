using System;

namespace StochasticUi.ViewModel.Scale
{
    public class Scaler : IScaler
    {
        public const double MOVE_STEP_RATIO = 0.05;
        public const double SCALE_STEP_RATIO = 0.6;

        private long _globalStart;
        private long _globalStop;
        private long _globalSize;

        private long _currentStart;
        private long _currentStop;
        private long _currentSize;
        private long _minSize;

        public bool CanMoveRight => _currentStop < _globalStop;
        public bool CanMoveLeft => _currentStart > _globalStart;

        public void MoveLeft()
        {
            var length = _currentStop - _currentStart;
            var step = Math.Min(_currentStart - _globalStart, (long) (length * MOVE_STEP_RATIO));
            _currentStart -= step;
            _currentStop -= step;
        }

        public void MoveRight()
        {
            var length = _currentStop - _currentStart;
            var step = Math.Min(_globalStop - _currentStop, (long) (length * MOVE_STEP_RATIO));
            _currentStart += step;
            _currentStop += step;
        }

        public void Scale(double relativePos, bool decrease)
        {
            var newSize = decrease
                ? Math.Max((long) (_currentSize * SCALE_STEP_RATIO), _minSize)
                : Math.Min((long) (_currentSize / SCALE_STEP_RATIO), _globalSize);

            long center = _currentStart + (long) (_currentSize * relativePos);
            _currentSize = newSize;
            if ((center + newSize / 2) > _globalStop)
            {
                _currentStop = _globalStop;
                _currentStart = _currentStop - newSize;
                return;
            }

            if ((center - newSize / 2) < _globalStart)
            {
                _currentStart = _globalStart;
                _currentStop = _currentStart + newSize;
                return;
            }

            _currentStart = center - newSize / 2;
            _currentStop = center + newSize / 2;
        }

        public void Init(long globalStart, long globalStop, long minSize)
        {
            _globalStart = globalStart;
            _globalStop = globalStop;
            _currentStart = _globalStart;
            _currentStop = _globalStop;
            _globalSize = _globalStop - _globalStart;
            _currentSize = _currentStop - _currentStart;
            _minSize = minSize;
        }

        public ScaleInfo GetCurrentScaleInfo()
        {
            return new ScaleInfo()
            {
                CurrentStart = _currentStart,
                CurrentStop = _currentStop
            };
        }
    }
}
