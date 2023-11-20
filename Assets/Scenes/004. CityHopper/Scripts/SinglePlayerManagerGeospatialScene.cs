using UnityEngine;
using UniRx;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;

namespace Bercetech.Games.Fleepas.CityBunny
{
    /// Controls the game logic and creation of objects
    public class SinglePlayerManagerGeospatialScene : SinglePlayerManagerGeospatial
    {
        [SerializeField]
        private Bunny _bunny;
        [SerializeField]
        private BunnyDoor _bunnyDoor;
        [SerializeField]
        private GameObject _jumpButton;
        [SerializeField]
        private GameObject _uiARView;
        [SerializeField]
        private GameObject _geospatialController;
        [SerializeField]
        private GameObject _geospatialCreatorOrigin;
        [SerializeField]
        private ARStreetscapeGeometryManager _streetScapeGeometryManager;
        [SerializeField]
        private ARPlaneManager _arPlaneManager;
        [SerializeField]
        private ARCoreExtensions _arCoreExtensions;
        [SerializeField]
        private GameObject _fleepSiteRankingTimer;

        override protected void Awake()
        {
            base.Awake();
            // Disabling Geospatial in Editory  mode
#if UNITY_EDITOR
            _uiARView.SetActive(false);
            _geospatialController.SetActive(false);
            _streetScapeGeometryManager.enabled = false;
            _arPlaneManager.enabled = false;
            _arCoreExtensions.enabled = false;
#endif
            // Geospatial creator can be disabled in app too.
            //_geospatialCreatorOrigin.SetActive(false);
            // Setting Shadow Distance (quite far in this case)
            QualitySettings.shadowDistance = 100f;
        }

        override protected void Start()
        {
            base.Start();
            // Forcing Screen in portrait mode
            Screen.orientation = ScreenOrientation.Portrait;
            // Disabling bunny in case it was enabled
            _bunny.enabled = false;
            // Subscribing to game started (before countdown)
            StartGame.SharedInstance.StartGameEvent.Subscribe(_ =>
            {

                // Enable Screen in landscape mode
                Screen.orientation = ScreenOrientation.AutoRotation;
            }).AddTo(disposables);
            // Subscribing to game started
            StartGame.SharedInstance.ActivateGameEvent.Subscribe(_ =>
            {
                // Open Door
                _bunnyDoor.OpenDoor();
                // Enabling bunny script and jump button
                _bunny.enabled = true;
                _jumpButton.SetActive(true);
            }).AddTo(disposables);


            // Subscribing to Final Score Count event
            Score.SharedInstance.FinalScoreCount.Subscribe(_ =>
            {
                // Disabing bunny
                _bunny.enabled = false;
                _jumpButton.SetActive(false);
                // Forcing Screen in portrait mode
                Screen.orientation = ScreenOrientation.Portrait;

            }).AddTo(disposables);

        }

        protected override void RoundReset()
        {
            base.RoundReset();
            // Clearing objects
            //_bottleGenerator.ClearingGame(false);
            //_alienGenerator.ClearingGame();
        }

        protected override void EndSinglePlayerRound()
        {
            // This is just a quick logic for the demo
            _fleepSiteRankingTimer.SetActive(true);
        }


    }
}

