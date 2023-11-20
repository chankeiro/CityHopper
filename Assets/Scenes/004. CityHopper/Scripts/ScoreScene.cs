
using UnityEngine;
using UniRx;
using TMPro;
using System;


namespace Bercetech.Games.Fleepas.CityBunny
{
    public class ScoreScene : Score
    {
        private bool _resetScore = true;


        // CompositeDisposable is similar with List<IDisposable>
        // It will be used to gather all disposables active after the game is finished
        // so they can be disposed at that moment
        protected CompositeDisposable disposables = new CompositeDisposable();
        private void OnDestroy()
        {
            disposables.Clear();
        }

        // Defining a static shared instance variable so other scripts can access to the object
        private static ScoreScene _sharedInstanceScene;
        public static ScoreScene SharedInstanceScene => _sharedInstanceScene;
        override protected void Awake()
        {
            base.Awake();
            // If there is an instance, and it's not me, delete myself.
            if (_sharedInstanceScene != null && _sharedInstanceScene != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _sharedInstanceScene = this;
            }

            _scoreText = GetComponent<TextMeshProUGUI>();
        }

        protected override void Start()
        {
            // Subscribe to the state Final Event
            Bunny.EndReached.Subscribe(_ =>
            {
                _finalScoreCount.Fire(_scorePoints);
            }).AddTo(gameObject);
        }

    }
}
