using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using System;

namespace Bercetech.Games.Fleepas
{
    public class SinglePlayerFinishedGameScreen : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _scoreText;
        [SerializeField]
        private GameObject _tryAgainButton;
        [SerializeField]
        private int _noTryAgainButtonHeight;
        [SerializeField]
        private int _withTryAgainButtonHeight;
        [SerializeField]
        private TextMeshProUGUI _restartMatchText;
        [SerializeField]
        private Color _noTryAgainRestartColor;
        [SerializeField]
        private Color _withTryAgainRestartColor;
        [SerializeField]
        private GameObject _mainPreMatchScreen;
        [SerializeField]
        private GameObject _singlePlayerRestartingRoundFailed;

        // Variable Strings
        private LocalizedString _finalScoreString = new LocalizedString("000 - Fleepas", "game_over_message")
        {
            { "matchRound", new IntVariable { Value = 1 }},
            { "roundScore", new IntVariable { Value = 1 }},
            { "nextRoundScoreThreshold", new IntVariable { Value = 1 }},
            { "totalScore", new IntVariable { Value = 1 }}
        };
        private IntVariable _matchRoundStringVar;
        private IntVariable _roundScoreStringVar;
        protected IntVariable _nextRoundScoreThresholdStringVar;
        private IntVariable _totalScoreStringVar;

        void OnEnable()
        {
            // Next function On Enable because we need to subscribe and update values at the same time
            GenerateStringVar();
            // Show Retry button in case user can retry
            if (SinglePlayerManager.SharedInstance.UserCanRetry)
            {
                _tryAgainButton.SetActive(true);
                gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _withTryAgainButtonHeight);
                // Change restart color to make it less appealing to user
                _restartMatchText.color = _withTryAgainRestartColor;
            }
            else
            {
                _tryAgainButton.SetActive(false);
                gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _noTryAgainButtonHeight);
                // Change restart color to make it more appealing to user
                _restartMatchText.color = _noTryAgainRestartColor;
            }
        }

        public void StartNewMatch()
        {
            if (SinglePlayerManager.SharedInstance.UserCanRetry)
            {
                _singlePlayerRestartingRoundFailed.SetActive(true);
            }
            else
            {
                SinglePlayerManager.SharedInstance.StartNewMatchAndNotRetry();
                _mainPreMatchScreen.SetActive(true);
            }
        }

        
        private void GenerateStringVar()
        {

            _finalScoreString.StringChanged += OnStringChanged;
            _matchRoundStringVar = _finalScoreString["matchRound"] as IntVariable;
            _roundScoreStringVar = _finalScoreString["roundScore"] as IntVariable;
            _nextRoundScoreThresholdStringVar = _finalScoreString["nextRoundScoreThreshold"] as IntVariable;
            _totalScoreStringVar = _finalScoreString["totalScore"] as IntVariable;
            
            // Update Values
            _matchRoundStringVar.Value = SinglePlayerManager.SharedInstance.MatchRound;
            _roundScoreStringVar.Value = SinglePlayerManager.SharedInstance.RoundScore;
            _nextRoundScoreThresholdStringVar.Value = SinglePlayerManager.SharedInstance.NextRoundScoreThreshold;
            _totalScoreStringVar.Value = (int)Math.Round(Score.SharedInstance.ScorePoints);

        }

        private void OnStringChanged(string str)
        {
            _scoreText.text = str;
        }

        private void OnDisable()
        {
            // Need to unsubscribe to avoid repeating events
            _finalScoreString.StringChanged -= OnStringChanged;
        }
    }
}