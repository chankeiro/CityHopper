using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Newtonsoft.Json;

namespace Bercetech.Games.Fleepas
{
    public class Menu : MonoBehaviour
    {

        private bool _isDebug;
        public bool IsDebug => _isDebug;
        private int _gameType;
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
        private string _fleepSiteId;
        public string FleepSiteId => _fleepSiteId;
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
        private string _ironSourceAndroidKey;
        public string IronSourceAndroidKey => _ironSourceAndroidKey;


        // Defining a static shared instance variable so other scripts can access to the object pool
        private static Menu _sharedInstance;
        public static Menu SharedInstance => _sharedInstance;

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
                DontDestroyOnLoad(this.gameObject); // Need this to keep all fleep params on memory when loading a new scene
            }

        }

        void Start()
        {
            // Preventing the screen to turn off
            // It keeps the value even when loading new scenes
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if UNITY_EDITOR
            // Testing purposes
            StartCoroutine(TestLoad()); // Need this to load on the first run
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
#endif
        }

#if UNITY_EDITOR
        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
#endif
        void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            // Call an specific scene directly (for testing purposes)
            if (scene.name == "Menu")
                StartCoroutine(TestLoad());
        }

        private IEnumerator TestLoad()
        {
            yield return new WaitForSeconds(2);
            LoadFleepFromAndroid("true|1|TEST1234567890|TEST0987654321|P2|PDOS|1|true|");
        }


        public void LoadFleepFromAndroid(string gameParametersString)
        {
            var gameParameters = gameParametersString.Split('|');
            // Setting variables from the string received
            _isDebug = gameParameters[0] == "true";

            try
            {
                _gameType = Int32.Parse(gameParameters[1]);
            }
            catch (FormatException)
            {
                Console.WriteLine($"Unable to parse gametype code: '{gameParameters[1]}'");
            }

            _arSessionId = gameParameters[2];
            _userSessionId = gameParameters[3]; // Same as arSessionId except for Multiplayer-Join
            _playerUid = gameParameters[4];
            _playerName = gameParameters[5];
            int fleep;
            try
            {
                fleep = Int32.Parse(gameParameters[6]);
            }
            catch (FormatException)
            {
                Console.WriteLine($"Unable to parse fleep code: '{gameParameters[6]}'");
                fleep = 0;
            }
            _canCreateFleepSite = gameParameters[7] == "true";
            _fleepSiteId = gameParameters[8];

            // FleepSite Prizes param
            _fleepSitePrizes = new List<FleepSitePrize>();
            for(int i = 9; i <= 23 ; i++) 
            {
                // Add to the list only configured prizes
                if (gameParameters[i] != "null")
                {
                    try
                    {
                        _fleepSitePrizes.Add(JsonConvert.DeserializeObject<FleepSitePrize>(gameParameters[i]));
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Unable to parse user FleepSite Prize: '{gameParameters[i]}'");
                        fleep = 0;
                    }
                }
            }
            try
            {
                _userFleepSiteTopScore = Int32.Parse(gameParameters[24]);
            }
            catch (FormatException)
            {
                Console.WriteLine($"Unable to parse user FleepSite Top Score: '{gameParameters[14]}'");
                fleep = 0;
            }
            try
            {
                _userFleepSiteRanking = Int32.Parse(gameParameters[25]);
            }
            catch (FormatException)
            {
                Console.WriteLine($"Unable to parse user FleepSite Ranking: '{gameParameters[15]}'");
                fleep = 0;
            }
            _userFleepSitePrizesGranted = gameParameters[26];
            _canReviewTheApp = gameParameters[27] == "true";
            _ironSourceAndroidKey = gameParameters[28];

            // Loading fleep
            if (fleep != 0)
                SceneManager.LoadScene(fleep);
            else
                Application.Unload();
        }

    }
}
