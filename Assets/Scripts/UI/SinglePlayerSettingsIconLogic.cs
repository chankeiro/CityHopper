using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Bercetech.Games.Fleepas
{
    public class SinglePlayerSettingsIconLogic : MonoBehaviour
    {

        [SerializeField]
        private GameObject _iconNoGame;
        [SerializeField]
        private GameObject _iconGame;

        // Start is called before the first frame update
        void Start()
        {
            // Switching icons when the game starts
            StartGame.SharedInstance.ActivateGameEvent.Subscribe(_ =>
            {
                _iconNoGame.SetActive(false);
                _iconGame.SetActive(true);
            }).AddTo(gameObject);
            // Switching back icons when the game ends
            Score.SharedInstance.FinalScoreCount.Subscribe(_ =>
            {
                _iconNoGame.SetActive(true);
                _iconGame.SetActive(false);
            }).AddTo(gameObject);
            // or when the restart game ad is shown
            MediationAd.AdShowed.Subscribe(adResult =>
            {
                if (adResult == MediationAd.AdResult.RestartGame)
                {
                    _iconNoGame.SetActive(true);
                    _iconGame.SetActive(false);
                }
            }).AddTo(gameObject);
        }

    }
}
