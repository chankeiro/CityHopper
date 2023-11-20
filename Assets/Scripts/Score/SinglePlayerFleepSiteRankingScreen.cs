using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UniRx;
using System;
using UnityEngine.Localization;


namespace Bercetech.Games.Fleepas
{
    public class SinglePlayerFleepSiteRankingScreen : MonoBehaviour
    {
        [SerializeField]
        protected TextMeshProUGUI _bestRankingText;
        [SerializeField]
        protected TextMeshProUGUI _bestScoreText;
        [SerializeField]
        protected TextMeshProUGUI _currentScoreText;
        [SerializeField]
        protected TextMeshProUGUI _currentRankingText;
        [SerializeField]
        private Scrollbar _currentRankingScrollbar;
        [SerializeField]
        private GameObject _prizesText;
        private AudioSource _currentPositionScrollbarAudio;
        private LTDescr _tweenRanking;
        private int _fleepSitePreviousRanking;
        private int _fleepSiteCurrentRanking;

        // Variable Strings
        private LocalizedString _currentScoreString = new LocalizedString("000 - Fleepas", "current_score_label");
        private LocalizedString _bestScoreString = new LocalizedString("000 - Fleepas", "best_score_label");
        private LocalizedString _bestRankingString = new LocalizedString("000 - Fleepas", "best_ranking_label");
        private LocalizedString _prizesWonString = new LocalizedString("000 - Fleepas", "prizes_won_title");
        private LocalizedString _prizesLeadString = new LocalizedString("000 - Fleepas", "prizes_lead_title");
        private LocalizedString _prizesOtherString = new LocalizedString("000 - Fleepas", "prizes_other_title");
        private LocalizedString _pointsString = new LocalizedString("000 - Fleepas", "points_label");
        private LocalizedString _positionString = new LocalizedString("000 - Fleepas", "position_label");
        private LocalizedString _andString = new LocalizedString("000 - Fleepas", "and_label");
        private LocalizedString _betweenPositionsString = new LocalizedString("000 - Fleepas", "between_positions_label");
        private LocalizedString _prizeAmountLimitString = new LocalizedString("000 - Fleepas", "prize_amount_limit_label");
        private LocalizedString _noPrizesString = new LocalizedString("000 - Fleepas", "no_prizes_configured_message");


        private void Awake()
        {

            // Subscribing to current game ranking event
            SinglePlayerManager.SharedInstance.CurrentGameNewRanking.Subscribe(newRanking =>
            {
                SetFleepSiteRankingCurrentGame(newRanking);
            }).AddTo(gameObject);
            // Getting audio component
            _currentPositionScrollbarAudio = _currentRankingScrollbar.GetComponent<AudioSource>();

            // Subscribing to game started to reset the current ranking in case we are in the first round
            StartGame.SharedInstance.ActivateGameEvent.Subscribe(_ =>
            {
                if (SinglePlayerManager.SharedInstance.MatchRound == 1) _fleepSiteCurrentRanking = 0;
            }).AddTo(gameObject);
            // Getting audio component
            _currentPositionScrollbarAudio = _currentRankingScrollbar.GetComponent<AudioSource>();
        }



        void OnEnable()
        {
            // Update Scores
            var currentBestScore = PlayerMode.SharedInstance.UserFleepSiteTopScore;
            _currentScoreText.text = _currentScoreString.GetLocalizedString() + ": " + (int)Math.Round(Score.SharedInstance.ScorePoints);
            _bestRankingText.text = _bestRankingString.GetLocalizedString() + ": " + HelperFunctions.GetOrdinalPosition(PlayerMode.SharedInstance.UserFleepSiteRanking);
            _bestScoreText.text = _bestScoreString.GetLocalizedString() + ": " + currentBestScore;
            // Position slider values
            _currentRankingText.text = "";
            // The new ranking might be worse than the previous in case there are more players at the same time
            if (_fleepSiteCurrentRanking <= _fleepSitePreviousRanking)
            {
                for (int i = _fleepSiteCurrentRanking; i <= _fleepSitePreviousRanking; i++)
                {
                    _currentRankingText.text += HelperFunctions.GetOrdinalPosition(i) + "\n";
                }
                // Sliding down to up
                _tweenRanking = LeanTween.value(0, 1f, 2.5f).setEaseInOutCubic().setOnUpdate((float val) =>
                {
                    _currentRankingScrollbar.value = val;
                    if ((Math.Round((_fleepSitePreviousRanking - _fleepSiteCurrentRanking) * val, 1) + 0.5) % 1 == 0)
                        _currentPositionScrollbarAudio.Play();
                });
            } else
            {
                for (int i = _fleepSitePreviousRanking; i <= _fleepSiteCurrentRanking; i++)
                {
                    _currentRankingText.text += HelperFunctions.GetOrdinalPosition(i) + "\n";
                }
                // Sliding up to down
                _tweenRanking = LeanTween.value(1, 0f, 2.5f).setEaseInOutCubic().setOnUpdate((float val) =>
                {
                    _currentRankingScrollbar.value = val;
                    if ((Math.Round((_fleepSiteCurrentRanking - _fleepSitePreviousRanking) * val, 1) + 0.5) % 1 == 0)
                        _currentPositionScrollbarAudio.Play();
                });
            }
            // Scale Prizes Text
            _prizesText.transform.localScale = Vector3.zero;
            Observable.Timer(TimeSpan.FromSeconds(1f)).TakeUntilDisable(gameObject).Subscribe(_ =>
            {
                _prizesText.LeanScale(Vector3.one, 1f).setEaseOutBack();
            }).AddTo(gameObject);

            // Set Screen Height depending on the number of prizes
            gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Min(800f + PlayerMode.SharedInstance.FleepSitePrizes.Count*150f, 1500));

            // In case of FleepSite, showing current ranking and list of prizes in case there are
            if (PlayerMode.SharedInstance.FleepSitePrizes.Count > 0)
            {
                // Clasiffying prizes in
                // 1.WON: don't depend on final position, only by points. Addittionally it is necessary
                // to check the user is within the list of granted prizes in case there is an amount limit
                // 2.LEAD: By position + min points
                // 3.PENDING : Res of prizes
                List<FleepSitePrize> wonPrizes = new List<FleepSitePrize>();
                List<FleepSitePrize> leadPrizes = new List<FleepSitePrize>();
                List<FleepSitePrize> pendingPrizes = new List<FleepSitePrize>();

                foreach (var prize in PlayerMode.SharedInstance.FleepSitePrizes)
                {
                    // WON PRIZES
                    if (currentBestScore >= prize.MinPoints // The Top Score is higher than the Minimum Points required for the prize
                        && (prize.MaxNumber == 0  // And there is no prize amount limit
                        || PlayerMode.SharedInstance.UserFleepSitePrizesGranted.Contains(prize.PrizeId)
                        || SinglePlayerManager.SharedInstance.MatchGrantedPrizes.Contains(prize.PrizeId) // or there is limit, but the prize has been granted to the user
                        ) 
                        && prize.MinPosition == 0 // And there is not position requirement
                    )
                    {
                        wonPrizes.Add(prize);
                        continue;
                    }

                    // LEAD PRIZES
                    if (currentBestScore >= prize.MinPoints // The Top Score is higher than the Minimum Points Points required for the prize
                        && (prize.MinPosition == PlayerMode.SharedInstance.UserFleepSiteRanking // And the user is currently in the required position 
                        || (prize.MinPosition <= PlayerMode.SharedInstance.UserFleepSiteRanking // or in the required position range
                        && prize.MaxPosition >= PlayerMode.SharedInstance.UserFleepSiteRanking))
                    )
                    {
                        leadPrizes.Add(prize);
                        continue;
                    }
                    // PENDING PRIZES
                    if (prize.MaxNumber == 0 || prize.PrizesLeft > 0)
                        pendingPrizes.Add(prize); // Rest of cases with some prize left or no limit

                }

                // Generate text based on the prizes classification
                var wonText = "";
                var leadText = "";
                var pendingText = "";
                if (wonPrizes.Count > 0)
                {
                    wonText = "<sprite name=\"cup\"> <color=#068569FF><b>"+_prizesWonString.GetLocalizedString()+"</b></color>";
                    foreach (var prize in wonPrizes)
                    {
                        wonText += "\n· ";
                        wonText += prize.MinPoints + " " + _pointsString.GetLocalizedString();
                        wonText += ": " + prize.Description;
                    }
                }
                if (leadPrizes.Count > 0)
                {
                    if (wonPrizes.Count > 0) leadText += "\n\n";
                    leadText += "<sprite name=\"ranking\"> <color=#FFCE69FF><b>" + _prizesLeadString.GetLocalizedString() + "</b></color>";
                    foreach (var prize in leadPrizes)
                    {
                        leadText += "\n· ";
                        if (prize.MaxPosition > 0)
                        {
                            leadText += _betweenPositionsString.GetLocalizedString(HelperFunctions.GetOrdinalPosition(prize.MinPosition, true), HelperFunctions.GetOrdinalPosition(prize.MaxPosition, true));
                        }
                        else
                        {
                            leadText += HelperFunctions.GetOrdinalPosition(prize.MinPosition, true) + " " + _positionString.GetLocalizedString();
                        }
                        if (prize.MinPoints > 0) leadText += " " + _andString.GetLocalizedString() + " " + prize.MinPoints + " " + _pointsString.GetLocalizedString();
                        leadText += ": " + prize.Description;
                    }
                }
                if (pendingPrizes.Count > 0)
                {
                    if (wonPrizes.Count > 0 || leadPrizes.Count > 0) pendingText += "\n\n";
                    pendingText += "<sprite name=\"goal\"> <color=#FAAFC7FF><b>" + _prizesOtherString.GetLocalizedString() + "</b></color>";
                    foreach (var prize in pendingPrizes)
                    {
                        pendingText += "\n· ";
                        if (prize.MaxPosition > 0)
                        {
                            pendingText += _betweenPositionsString.GetLocalizedString(HelperFunctions.GetOrdinalPosition(prize.MinPosition, true), HelperFunctions.GetOrdinalPosition(prize.MaxPosition, true));
                        }
                        else if (prize.MinPosition > 0)
                        {
                            pendingText += HelperFunctions.GetOrdinalPosition(prize.MinPosition, true) + " " + _positionString.GetLocalizedString();
                        }
                        if (prize.MinPosition > 0 && prize.MinPoints > 0)
                        {
                            pendingText += " "+ _andString.GetLocalizedString() + " " + prize.MinPoints + " " + _pointsString.GetLocalizedString();
                        }
                        else if (prize.MinPoints > 0)
                        {
                            pendingText += prize.MinPoints + " " + _pointsString.GetLocalizedString();
                            if (prize.MaxNumber > 0) pendingText += _prizeAmountLimitString.GetLocalizedString(prize.PrizesLeft);
                        }
                        pendingText += ": " + prize.Description;
                    }

                }
                _prizesText.GetComponent<TextMeshProUGUI>().text = wonText + leadText + pendingText;

            } else
            {
                _prizesText.GetComponent<TextMeshProUGUI>().text = _noPrizesString.GetLocalizedString();
            }
        }

        private void OnDisable()
        {
            LeanTween.cancel(_tweenRanking.id);
        }
        private void SetFleepSiteRankingCurrentGame(int newRanking)
        {
            if (_fleepSiteCurrentRanking != 0)
                _fleepSitePreviousRanking = _fleepSiteCurrentRanking;
            else _fleepSitePreviousRanking = _fleepSiteCurrentRanking + 20; // Artificially increase first ranking to provide a sense of improvement
            _fleepSiteCurrentRanking = newRanking;
        }

    }
}
