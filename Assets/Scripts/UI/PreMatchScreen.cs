using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.Localization;

namespace Bercetech.Games.Fleepas
{
    public class PreMatchScreen : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _adShowedScreenText;
        [SerializeField]
        private TextMeshProUGUI _mainPreMatchScreen;

        // Variable Strings
        private LocalizedString _adShowedString = new LocalizedString("000 - Fleepas", "ad_showed_screen");
        private LocalizedString _mainPrematchString = new LocalizedString("000 - Fleepas", "main_prematch_screen");
        


        // Update is called once per frame
        void OnEnable()
        {
            // If the Bonus is already active, go directly to play game
            if (ScoreBonus.SharedInstace.ScoreBonusIsActive)
            {
                gameObject.SetActive(false);
                StartGame.SharedInstance.RestartGame();
            }
            else
            {
                // Changing text bonus according to the configuration
                _adShowedScreenText.text = _adShowedString.GetLocalizedString((int)Math.Round(100 * (ScoreBonus.SharedInstace.ScoreBonusPercentage - 1)));
                _mainPreMatchScreen.text = _mainPrematchString.GetLocalizedString((int)Math.Round(100 * (ScoreBonus.SharedInstace.ScoreBonusPercentage - 1)));
            }
        }

    }
}
