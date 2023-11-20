using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UniRx;


namespace Bercetech.Games.Fleepas
{
    public class IronSourceAd : MonoBehaviour
    {

        // Defining a static shared instance variable so other scripts can access to it
        private static IronSourceAd _sharedInstance;
        public static IronSourceAd SharedInstance => _sharedInstance;

        // Showed/Failed Ad Events
        private Signal _ironSourceAdFailed = new Signal();
        public Signal IronSourceAdFailed => _ironSourceAdFailed;
        private Signal _ironSourceAdShowed = new Signal();
        public Signal IronSourceAdShowed => _ironSourceAdShowed;

        private void Awake()
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

        // Need to pass the state of the application by executing the following event function during the Application Lifecycle.
        private void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
        }



       private void OnEnable()
        {
            //Add Init Event
            IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;

            //Add Rewarded Video Events
            IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;

            // Add Interstitial Video Events
            IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
            IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
            IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
            IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
            IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
            IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;

        }


        private void Start()
        {


#if UNITY_ANDROID && !UNITY_EDITOR
            string appKey = Menu.SharedInstance.IronSourceAndroidKey;
#elif UNITY_IPHONE && !UNITY_EDITOR
        string appKey = "8545d445";
#else
        string appKey = "unexpected_platform";
#endif
            // This will confirm which networks are verified
            IronSource.Agent.validateIntegration();

            // SDK init
            IronSource.Agent.init(appKey);

            // Start asking for interstitial ads, which need to be preloaded
            LoadInterstitial();
        }


        void SdkInitializationCompletedEvent()
        {
            // DO NOTHING
        }


        public void ShowRewardedAd(string placement)
        {

            if (IronSource.Agent.isRewardedVideoAvailable())
                // Show ad 
                IronSource.Agent.showRewardedVideo(placement);
            else
                // Sending Ad Failed if ad rewarded is not ready
                _ironSourceAdFailed.Fire();
        }

        /************* RewardedVideo AdInfo Delegates *************/
        // Indicates that there’s an available ad.
        // The adInfo object includes information about the ad that was loaded successfully
        void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
        {
        }
        // Indicates that no ads are available to be displayed
        void RewardedVideoOnAdUnavailable()
        {
            //Debug.Log("Ironsource Ad Rewarded unavailable");
        }
        // The Rewarded Video ad view has opened. Your activity will loose focus.
        void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
        }
        // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
        void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
        }
        // The user completed to watch the video, and should be rewarded.
        // The placement parameter will include the reward data.
        // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
        void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            _ironSourceAdShowed.Fire();
        }
        // The rewarded video ad was failed to show.
        void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
        {
            //Debug.Log("Ironsource Ad rewarded show error " + error);
            _ironSourceAdFailed.Fire();
        }

        public void ShowInterstitialAd(string placement)
        {
            if (IronSource.Agent.isInterstitialReady())
                // Show ad 
                IronSource.Agent.showInterstitial(placement);
            else
                // Sending Ad Failed if ad rewarded is not ready
                _ironSourceAdFailed.Fire();
        }

        // I don't know why the interstitial must be loaded but the rewarded no
        public void LoadInterstitial()
        {
            if (!IronSource.Agent.isInterstitialReady())
            {
                IronSource.Agent.loadInterstitial();
            }
        }

        /************* Interstitial AdInfo Delegates *************/
        // Invoked when the interstitial ad was loaded succesfully.
        void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo)
        {
        }
        // Invoked when the initialization process has failed.
        void InterstitialOnAdLoadFailed(IronSourceError ironSourceError)
        {
            //Debug.Log("IronsourceAd interstitial load error " + ironSourceError);
            // Ask to load again
            LoadInterstitial();
        }
        // Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
        void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            _ironSourceAdShowed.Fire();
        }
        // Invoked when end user clicked on the interstitial ad
        void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo)
        {
        }
        // Invoked when the ad failed to show.
        void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
        {
            _ironSourceAdFailed.Fire();
            //Debug.Log("Ironsource Ad interstitial show error " + ironSourceError);
        }
        // Invoked when the interstitial ad closed and the user went back to the application screen.
        void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
            // Ask to load again
            LoadInterstitial();
        }

        private void OnDisable()
        {
            //Add Init Event
            IronSourceEvents.onSdkInitializationCompletedEvent -= SdkInitializationCompletedEvent;

            //Add Rewarded Video Events
            IronSourceRewardedVideoEvents.onAdAvailableEvent -= RewardedVideoOnAdAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent -= RewardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdOpenedEvent -= RewardedVideoOnAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent -= RewardedVideoOnAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent -= RewardedVideoOnAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;

            // Add Interstitial Video Events
            IronSourceInterstitialEvents.onAdReadyEvent -= InterstitialOnAdReadyEvent;
            IronSourceInterstitialEvents.onAdLoadFailedEvent -= InterstitialOnAdLoadFailed;
            IronSourceInterstitialEvents.onAdOpenedEvent -= InterstitialOnAdOpenedEvent;
            IronSourceInterstitialEvents.onAdClickedEvent -= InterstitialOnAdClickedEvent;
            IronSourceInterstitialEvents.onAdShowFailedEvent -= InterstitialOnAdShowFailedEvent;
            IronSourceInterstitialEvents.onAdClosedEvent -= InterstitialOnAdClosedEvent;

        }

    }
}

       