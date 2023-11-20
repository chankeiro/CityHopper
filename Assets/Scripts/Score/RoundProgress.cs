using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;


namespace Bercetech.Games.Fleepas
{
    public class RoundProgress : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _roundText;
        [SerializeField]
        protected TextMeshProUGUI _roundThresholdText;
        [SerializeField]
        private Image _barHandle;
        [SerializeField]
        private Color _colorRoundPassed;
        [SerializeField]
        private Color _colorRoundPending;
        protected float _roundScore;
        public float RoundScore => _roundScore;
        private int _nextRoundScoreThreshold;
        private float _percentage;
        private bool _roundPassed;
        private int _tweenBarId;


        private LocalizedString _roundString = new LocalizedString("000 - Fleepas", "round_value")
        {
            { "matchRound", new IntVariable { Value = 1 }}
        };
        private IntVariable _matchRoundStringVar;

        // Defining a static shared instance variable so other scripts can access to the object
        private static RoundProgress _sharedInstance;
        public static RoundProgress SharedInstance => _sharedInstance;
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
;
        }

        private void OnEnable()
        {
            // Reseting round passed Flag
            _roundPassed = false;
            // The round starts with 0 score and percentage
            _roundScore = 0;
            _percentage = 0;
            UpdateBarPercentage();
            // Setting bar text
            GenerateStringVar();
            _roundThresholdText.text = Mathf.RoundToInt(_roundScore).ToString();
            // Setting scale at 1 just in case the object was disabled in the
            // middle of the tween movement
            gameObject.transform.localScale = Vector3.one;
            
        }

        protected void UpdateBarPercentage()
        {
            _nextRoundScoreThreshold = Mathf.Max(SinglePlayerManager.SharedInstance.NextRoundScoreThreshold,1);
            _percentage = Mathf.Min((float)(_roundScore) / (float)(_nextRoundScoreThreshold ), 1);
            // Setting bar percentage
            GetComponent<Scrollbar>().size = _percentage;
            // Changing bar color and activating effect in case percetage = 1
            if (_percentage == 1)
            {
                if (!_roundPassed) {
                    _barHandle.color = _colorRoundPassed;
                    // Activating bar animation
                    _tweenBarId = LeanTween.scale(gameObject, new Vector3(0.8f,1.2f,1f), 0.25f).setEaseInOutCubic().setLoopPingPong(1).id;
                    // Playing Success sound
                    gameObject.GetComponent<AudioSource>().Play();
                    // Flagging round achieved
                    _roundPassed = true;
                }
            } else
            {
                _barHandle.color = _colorRoundPending;
            }
        }

        private void GenerateStringVar()
        {

            _roundString.StringChanged += OnStringChanged;
            _matchRoundStringVar = _roundString["matchRound"] as IntVariable;
            // Update Values
            _matchRoundStringVar.Value = SinglePlayerManager.SharedInstance.MatchRound;
        }

        private void OnStringChanged(string str)
        {
            _roundText.text =  str;
        }


        private void OnDisable()
        {
            LeanTween.cancel(_tweenBarId);
            // Need to unsubscribe to avoid repeating events
            _roundString.StringChanged -= OnStringChanged;
        }
    }
}
