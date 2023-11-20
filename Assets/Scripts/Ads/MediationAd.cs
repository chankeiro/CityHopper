using UnityEngine;
using UniRx;
using UnityEngine.SceneManagement;

namespace Bercetech.Games.Fleepas
{
    public class MediationAd: MonoBehaviour
    {
        [SerializeField]
        private GameObject _adShowedScreen;
        [SerializeField]
        private GameObject _adFailedScreen;
        [SerializeField]
        private GameObject _previousScreen;
        [SerializeField]
        private AdResult _adResult;
        [SerializeField]
        private AdType _adType;
        [SerializeField]
        private bool _giveResultAlsoIfAdFailed = true;


        public enum AdResult : uint
        {
            ScoreBonus,
            RestartGame,
            TryAgain,
            AddFleepCoins
        }

        public enum AdType : uint
        {
            Rewarded,
            Interstitial
        }


        // Shared Video Event
        private static Signal<AdResult> _adShowed = new Signal<AdResult>();
        public static Signal<AdResult> AdShowed => _adShowed;
        // Variable to distinguish the ad button doing the request
        private bool _pendingAdResponse = false;

        private void Start()
        {
            // Susbcribing to IronSource vents
            IronSourceAd.SharedInstance.IronSourceAdShowed.Subscribe(_ => ShowAdShowedScreen()).AddTo(gameObject);
            IronSourceAd.SharedInstance.IronSourceAdFailed.Subscribe(_ => ShowAdFailedScreen()).AddTo(gameObject);
        }

        public void ShowAd()
        {

            // We need to know that this is the button that must react to the message from the native activity
            _pendingAdResponse = true;
            // Asking for Ad
            var adPlacement = _adResult.ToString() + SceneManager.GetActiveScene().buildIndex;
            if (_adType == AdType.Rewarded)
                IronSourceAd.SharedInstance.ShowRewardedAd(adPlacement);
            if (_adType == AdType.Interstitial)
                IronSourceAd.SharedInstance.ShowInterstitialAd(adPlacement);
        }

        public void ShowAdShowedScreen()
        {
            if (_pendingAdResponse) 
            {
                if (_previousScreen != null)
                    _previousScreen.SetActive(false);
                if (_adShowedScreen != null)
                    _adShowedScreen.SetActive(true);
                if (_adFailedScreen != null)
                    _adFailedScreen.SetActive(false);
                // Emiting Ad Showed Event
                _adShowed.Fire(_adResult);
                _pendingAdResponse = false;
            }
        }

        public void ShowAdFailedScreen()
        {
            if (_pendingAdResponse)
            {
                if (_previousScreen != null)
                    _previousScreen.SetActive(false);
                if (_adShowedScreen != null)
                    _adShowedScreen.SetActive(false);
                if (_adFailedScreen != null)
                    _adFailedScreen.SetActive(true);
                // Emiting Ad Showed Event
                if (_giveResultAlsoIfAdFailed)
                    _adShowed.Fire(_adResult);
                _pendingAdResponse = false;
            }
        }

    }
}
