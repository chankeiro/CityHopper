using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System;

namespace Bercetech.Games.Fleepas
{
    // Class to manage the activation of Single and Multiplayer modes
    // and to manage shared Menu Objects
    public class PlayerMode : MonoBehaviour
    {
        private SinglePlayerManager _singlePlayerManager;

        private bool _isDebug;
        public bool IsDebug => _isDebug;
        protected int _gameType;
        public int GameType => _gameType;
        private string _arSessionId;
        public string ARSessionId => _arSessionId;
        private string _userSessionId;
        public string UserSessionId => _userSessionId;
        private string _playerUid;
        public string PlayerUId => _playerUid;
        private string _playerName;
        public string PlayerName => _playerName;
        private bool _canCreateFleepSite;
        public bool CanCreateFleepSite => _canCreateFleepSite;

        private List<FleepSitePrize> _fleepSitePrizes;
        public List<FleepSitePrize> FleepSitePrizes => _fleepSitePrizes;
        private int _userFleepSiteTopScore;
        public int UserFleepSiteTopScore => _userFleepSiteTopScore;
        private int _userFleepSiteRanking;
        public int UserFleepSiteRanking => _userFleepSiteRanking;
        private string _userFleepSitePrizesGranted;
        public string UserFleepSitePrizesGranted => _userFleepSitePrizesGranted;
        private bool _canReviewTheApp;
        public bool CanReviewTheApp => _canReviewTheApp;
        private string _fleepSiteId;
        public string FleepSiteId => _fleepSiteId;

        [SerializeField]
        private GameObject _ScoreClock;

        private bool _backClicEnabled = true;

        // Defining a static shared instance variable so other scripts can access to the object
        private static PlayerMode _sharedInstance;
        public static PlayerMode SharedInstance => _sharedInstance;
        

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


            // Enable AR Logging
            //#if DEVELOPMENT_BUILD || UNITY_EDITOR
            //            ARLog.EnableLogFeature("Niantic");
            //#endif

            // This is just to have variables definition in case we don't go through the Menu Scene when testing In Editor.
            // Normally it wouldn't be necessary, and we could call in all the scripts Menu.SharedInstance.[Variable] instead
            // because those variables have been already defined when loading the menu scene
            if (Menu.SharedInstance != null)
            {
                _isDebug = Menu.SharedInstance.IsDebug;
                _gameType = Menu.SharedInstance.GameType;
                _arSessionId = Menu.SharedInstance.ARSessionId;
                _userSessionId = Menu.SharedInstance.UserSessionId;
                _playerUid = Menu.SharedInstance.PlayerUId;
                _playerName = Menu.SharedInstance.PlayerName;
                _canCreateFleepSite = Menu.SharedInstance.CanCreateFleepSite;
                _fleepSiteId = Menu.SharedInstance.FleepSiteId;
                _fleepSitePrizes = Menu.SharedInstance.FleepSitePrizes;
                _userFleepSiteTopScore = Menu.SharedInstance.UserFleepSiteTopScore;
                _userFleepSiteRanking = Menu.SharedInstance.UserFleepSiteRanking;
                _userFleepSitePrizesGranted = Menu.SharedInstance.UserFleepSitePrizesGranted;
                _canReviewTheApp = Menu.SharedInstance.CanReviewTheApp;

            }
            else
            {
                // Editor Testing variables
                _isDebug = true;
                _gameType = 0;
                _arSessionId = "TEST1234567890";
                _userSessionId = "TEST0987654321";
                _playerUid = "P2";
                _playerName = "PTWO";
                _canCreateFleepSite = true;
                _fleepSiteId = "";
                _fleepSitePrizes = new List<FleepSitePrize> {
                    new FleepSitePrize("PRIZEID", "Discount 1", 20, 0, 0, 100, 50),
                    new FleepSitePrize("p1", "Discount 2Discount 2Discount 2Discount 2Discount 2Discount 2Discount 2", 20, 0, 0, 0, 0),
                    new FleepSitePrize("p2", "Discount 3Discount 3Discount 3Discount 3Discount 3Discount 3Discount 3", 10, 2, 4, 0, 0),
                    new FleepSitePrize("p3", "Discount 4Discount 4Discount 4Discount 4Discount 4Discount 4Discount 4", 0, 1, 0, 0, 0),
                    new FleepSitePrize("p4", "Discount 5Discount 5Discount 5Discount 5Discount 5Discount 5Discount 5", 50, 0, 0, 0, 0),
                    new FleepSitePrize("p5", "Discount 6Discount 6Discount 6Discount 6Discount 6Discount 6Discount 6", 0, 4, 0, 0, 0),
                    new FleepSitePrize("p6", "Discount 7Discount 7Discount 7Discount 7Discount 7Discount 7Discount 7", 0, 4, 0, 0, 0),
                    new FleepSitePrize("p7", "Discount 8Discount 8Discount 8Discount 8Discount 8Discount 8Discount 8", 0, 4, 0, 0, 0),
                    //new FleepSitePrize("p8", "Discount 9", 0, 4, 0, 0, 0),
                    //new FleepSitePrize("p9", "Discount 10", 0, 4, 0, 0, 0),
                    //new FleepSitePrize("p10", "Discount 11", 0, 4, 0, 0, 0),
                    //new FleepSitePrize("p11", "Discount 12", 0, 4, 0, 0, 0),
                    //new FleepSitePrize("p12", "Discount 13", 0, 4, 0, 0, 0),
                    //new FleepSitePrize("p13", "Discount 14", 0, 4, 0, 0, 0),
                    //new FleepSitePrize("p14", "Discount 15", 0, 4, 0, 0, 0)
                };
                _userFleepSiteTopScore = 5;
                _userFleepSiteRanking = 2;
                _userFleepSitePrizesGranted = "ADB,PRIZEID";
                _canReviewTheApp = true;
            }

        }

        private void Update()
        {
            // Show settings menu when on back click
            CaptureBackButtonClick();
        }


        protected virtual void OnEnable()
        {
            _singlePlayerManager = GetComponent<SinglePlayerManager>();
            switch (_gameType)
            {
                case 0: // Single Player: New game
                    // Enable Single Player manager
                    _singlePlayerManager.enabled = true;
                    break;
            }

        }

        private void Start()
        {
            // Subscribe to events to update FleepSite values from outside
            SinglePlayerManager.SharedInstance.CurrentGameNewRanking.Subscribe(newRanking =>
            {
                if (newRanking < _userFleepSiteRanking) _userFleepSiteRanking = newRanking; // Only update if the ranking is improved
            }).AddTo(gameObject);

            var endObservable = true;
            // Need to wait until Score SharedInstance is created to subscribe
            Observable.Interval(TimeSpan.FromSeconds(1f)).TakeWhile(_ => endObservable).Subscribe(_ =>
            {
                if (Score.SharedInstance != null)
                {
                    Score.SharedInstance.FinalScoreCount.Subscribe(_ =>
                    {
                        // Updating historical top score at the end of the round 
                        if ((int)Mathf.Round(Score.SharedInstance.ScorePoints) > _userFleepSiteTopScore)
                            _userFleepSiteTopScore = (int)Mathf.Round(Score.SharedInstance.ScorePoints);
                        // Assigning prizes granted during the match to the user FleepSite prizes 
                        SinglePlayerManager.SharedInstance.MatchGrantedPrizes.ForEach(_grantedPrizeId =>
                        {
                            if (!_userFleepSitePrizesGranted.Contains(_grantedPrizeId))
                                _userFleepSitePrizesGranted += "," + _grantedPrizeId;
                        });
                    }).AddTo(gameObject);
                    endObservable = false;
                }
            }).AddTo(gameObject);
        }


        // To manage shared menu object (HostScanFinishedMessage)
        public void ActionAfterHostScanFinished()
        {
            switch (_gameType)
            {
                case 0: // Single Player: New game
                    _singlePlayerManager.ActionAfterHostScanFinished();
                    break;
            }
        }

        public void ToggleSettingsMenu()
        {
            switch (_gameType)
            {
                case 0: // Single Player: New game
                    _singlePlayerManager.ToggleSettingsMenu();
                    break;
            }
        }


        public void ReturnToNativeApp()
        {
            switch (_gameType)
            {
                case 0: // Single Player: New game
                    _singlePlayerManager.ReturnToNativeApp();
                    break;
            }
        }

        public void PoolObjectsWarming()
        {
            // Preload gameobjects from pool once, to prevent the game to stall when first loading 
            // in the middle of the game due to resources needed loaded for the first time
            // Going through all the Object Pool Tag
            var poolItems = ObjectPool.ItemsToPool;
            foreach (var pi in poolItems)
            {
                var tag = pi.ObjectToPool.tag;
                GameObject preloadedObject = ObjectPool.SharedInstace.GetPooledObject(tag);
                if (preloadedObject != null)
                {
                    StartCoroutine(PreloadObject(preloadedObject, 0.5f));
                    //if (tag == "ExplosionText")
                    //{
                    //    // The first access to this component stalls the game in phones
                    //    TextMeshPro tm = preloadedObject.GetComponent<TextMeshPro>();
                    //    tm.text = "Warming Text";
                    //    //tm.fontStyle = FontStyles.Bold;
                    //    //tm.fontStyle = FontStyles.Normal;
                    //}
                }
            }
        }

        private IEnumerator PreloadObject(GameObject preloadedObject, float seconds)
        {
            // Moving it far away first, to avoid be seen in the game screen for any reason
            preloadedObject.transform.position = 999 * Vector3.one;
            preloadedObject.SetActive(true);
            yield return new WaitForSeconds(seconds);
            preloadedObject.SetActive(false);
        }

        public virtual void GameWarming()
        {
            // Clock consumes time when loading on the first game launch, because they
            // have several methods in their awake, enable blocks
            _ScoreClock.SetActive(true);
            // Wait a litte bit to disable. Otherwise some blocks are not completely preloaded
            Observable.Timer(TimeSpan.FromSeconds(2f)).Subscribe(_ => {
                _ScoreClock.SetActive(false);
            });


            // Warm up shaders
            Shader.WarmupAllShaders();
        }

        private void CaptureBackButtonClick()
        {
            if (Input.GetKey(KeyCode.Escape) && _backClicEnabled)
            {
                //StartCoroutine(ToggleSettingsOnBackButtonClick());
                ToggleSettingsMenu();
                _backClicEnabled = false;
                StartCoroutine(
                    HelperFunctions.SpaceButtonClick(() =>_backClicEnabled = true)
                );
            }
        }

        public void FinishFleepSitePrize(string prizeId)
        {
            _fleepSitePrizes.Find(prize => prize.PrizeId == prizeId).PrizesLeft = 0;
        }

    }
}
