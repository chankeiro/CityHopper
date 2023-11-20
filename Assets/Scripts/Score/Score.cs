using UnityEngine;
using UniRx;
using TMPro;
using System;


namespace Bercetech.Games.Fleepas
{
    public class Score : MonoBehaviour
    {

        // Variable to keep the score
        protected float _scorePoints;
        public float ScorePoints => _scorePoints;
        protected TextMeshProUGUI _scoreText;
        // To send only the final score
        protected Signal<float> _finalScoreCount = new Signal<float>();
        public Signal<float> FinalScoreCount => _finalScoreCount;
        // To send with each score update
        protected Signal<float> _scorePointsStream = new Signal<float>();
        public Signal<float> ScorePointsStream => _scorePointsStream;


        // Defining a static shared instance variable so other scripts can access to the object
        private static Score _sharedInstance;
        public static Score SharedInstance => _sharedInstance;
        protected virtual void Awake()
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

            _scoreText = GetComponent<TextMeshProUGUI>();
        }

        protected virtual void Start()
        {
            // Subscribe to the Countdown finished event, to send the final score count
            Clock.SharedInstance.CountDownFinished.Subscribe(_ =>
            {
                _finalScoreCount.Fire(_scorePoints);
            }).AddTo(gameObject);
        }

        protected virtual void OnEnable()
        {
            // Do nothing here
        }

        public void UpdateScore(float points, bool addPoints)
        {
            if (addPoints)
                _scorePoints += points;
            else
                _scorePoints = points;
            _scoreText.text = ((int)Math.Round(_scorePoints)).ToString();
            _scorePointsStream.Fire(_scorePoints);
        }
    }
}
