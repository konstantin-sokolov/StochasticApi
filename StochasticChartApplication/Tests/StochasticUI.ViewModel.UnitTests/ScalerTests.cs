using System;
using FluentAssertions;
using NUnit.Framework;
using StochasticUi.ViewModel.Scale;

namespace StochasticUI.ViewModel.UnitTests
{
    [TestFixture]
    public class ScalerTests
    {
        private const long TEST_MIN_SIZE = 10;

        [OneTimeSetUp]
        public void Init()
        {
        }

        [Test]
        public void Scaler_CheckInitialState()
        {
            var scaler = new Scaler();
            scaler.Init(0, 100, TEST_MIN_SIZE);
            ScaleInfo expectedScaleInfo = new ScaleInfo()
            {
                CurrentStart = 0,
                CurrentStop = 100,
                CurrentWidth = 100
            };

            scaler.CanMoveLeft.Should().Be(false);
            scaler.CanMoveRight.Should().Be(false);

            scaler.MoveRight();
            var currentScaleInfo = scaler.GetCurrentScaleInfo();
            currentScaleInfo.Should().BeEquivalentTo(expectedScaleInfo);

            scaler.MoveLeft();
            currentScaleInfo = scaler.GetCurrentScaleInfo();
            currentScaleInfo.Should().BeEquivalentTo(expectedScaleInfo);
        }

        [Test]
        public void Scale_CheckStateAfterScale()
        {
            var scaler = new Scaler();
            scaler.Init(0, 100, TEST_MIN_SIZE);
            scaler.Scale(0.5, true);

            scaler.CanMoveLeft.Should().Be(true);
            scaler.CanMoveRight.Should().Be(true);

            var actual = scaler.GetCurrentScaleInfo();
            actual.CurrentStart.Should().BeGreaterThan(0);
            actual.CurrentStop.Should().BeLessThan(100);
        }


        [Test]
        public void Scale_CheckDecreaseAndIncrease()
        {
            var scaler = new Scaler();
            scaler.Init(0, 100, TEST_MIN_SIZE);
            scaler.Scale(0.5, true);
            scaler.Scale(0.5, false);
            scaler.Scale(0.5, false);

            var actual = scaler.GetCurrentScaleInfo();
            actual.CurrentStart.Should().Be(0);
            actual.CurrentStop.Should().Be(100);
        }

        [Test]
        public void MoveRight_TillTheEnd()
        {
            var scaler = new Scaler();
            scaler.Init(0, 100, TEST_MIN_SIZE);
            scaler.Scale(0.5, true);

            while (scaler.CanMoveRight)
                scaler.MoveRight();

            var actual = scaler.GetCurrentScaleInfo();
            actual.CurrentStop.Should().Be(100);
        }

        [Test]
        public void MoveLeft_TillTheEnd()
        {
            var scaler = new Scaler();
            scaler.Init(0, 100, TEST_MIN_SIZE);
            scaler.Scale(0.5, true);

            while (scaler.CanMoveLeft)
                scaler.MoveLeft();

            var actual = scaler.GetCurrentScaleInfo();
            actual.CurrentStart.Should().Be(0);
        }

        [Test]
        public void Scale_CheckLeftCenterPoint()
        {
            var scaler = new Scaler();
            scaler.Init(0, 100, TEST_MIN_SIZE);
            scaler.Scale(0, true);

            var actual = scaler.GetCurrentScaleInfo();
            actual.CurrentStart.Should().Be(0);
        }

        [Test]
        public void Scale_CheckRightCenterPoint()
        {
            var scaler = new Scaler();
            scaler.Init(0, 100, TEST_MIN_SIZE);
            scaler.Scale(1, true);

            var actual = scaler.GetCurrentScaleInfo();
            actual.CurrentStop.Should().Be(100);
        }

        [Test]
        public void Scale_CheckMinSize()
        {
            var scaler = new Scaler();
            scaler.Init(0, 100, TEST_MIN_SIZE);
            var steps = Math.Log((double) TEST_MIN_SIZE / 100, Scaler.SCALE_STEP_RATIO);
            for (int i = 0; i < steps + 1; i++)
                scaler.Scale(0.5, true);

            var actual = scaler.GetCurrentScaleInfo();
            var currentLength = (actual.CurrentStop - actual.CurrentStart);
            currentLength.Should().BeGreaterOrEqualTo(TEST_MIN_SIZE);
        }
        [Test]
        public void Scale_CheckCenterPosition()
        {
            var scaler = new Scaler();
            scaler.Init(0, 100, TEST_MIN_SIZE);
            var steps = Math.Log((double)TEST_MIN_SIZE / 100, Scaler.SCALE_STEP_RATIO);
            for (int i = 0; i < steps + 1; i++)
                scaler.Scale(0.5, true);

            var actual = scaler.GetCurrentScaleInfo();
            var currentLength = (actual.CurrentStop - actual.CurrentStart);
            currentLength.Should().BeGreaterOrEqualTo(TEST_MIN_SIZE);

            (actual.CurrentStart + currentLength / 2).Should().Be(50);
        }
    }
}