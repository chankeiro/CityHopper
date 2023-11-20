using UnityEngine;
using UniRx;
using System;

namespace Bercetech.Games.Fleepas
{
    public class StartGame : MonoBehaviour
    {
        [SerializeField]
        private GameObject _countDownStart;
        [SerializeField]
        private float _audioPitchNumbers = 1.5f;
        [SerializeField]
        private float _audioPitchGo = 2f;

        // Publish the start game click
        private Signal _startGameEvent = new Signal();
        public Signal StartGameEvent => _startGameEvent;

        // Publish the activate game event
        private Signal _activateGameEvent = new Signal();
        public Signal ActivateGameEvent => _activateGameEvent;

        [SerializeField]
        private GameObject _hostStartGameScreen;
        [SerializeField]
        private GameObject _noHostAfterLocalizationGameStoppedScreen;

        // Defining a static shared instance variable so other scripts can access to the object
        private static StartGame _sharedInstance;
        public static StartGame SharedInstance => _sharedInstance;

        void Awake()
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

        }

        public void RestartGame()
        {
            switch (PlayerMode.SharedInstance.GameType)
            {
                case 0: // Single Player: New game
                    // Enable Single Player manager
                    StartPlaying();
                    break;
                case 1: // Multiplayer: New Game
                    _hostStartGameScreen.SetActive(true);
                    break;
                case 2: // Multiplayer: Join Game
                    // Show waiting screen

                    _noHostAfterLocalizationGameStoppedScreen.SetActive(true);
                    break;
            }

        }

        public void StartPlaying()
        {
            // Starting CountDown
            Logging.Omigari("Game is Started");
            _startGameEvent.Fire();
            var audioSource = _countDownStart.GetComponent<AudioSource>();
            // Increasing pitch
            audioSource.pitch = _audioPitchNumbers;
            // Showing Countdown Start
            _countDownStart.transform.Find("Number3").gameObject.SetActive(true);
            audioSource.PlayDelayed(0.1f);
            Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
            {
                _countDownStart.transform.Find("Number2").gameObject.SetActive(true);
                audioSource.PlayDelayed(0.1f);
            }).AddTo(gameObject);
            Observable.Timer(TimeSpan.FromSeconds(2f)).Subscribe(_ =>
            {
                _countDownStart.transform.Find("Number1").gameObject.SetActive(true);
                audioSource.PlayDelayed(0.1f);
            }).AddTo(gameObject);
            Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(_ =>
            {
                _countDownStart.transform.Find("NumberGO").gameObject.SetActive(true);
                // Increasing pitch more for the last beep
                audioSource.pitch = _audioPitchGo;
                audioSource.PlayDelayed(0.1f);
            }).AddTo(gameObject);
            Observable.Timer(TimeSpan.FromSeconds(4f)).Subscribe(_ =>
            {
                // Actvivating game 
                _activateGameEvent.Fire();
            }).AddTo(gameObject);
        }
    }
}

