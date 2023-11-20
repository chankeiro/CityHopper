using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using UnityEngine.Localization;


namespace Bercetech.Games.Fleepas
{
    public class ScanningProgress : MonoBehaviour
    {
        [SerializeField]
        private GameObject _arMesh;
        [SerializeField]
        private GameObject _progressBar;
        [SerializeField]
        private GameObject _resetButton;
        [SerializeField]
        private GameObject _hostScanFinishedMessage;
        [SerializeField]
        private GameObject _textProgress;
        [SerializeField]
        private GameObject _scanMessage;
        [SerializeField]
        private GameObject _longScanWarning;
        [SerializeField]
        private int _longScanWarningTime;
        [SerializeField]
        private GameObject _arMeshManager;
        private float _percentage;
        private float _timeStep;

        private LocalizedString _percScannedString = new LocalizedString("000 - Fleepas", "perc_scanned");
        string PercScannedString => _percScannedString.GetLocalizedString(Math.Round(100 * _percentage, 0));


        // Defining a static shared instance variable so other scripts can access to the object
        private static ScanningProgress _sharedInstance;
        public static ScanningProgress SharedInstance => _sharedInstance;
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

        private void Start()
        {
            // Set percentage to 0 on start and any time the meshing process is restarted
            _percentage = 0f;
            _timeStep = 0.05f;
            ARFManager.SharedInstance.MeshingRestarted.Subscribe(_ =>
            {
                _percentage = 0f;
            }).AddTo(gameObject);
        }

        // Start is called before the first frame update
        void OnEnable()
        {

            Observable.Interval(TimeSpan.FromSeconds(_timeStep)).TakeUntilDisable(gameObject).Subscribe(_ =>
            {
                // As some chunks might dissapear, sometimes the scanned area goes down (slightly)
                // Here the percentage is kept with the maximum historical value so the user is not confused by this effect
                if (_percentage < Math.Min(ARMeshData.SharedInstance.GetAreaFromMeshManager() / ARMeshData.SharedInstance.MinAreaSize, 1f))
                    _percentage = Math.Min(ARMeshData.SharedInstance.GetAreaFromMeshManager() / ARMeshData.SharedInstance.MinAreaSize, 1f);


                // Setting bar and text values
                _progressBar.GetComponent<Scrollbar>().size = _percentage;
                _textProgress.GetComponent<TextMeshProUGUI>().text = PercScannedString;

                // Hide scan message when the percentage is higher than 15%
                if (_percentage >= 0.15f)
                {
                    if (_scanMessage.activeSelf)
                        _scanMessage.GetComponent<FadingImage>().Fade();
                }

                // When the minimum area is scanned, the message to start the game pops up and the Scrollbar is disabled
                if (_percentage >= 1)
                {
                    gameObject.SetActive(false);
                    _hostScanFinishedMessage.SetActive(true);
                    return;
                }

            }).AddTo(gameObject); ;

            // Start time to warn in case the scan takes too long
            LaunchLongScanTimer();
        }


        public void LaunchLongScanTimer()
        {
            Observable.Timer(TimeSpan.FromSeconds(_longScanWarningTime)).TakeUntilDisable(gameObject).Subscribe(_ =>
                {
                    if (_percentage < 1)
                        _longScanWarning.SetActive(true);
                }
            ).AddTo(gameObject);
        }






    }
}
