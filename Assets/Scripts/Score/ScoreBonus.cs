using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System;

namespace Bercetech.Games.Fleepas
{
    // This script is kept in a different one than Score, becaise it must be
    // awaken on the Scene load, since other scripts depend on it
    public class ScoreBonus : MonoBehaviour
    {

        // Score Bonus: when watching add or sharing game video
        [SerializeField]
        private float _scoreBonusPercentage;
        public float ScoreBonusPercentage => _scoreBonusPercentage;
        [SerializeField]
        private GameObject _scoreBonusIcon;
        private bool _scoreBonusIsActive;
        public bool ScoreBonusIsActive => _scoreBonusIsActive;
        private Signal _scoreBonusActivated = new Signal();
        public Signal ScoreBonusActivated => _scoreBonusActivated;

        // CompositeDisposable is similar with List<IDisposable>
        // It will be used to gather all disposables active after the game is finished
        // so they can be disposed at that moment
        protected CompositeDisposable disposables = new CompositeDisposable();
        private void OnDestroy()
        {
            disposables.Clear();
        }

        // Defining a static shared instance variable so other scripts can access to the object
        private static ScoreBonus _sharedInstace;
        public static ScoreBonus SharedInstace => _sharedInstace;
        void Awake()
        {
            // If there is an instance, and it's not me, delete myself.
            if (_sharedInstace != null && _sharedInstace != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _sharedInstace = this;
            }

            // Initialize score bonus variable and subscribe to events that can activate it
            _scoreBonusIsActive = false;
            SocialShareUtil.VideoShared.Subscribe(_ =>
            {
                ActivateScoreBonus();
            }).AddTo(disposables);
            MediationAd.AdShowed.Subscribe(adResult =>
            {
                if (adResult == MediationAd.AdResult.ScoreBonus ||
                adResult == MediationAd.AdResult.RestartGame)
                    ActivateScoreBonus();
            }).AddTo(disposables);
        }

        public void DeactivateScoreBonus()
        {
           // Reser score bonus at the end of the match. 
            _scoreBonusIsActive = false;
            _scoreBonusIcon.SetActive(false);
        }


        private void ActivateScoreBonus()
        {
            _scoreBonusIsActive = true;
            _scoreBonusIcon.SetActive(true);
            _scoreBonusActivated.Fire();
        }
    }
}
