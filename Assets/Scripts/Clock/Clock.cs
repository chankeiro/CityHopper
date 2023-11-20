using System.Collections;
using System;
using UnityEngine;
using UniRx;
using TMPro;



namespace Bercetech.Games.Fleepas
{
    public class Clock : MonoBehaviour
    {
        [SerializeField]
        private ClockMode _clockMode = ClockMode.CountDown;
        [SerializeField]
        private int _secondsClockFormatChange = 100;
        [SerializeField]
        protected int _totalSecondsSP;
        public int TotalSecondsSP => _totalSecondsSP;
        [SerializeField]
        private int _totalSecondsMP;
        private int _totalSeconds;
        public int TotalSeconds => _totalSeconds;
        protected TextMeshProUGUI _clockText;
        protected Signal _countDownFinished = new Signal();
        public Signal CountDownFinished => _countDownFinished;

        // Time passed
        protected int _timePassed;
        public int TimePassed => _timePassed;

        // Signal
        private Signal<int> _timePassedMessage = new Signal<int>();
        public Signal<int> TimePassedMessage => _timePassedMessage;
        // Stop Clock Signal
        protected static Signal _stopClock = new Signal();
        // Resume Clock Signal
        private Signal _resumeClock = new Signal();

        private enum ClockMode
        {
            CountDown,
            Timer
        }


        // Defining a shared instance variable just to apply the singleton. No need
        // to make it static because no other script needs access to other variables from Clock,
        // but the two Signals variables, which must be static anyway, because this Clock is enabled
        // after the other scripts subscribe to that Countdown Signal, but they could only subscribe
        // through this sharedInstance if the sharedInstance itself exists at the moment of subscribal.
        // Using the singleton pattern anyway to ensure that only one clock instance can be created.
        // Defining a static shared instance variable so other scripts can access to the object
        private static Clock _sharedInstance;
        public static Clock SharedInstance => _sharedInstance;
        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.
            if (_sharedInstance != null && _sharedInstance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _sharedInstance = this;
            }
            // Getting text component in advance
            _clockText = GetComponent<TextMeshProUGUI>();

        }

        protected virtual void Start() { }

        private void OnEnable()
        {
            // TO DO: unify PlayerMode and PlayerModeGeospatial 
            // and singlePlayerManager and singlePlayerManagerGeospatial
            if (PlayerMode.SharedInstance != null)
            {
                // Setting the clock time depending on the player mode
                if (PlayerMode.SharedInstance.GameType == 0)
                    _totalSeconds = _totalSecondsSP;
                else
                    _totalSeconds = _totalSecondsMP;
            }

            if (PlayerModeGeospatial.SharedInstance != null)
            {
                // Setting the clock time depending on the player mode
                if (PlayerModeGeospatial.SharedInstance.GameType == 0)
                    _totalSeconds = _totalSecondsSP;
                else
                    _totalSeconds = _totalSecondsMP;
            }

            // Starting time variables
            _timePassed = 0;

            // Generating  countdown
            Observable.Interval(TimeSpan.FromMilliseconds(500)).StartWith(0)
                .TakeUntil(_countDownFinished) // Stop when the countdowns is finished
                .TakeUntil(_stopClock) // or when the game is restarted before finishing
                .TakeUntilDisable(this) // or when this clock is disable (like when prewarming)
                .Subscribe(_ =>
            {
                WriteClock();
                // All users send internal event with timepassed
                _timePassedMessage.Fire(_timePassed);

                // Check if countdown is finished
                if (_timePassed >= _totalSeconds * 1000 && _clockMode == ClockMode.CountDown)
                {
                    Logging.Omigari("Game is finished");
                    _countDownFinished.Fire();
                }

                _timePassed += 500;

            }).AddTo(gameObject);


        }

        protected virtual void OnDisable() { }

        private void WriteClock()
        {
            var milliseconds = 0;
            if (_clockMode == ClockMode.CountDown)
                // Adding almost one second to the clock. Otherwise, for example, on 59s.999ms, the clock will
                // paint 59s, instead of 1:00. We want the clock to change when the second is finished.
                milliseconds = _totalSeconds * 1000 + 999 - _timePassed;
            else
                milliseconds = _timePassed;
            TimeSpan clock = TimeSpan.FromMilliseconds(milliseconds);
            if (clock.Seconds < _secondsClockFormatChange)
            {
                _clockText.text = Mathf.FloorToInt(milliseconds / 1000f).ToString();
                return;
            }
            if (clock.Minutes < 10)
            {
                _clockText.text = clock.ToString(@"m\:ss");
                return;
            }
            _clockText.text = clock.ToString(@"mm\:ss");
        }


        public void StopClock()
        {
            _stopClock.Fire();
        }

        public void PauseClock()
        {
            // Let's simulate that the clock is stopped reducing the clock increment
            // in the same amount as in the base script
            Observable.Interval(TimeSpan.FromMilliseconds(500)).StartWith(0)
            .TakeUntil(_stopClock) // or when the game is restarted before finishing
            .TakeUntil(_resumeClock) // or when somethings asks to resumen
            .TakeUntilDisable(this) // or when this clock is disabled (like when prewarming)
            .Subscribe(_ =>
            {
                _timePassed -= 500;
            }).AddTo(gameObject);
        }

        public void ResumeClock()
        {
            _resumeClock.Fire();
        }

    }
}
