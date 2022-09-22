using System;
namespace SimpleReactionMachine
{
    public class SimpleReactionController : IController
    {
        // Timer constants //
        private const int GAME_TIME = 200;
        private const int DELAY_MIN = 100;
        private const int DELAY_MAX = 250;
        private const int FINAL_DISPLAY_TIME = 300;
        private const int AVERAGE_DISPLAY_TIME = 500;
        private const int COIN_TIMEOUT = 1000;

        // Instance variables //
        private IState ControllerState;
        private int Timer;
        private IGui gui;
        private IRandom random;
        private int GameCount;
        private decimal ScoreTotal;

        // Connect controller to gui //
        public void Connect(IGui gui, IRandom rng)
        {
            this.gui = gui;
            this.random = rng;
            Init();
        }

        // Initialise to starting state (no coin) //
        public void Init()
        {
            ControllerState = new NoCoinState(this);
        }

        // Methods //
        public void CoinInserted()
        {
            ControllerState.CoinInserted();
        }

        public void GoStopPressed()
        {
            ControllerState.GoStopPressed();
        }

        public void Tick()
        {
            ControllerState.Tick();
        }

        // Base state interface //
        private interface IState
        {
            public abstract void CoinInserted();
            public abstract void GoStopPressed();
            public abstract void Tick();
        }

        // Class describing the idle state, or where no coin has been inserted //
        private class NoCoinState : IState
        {
            private SimpleReactionController _controller;

            public NoCoinState(SimpleReactionController controller)
            {
                this._controller = controller;
                _controller.gui.SetDisplay("Insert coin");
                _controller.GameCount = 0;
                _controller.ScoreTotal = 0;
            }

            public void CoinInserted()
            {
                // Move to CoinState state once a coin is inserted //
                _controller.ControllerState = new CoinState(_controller);
            }

            public void GoStopPressed()
            {
                // Nothing happens //
            }

            public void Tick()
            {
                // Nothing happens //
            }
        }

        // Class describing the state where coin has been inserted //
        private class CoinState : IState
        {
            private SimpleReactionController _controller;

            public CoinState(SimpleReactionController controller)
            {
                this._controller = controller;
                _controller.gui.SetDisplay("Press GO!");
                _controller.GameCount++;
                _controller.Timer = 0;
            }

            public void CoinInserted()
            {
                // Nothing happens //
            }

            public void GoStopPressed()
            {
                // Move to RandomDelay state if go/stop button pressed //
                _controller.ControllerState = new RandomDelay(_controller);
            }

            public void Tick()
            {
                // Return to no coin state if no go/stop pressed within 10 seconds,
                // ie. game times out //
                _controller.Timer++;
                if (_controller.Timer >= COIN_TIMEOUT)
                {
                    _controller.ControllerState = new NoCoinState(_controller);
                }
            }
        }

        // Class describing the state where the random delay is running //
        private class RandomDelay : IState
        {
            private SimpleReactionController _controller;
            private int randomNum;

            public RandomDelay(SimpleReactionController controller)
            {
                this._controller = controller;
                _controller.gui.SetDisplay("Wait...");
                randomNum = controller.random.GetRandom(DELAY_MIN, DELAY_MAX);
                _controller.Timer = randomNum;
            }

            public void CoinInserted()
            {
                // Nothing happens //
            }

            public void GoStopPressed()
            {
                // Returns to idle, or NoCoinState, state when go/stop pressed during RandomDelay state.
                // Can't reward the cheaters!! //
                _controller.ControllerState = new NoCoinState(_controller);
            }

            public void Tick()
            {
                // Counts down random delay and then moves to GamePlay state //
                _controller.Timer--;
                if (_controller.Timer <= 0)
                {
                    _controller.ControllerState = new GamePlay(_controller);
                }
            }
        }
        
        // Class describing the state where the game is running //
        private class GamePlay : IState
        {
            private SimpleReactionController _controller;

            public GamePlay(SimpleReactionController controller)
            {
                this._controller = controller;
                _controller.Timer = 0;
                _controller.gui.SetDisplay("0.00");
            }

            public void CoinInserted()
            {
                // Nothing happens //
            }

            public void GoStopPressed()
            {
                // Move to FinalTimerValue state if go/stop pressed //
                _controller.ControllerState = new FinalTimerValue(_controller);
            }

            public void Tick()
            {
                // Counts up from 0 to 2 seconds and prints this for user, records time when stopped,
                // if timer == 2 seconds it moves to FinalTimerValue state //
                _controller.Timer++;
                _controller.gui.SetDisplay(((decimal)(_controller.Timer) / 100).ToString("0.00"));
                if (_controller.Timer >= GAME_TIME)
                {
                    _controller.ControllerState = new FinalTimerValue(_controller);
                }
            }
        }

        // Class describing the state where the final timer value is displayed //
        private class FinalTimerValue : IState
        {
            private SimpleReactionController _controller;
            private readonly int finalValue;

            public FinalTimerValue(SimpleReactionController controller)
            {
                this._controller = controller;
                finalValue = _controller.Timer;
                _controller.ScoreTotal += finalValue;
                _controller.Timer = 0;
                _controller.gui.SetDisplay(((decimal)finalValue / 100).ToString("0.00"));
            }

            public void CoinInserted()
            {
                // Nothing happens //
            }

            public void GoStopPressed()
            {
                // If go/stop pressed during waiting period, immediately move to next state //
                if (_controller.GameCount != 3)
                {
                    _controller.ControllerState = new CoinState(_controller);
                }
                else
                {
                    _controller.ControllerState = new AverageScore(_controller);
                }
            }

            public void Tick()
            {
                // Prints final time and keeps this on screen for 3 seconds, then proceeds to next state //
                _controller.Timer++;
                if (_controller.Timer >= FINAL_DISPLAY_TIME)
                {
                    if (_controller.GameCount != 3)
                    {
                        _controller.ControllerState = new CoinState(_controller);
                    }
                    else
                    {
                        _controller.ControllerState = new AverageScore(_controller);
                    }
                }
            }
        }

        // Class describing the state where the average score is displayed //
        private class AverageScore : IState
        {
            private SimpleReactionController _controller;

            public AverageScore(SimpleReactionController controller)
            {
                this._controller = controller;
                _controller.Timer = 0;
                _controller.gui.SetDisplay("Average = " + (_controller.ScoreTotal / 100 / 3).ToString("0.00"));
            }

            public void CoinInserted()
            {
                // Nothing happens //
            }

            public void GoStopPressed()
            {
                // if go/stop pressed, game ends //
                _controller.ControllerState = new NoCoinState(_controller);
            }

            public void Tick()
            {
                // Prints average score and keeps this on screen for 5 seconds, then restart game //
                _controller.Timer++;
                if (_controller.Timer >= AVERAGE_DISPLAY_TIME)
                {
                    _controller.ControllerState = new NoCoinState(_controller);
                }
            }
        }
    }
}
