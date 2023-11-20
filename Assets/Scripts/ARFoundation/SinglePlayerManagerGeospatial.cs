// Copyright 2021 Niantic, Inc. All Rights Reserved.

using UnityEngine;
using UniRx;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;



namespace Bercetech.Games.Fleepas
{
    /// Controls the game logic and creation of objects
    public class SinglePlayerManagerGeospatial : MonoBehaviour
    {

        [SerializeField]
        protected GameObject _singlePlayerStartGame;
        [SerializeField]
        protected GameObject _singlePlayerFinishedGame;
        [SerializeField]
        private int _maxRoundTries;
        private int _roundTry;
        private bool _userCanRetry;
        public bool UserCanRetry => _userCanRetry;
        [SerializeField]
        protected GameObject _singlePlayerNextRound;
        [SerializeField]
        protected GameObject _singlePlayerFleepSiteRanking;
        [SerializeField]
        protected GameObject _singlePlayerFleepSitePrize;
        [SerializeField]
        protected TextMeshProUGUI _singlePlayerFleepSitePrizeText;
        [SerializeField]
        private GameObject _scoreClock;
        [SerializeField]
        private GameObject _gameSigns;
        [SerializeField]
        private GameObject _singlePlayerSettingsMenu;
        [SerializeField]
        private GameObject _singlePlayerLeavingGame;
        [SerializeField]
        private GameObject _finalSound;
        [SerializeField]
        private ARFManager _arfManager;
        [SerializeField]
        private CanvasGroup _fadeToBlackScreenCanvasGroup;
        [SerializeField]
        private AudioSource _music;
        private Signal _returnedToNativeApp = new Signal();
        public Signal ReturnedToNativeApp => _returnedToNativeApp;

        private bool _fleepSiteCreationStarted;
        // Match count variable
        private int _matchCount = 0;
        //private IARSession _arSession;
        private bool _isArSessionEnabled = false;
        // Match Round
        protected int _matchRound = 1;
        public int MatchRound => _matchRound;
        // Next Round event
        protected Signal<bool> _nextRoundReached = new Signal<bool>();
        public Signal<bool> NextRoundReached => _nextRoundReached;
        // Next Round score treshold event
        protected int _nextRoundScoreThreshold;
        public int NextRoundScoreThreshold => _nextRoundScoreThreshold;
        private bool _fleepSiteRankingCurrentGameResponse = false;
        // New prize screen list
        private List<string> _newPrizeScreens;
        private int _newPrizeScreenIndex;
        // Prize Ids already checked
        private List<string> _prizeIdsChecked = new List<string>();
        private Signal<int> _currentGameNewRanking = new Signal<int>();
        public Signal<int> CurrentGameNewRanking => _currentGameNewRanking;
        // Matches granted during the match
        protected List<string> _matchGrantedPrizes = new List<string>();
        public List<string> MatchGrantedPrizes => _matchGrantedPrizes;
        private int _scoreAtStart;
        private int _roundScore;
        public int RoundScore => _roundScore;
        private int _totalScore;

        // Variable Strings
        private LocalizedString _nextRoundString = new LocalizedString("000 - Fleepas", "next_round")
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

        // CompositeDisposable is similar with List<IDisposable>
        // It will be used to gather all disposables active after the game is finished
        // so they can be disposed at that moment
        protected CompositeDisposable disposables = new CompositeDisposable();

        // Defining a static shared instance variable so other scripts can access to the object
        private static SinglePlayerManagerGeospatial _sharedInstance;
        public static SinglePlayerManagerGeospatial SharedInstance => _sharedInstance;

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
        }


        protected virtual void Start()
        {

     

            // Enable AR Session
            EnableARSession();

            // Prewarming resources
            ObjectPool.PoolObjectsGenerated.Subscribe(_ => PlayerMode.SharedInstance.PoolObjectsWarming()).AddTo(disposables);
            PlayerModeGeospatial.SharedInstance.GameWarming();

            if (PlayerModeGeospatial.SharedInstance.FleepSiteId == "")
            {
            }
            else
            {
                // Load FleepSite data and screens
                StartFleepSiteLoadProcess();
            }


            // Subscribing to game started
            StartGame.SharedInstance.ActivateGameEvent.Subscribe(_ =>
            {
                // Activating clock, target generator, shooting and next paper color bottom hing
                _scoreClock.SetActive(true);
                // Deactivate Final Sound in case it was previously activated
                _finalSound.SetActive(false);
                // Increasing MatchCount on the first round
                if (_matchRound == 1)
                    _matchCount += 1;
                // Send score updates to main activity every 15s
                Observable.Interval(TimeSpan.FromSeconds(16f)).StartWith(0)
                .TakeUntil(Score.SharedInstance.FinalScoreCount)
                .TakeUntil(_nextRoundReached) // This is in case the match is restarted before it finishes
                .Subscribe(
                    // This block is run every interval and on time 0
                    _1 => sendMatchData(false, true, false) // Neither round nor match are finished, but round is started
                ).AddTo(disposables);
                // Updating score at the start and match granted prizes
                _scoreAtStart = (int)Math.Round(Score.SharedInstance.ScorePoints);
            }).AddTo(disposables);

            // Subscribing to Final Score Count event
            Score.SharedInstance.FinalScoreCount.Subscribe(_ =>
            {
                // Activate final sound
                _finalSound.SetActive(true);
                _singlePlayerSettingsMenu.SetActive(false);  // Just in case it is open
                RoundReset();
                // Define what else to do when finishing the round
                EndSinglePlayerRound();

            }).AddTo(disposables);

            // Subscribing to retry game event
            MediationAd.AdShowed.Subscribe(adResult =>
            {
                if (adResult == MediationAd.AdResult.TryAgain)
                    _roundTry += 1;
            }).AddTo(disposables);


        }

        public void ActionAfterGeospatialLocalization()
        {
            _singlePlayerStartGame.SetActive(true);
        }

        protected virtual void RoundReset()
        {
            // Deactivating clock, target generator, shooting and next paper color bottom hint
            _scoreClock.SetActive(false);
            //_gameSigns.SetActive(false);
            //ScoreBonus.SharedInstace.DeactivateScoreBonus();
        }

        
        // To restart a match at the end of the round (Use FullMatchReset if restarting in the middle of the round)
        protected virtual void MatchReset()
        {
            // Reset round. It must be done after sending the match
            // data of the previous match. Otherwise we will send a wrong round value
            _matchRound = 1;
            _matchRoundStringVar.Value = _matchRound;
            // Reset Score and PowerUps, and stop score updates
            _nextRoundReached.Fire(false);
            // Reset granted prizes (must be done after the above event is fired)
            // because the granted prizes must be copied to the fleepSite granted prizes
            // when the event is triggered
            _matchGrantedPrizes = new List<string>();
            // Setting RoundTry at 0
            _roundTry = 0;
        }

        // This function is called from the UI
        public void StartNewMatchAndNotRetry()
        {
            if (_userCanRetry)
            {
                // In case the user confirms that wants to start a new match and 
                // not retrying when having the oportunity to do so, we send a 
                // message confirming that the match is finished. Previously only a
                // message confirming that the round was finished was sent, waiting
                // to see if the user finally restarts the match or not (in this case
                // the user finally doesn't restart it)
                sendMatchData(true, true, true);
            }
            MatchReset();
        }

        // This function is called from the UI
        public void StartNewMatchAndNotContinue()
        {
            // This is called when the user has passed the round, so the next one is neither finished
            // nor started, but he decices to stop the match
            sendMatchData(true, false, false);
            MatchReset();
        }

        public void FullMatchReset()
        {
            // Needed to restart a match in the middle of one round (in addition to MatchReset)
            RoundReset();
            // Stoping clock
            Clock.SharedInstance.StopClock();
            if (_matchRound == 1)
            {
                // In case we are in the first round, the values of the match won't be processed
                // Therefore, intead of keeping a not finished match in the middle of the fleep,
                // We will overwrite it with values of the following match
                _matchCount -= 1;
            }
            else
            {
                // If we are in rounds > 1, just reset match data before starting a new one
                // In this case the round is started but not finished because we are restarting
                // in the middle of the match
                sendMatchData(true, true, false);

            }
            MatchReset();
        }

        public void EnableARSession()
        {
            // Enable AR Foundation and Show Cloud Anchor Resolver Screen
            // _arfManager.EnableARFoundation();
            // Flag session as enabled and start saving data
            OnAnyARSessionRan();
        }

        private void OnAnyARSessionRan()
        {
            if (!_isArSessionEnabled) // Controlling that this block is only run once, independently of the AR session being rerun several times
            {
                // Sending session duration and score updates to native main activity every 60s
                Observable.Interval(TimeSpan.FromSeconds(60f)).StartWith(0).Subscribe(_1 =>
                {
                    MainActivityMessagingManager.SendSessionDataToMainActivity(MainActivityMessagingManager.unityTimeMessages.SAVE_SESSION);
                }).AddTo(disposables);
                _isArSessionEnabled = true;
            }
        }

        public void ReturnToNativeApp()
        {
            // StopMusic, because otherwise we might go to the Native App and still hear the music while the Unity session is not completely destroyed
            // I don't know if there is a better way
            if (_music.isPlaying)
                _music.Stop();
            // Sending event
            _returnedToNativeApp.Fire();
            // Clear Disposables from this manager
            disposables.Clear();
            // Sending session message to app native activity
            if (_isArSessionEnabled)
            {
                // In case the AR Session was enabled, update session duration and flag the activity as finished
                MainActivityMessagingManager.SendSessionDataToMainActivity(MainActivityMessagingManager.unityTimeMessages.SESSION_ENABLED_FINISHED);
            }
            else
            {
                // In case the AR Session was NOT enabled, just flag it as finished
                MainActivityMessagingManager.SendSessionDataToMainActivity(MainActivityMessagingManager.unityTimeMessages.SESSION_NOT_ENABLED_FINISHED);
            }
            // Fading to Black           
            LeanTween.alphaCanvas(_fadeToBlackScreenCanvasGroup, 1.2f, 0.2f).setOnComplete(_ =>
            {
                // Disabling AR Foundation if it was enabled.
                //_arfManager.DisableARFoundation();
                // Unloading Unity (doesn't kill the Activity)
                SceneManager.LoadScene(0); // Coming back to the menu scene to destroy everything in the current scene
                Application.Unload();
                this.enabled = false;
            });

        }

        protected virtual void EndSinglePlayerRound()
        {

            // New Prizes and Ranking Screen. Only registered users and FleepSites.
            // (needs to be done before the "next round" logic, which may trigger the UserFleepSiteTopScore and UserFleepSitePrizesGranted values update)
            // Reset values first
            _fleepSiteRankingCurrentGameResponse = false;
            _newPrizeScreens = new List<string>();
            _newPrizeScreenIndex = 0;
            _totalScore = (int)Math.Round(Score.SharedInstance.ScorePoints);
            _totalScoreStringVar.Value = _totalScore;
            //_roundScore = (int)Math.Round(RoundProgress.SharedInstance.RoundScore);
            //_roundScoreStringVar.Value = _roundScore;
            // Check it only for registered  users in Fleepsite
            if (PlayerModeGeospatial.SharedInstance.PlayerUId != "null" // Null with string, since this is a string parameter that comes from Main Activity
                && PlayerModeGeospatial.SharedInstance.FleepSiteId != "") 
            {
                // Check if the user has got some new prize based on points
                foreach (FleepSitePrize prize in PlayerModeGeospatial.SharedInstance.FleepSitePrizes)
                {
                    // Prizes without limit
                    // First we need to check if the prize can be granted
                    if (_totalScore >= prize.MinPoints && prize.MinPosition == 0)
                    {
                        // Depending on the prize having a limit or not, the logic will be different
                        if (prize.MaxNumber == 0)
                        {
                            // We check that the user didn't get the points already in the past
                            if (Mathf.Max(PlayerModeGeospatial.SharedInstance.UserFleepSiteTopScore , _scoreAtStart) < prize.MinPoints) 
                            {
                                // Add screen to array of screens to be shown
                                _newPrizeScreens.Add(prize.PrizeId);
                            }
                        }
                        else
                        {
                            // We check if the prize was granted before reviewing the granted prizes string
                            if (!PlayerModeGeospatial.SharedInstance.UserFleepSitePrizesGranted.Contains(prize.PrizeId)
                                && !_matchGrantedPrizes.Contains(prize.PrizeId)
                                // And that we didn't already receive a response from this prizeId before during this session
                                && !_prizeIdsChecked.Contains(prize.PrizeId))
                            {
                                // In this case, we must query if the limit has been reached to know if the prize can be granted
                                // The response to that query will decide if the screen must be shown or not
                                // NO INTERNET: the prize won't be assigned. The user won't even see the ranking screen
                                // with the list of potential prizes
                                // because that screen also needs internet connection
                                MainActivityMessagingManager.CheckFleepSitePrizeLimitReached(prize.PrizeId, prize.MaxNumber);
                            }
                        }
                    }
                }

                // Checking ranking in current game too
                MainActivityMessagingManager.GetUserFleepSiteRanking((int)Math.Round(Score.SharedInstance.ScorePoints));
            }
            // Start a timer of 1 seconds. If no network response of the previous calls is received before, we'll directly move to ShowNextRoundScreen()
            // instead of showing the prizes/ranking screen
            Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
            {
                // Calculating if the user can retry or not:
                // If the number of tries is below the limit and it is not the first round
                _userCanRetry = (_roundTry < _maxRoundTries && _matchRound > 1);

                // Check if there is some Prize or Ranking screen to show
                // or show next round screens otherwise
                ShowPrizeScreen();

                // Next Round Logic
                if (_roundScore >= _nextRoundScoreThreshold)
                {
                    // Move on to the next Round
                    _matchRoundStringVar.Value = _matchRound;
                    _matchRound += 1;
                    _nextRoundReached.Fire(true);
                    // Reset round tries
                    _roundTry = 0;
                    //Neither new round (next one) nor match are finished
                    sendMatchData(false, false, false);
                }
                else
                {
                    if (_userCanRetry)
                    {
                        // If the user can retry the match cannot be considered finished until the 
                        // user decides to start a new match and not retrying. But we can already
                        // flag the round as finished here because, in case the user retries, the round 
                        // will change back to not finished. Right now it is important
                        // to flag it as finished in case the user leaves the app, so we
                        // can distinguish later if the round was actually finished or not. For the 
                        // match it is not a problem to leave it now as not finished in case the user
                        // leaves the app, because a cloud function will flag it as finished after some time.
                        // We cannot flag the match as finished here because in such case
                        // we would create a new match entry in DB, even if the user finally
                        // retries. Therefore we have to wait until be 100% sure that the match
                        // is finished to flag it that way.
                        // Of course the round is started and will be like that in case the user decides to retry
                        sendMatchData(false, true, true);
                    }
                    else
                    {
                        // If the user cannot retry, both match and round are finished. Round is also started
                        sendMatchData(true, true, true);
                    }   
                }

            }).AddTo(disposables);

        }

        public void ShowPrizeScreen()
        {
            if (_newPrizeScreens.Count > _newPrizeScreenIndex)
            {
                // Show Next Prize Screen
                string prizeDescription = PlayerModeGeospatial.SharedInstance.FleepSitePrizes
                    .Find(prize => prize.PrizeId == _newPrizeScreens[_newPrizeScreenIndex]).Description;
                _singlePlayerFleepSitePrizeText.text = prizeDescription;
                _singlePlayerFleepSitePrize.SetActive(true);
                // Increase index value
                _newPrizeScreenIndex += 1;
            }
            // Show Ranking screens if there are not new prizes left to show
            else if (_fleepSiteRankingCurrentGameResponse)
            {
                // Activating the FleepSite Ranking Screen
                _singlePlayerFleepSiteRanking.SetActive(true);
            }
            // Directly showing the next round screen
            else
            {
                ShowNextRoundScreen();
            }
        }

        public void ReceiveFleepSitePrizeLimitReachedFromAndroid(string prizeLimitResponseParams)
        {
            // Parsing received parameters
            var prizeLimitParams = prizeLimitResponseParams.Split('|');
            var prizeId = prizeLimitParams[0];
            var limitReached = prizeLimitParams[1];
            if (limitReached == "0" )
            {
                // Add Next Screen to list to show
                _newPrizeScreens.Add(prizeId);
                // Update granted prizes string
                _matchGrantedPrizes.Add(prizeId);

            } else
            {
                // The prize limit has been reached
                // Add the prize id to list of checked prize Ids
                // so we don't check it again during this session
                _prizeIdsChecked.Add(prizeId);
                // We also flag PrizesLeft as 0 so the prize won't show up any more
                // in the prizes list
                PlayerModeGeospatial.SharedInstance.FinishFleepSitePrize(prizeId);
            }
            
        }


        public void ReceiveFleepSiteRankingCurrentGameFromAndroid(string newRanking)
        {
            try
            {
                var newRankingInt = Int32.Parse(newRanking);
                // Setting new ranking value and sending signal to other scripts to update values
                _currentGameNewRanking.Fire(newRankingInt);
                _fleepSiteRankingCurrentGameResponse = true;
            }
            catch (FormatException)
            {
                Console.WriteLine($"Unable to parse current game FleepSite Ranking");
            }
        }

        public void ShowNextRoundScreen()
        {

            // In this case, the Round depends on the score treshold being
            // reached during the match
            if (_roundScore >= _nextRoundScoreThreshold)
            {
                _singlePlayerNextRound.SetActive(true);
            }
            else
            {
                _singlePlayerFinishedGame.SetActive(true);
            }
        }

        public void ToggleSettingsMenu()
        {
            if (!_singlePlayerSettingsMenu.activeSelf & !_singlePlayerLeavingGame.activeSelf)
            {
                _singlePlayerSettingsMenu.SetActive(true);
            }
            else
            {
                _singlePlayerSettingsMenu.SetActive(false);
                _singlePlayerLeavingGame.SetActive(false);
            }

        }

        public void sendMatchData(bool isFinished, bool roundStarted, bool roundFinished)
        {
            //// Sending message to main activity
            //MainActivityMessagingManager.SendMatchDataToMainActivity(
            //    _matchCount, // MatchId
            //    PlayerMode.SharedInstance.PlayerUId, // userUid
            //    PlayerMode.SharedInstance.PlayerName, // userName
            //    PlayerMode.SharedInstance.UserSessionId, // userSessionId, Fleepas DB Session Entry of each user, not the AR Session Id
            //    Convert.ToInt32((int)Math.Round(Score.SharedInstance.ScorePoints)), // Score
            //    true, // Is the player currently playing the game? (always true in Single Player)
            //    isFinished, // Is the Match Finished?);
            //    1, //Ranking
            //    Convert.ToInt32(Score.SharedInstance.ScorePoints), // FleepPoints: equal to score for single player
            //    _matchRound, // MatchRound,
            //    roundStarted, // If the round has been started or the user has left before
            //    roundFinished, // If the round has been finished or the match was left before
            //    PlayerMode.SharedInstance.UserFleepSiteTopScore <= (int)Math.Round(Score.SharedInstance.ScorePoints) // If the match is the top score or not
            //    );
        }


        private void StartFleepSiteLoadProcess()
        {
        }




    }
}

