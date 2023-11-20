using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;

namespace Bercetech.Games.Fleepas
{
    public class FleepCoinsCounter : MonoBehaviour
    {
        // Variable to keep the score
        protected int _fleepCoins;
        public int FleepCoins => _fleepCoins;
        [SerializeField]
        protected TextMeshProUGUI _fleepCoinsText;
        private bool _resetCoins;
        [SerializeField]
        private float _firstCoinPointsCost = 10f;
        [SerializeField]
        private float _increasingCoinPointsCost = 20f;
        private float _nextCoinPointsCost;
        private float _cumScore;
        private int _leanTweenCoinTextId;
        private AudioSource _coinSound;


        // Defining a static shared instance variable so other scripts can access to the object
        private static FleepCoinsCounter _sharedInstance;
        public static FleepCoinsCounter SharedInstance => _sharedInstance;
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

            // Initializing variables
            InitializeCounter();
            _resetCoins = false;
        }

        protected virtual void Start()
        {
            // Subscribe to the next round event, to know if must reset the score
            SinglePlayerManager.SharedInstance.NextRoundReached.Subscribe(moveToNextRound =>
            {
                // Not reseting if the next level is reached
                _resetCoins = !moveToNextRound;
            }).AddTo(gameObject);
            _coinSound = gameObject.GetComponent<AudioSource>();

        }

        private void OnEnable()
        {

            if (_resetCoins)
            {
                InitializeCounter();
            }
            _fleepCoinsText.gameObject.transform.localScale = Vector3.one;

        }

        public void UpdateFleepCoins(float score)
        {
            _cumScore += score;
            while (_cumScore >= _nextCoinPointsCost)
            {
                _fleepCoins += 1;
                _fleepCoinsText.text = _fleepCoins.ToString();
                _cumScore -= _nextCoinPointsCost;
                _nextCoinPointsCost += _increasingCoinPointsCost;
                // Slight visual effect and sound
                _fleepCoinsText.gameObject.transform.localScale = Vector3.one;
                if (!LeanTween.isTweening(_fleepCoinsText.gameObject))
                {
                    _leanTweenCoinTextId = LeanTween.scale(_fleepCoinsText.gameObject, 2.5f * Vector3.one, 0.2f).setEaseInOutCubic().setLoopPingPong(1).id;
                    _coinSound.Play();
                }
            }
        }

        private void InitializeCounter()
        {
            _fleepCoinsText.text = "0";
            _cumScore = 0;
            _fleepCoins = 0;
            _nextCoinPointsCost = _firstCoinPointsCost;
        }

        private void OnDisable()
        {
            LeanTween.cancel(_leanTweenCoinTextId);
        }

    }
}
