using System.Collections;
using UnityEngine;
using Google.Play.Review;
using UnityEngine.Localization;


namespace Bercetech.Games.Fleepas
{
    public class GooglePlayReviewManager : MonoBehaviour
    {
        [SerializeField]
        private int _roundToRequestReview;
        [SerializeField]
        private GameObject _nextScreen;
        [SerializeField]
        private GameObject _errorScreen;
        public int RoundToRequestReview => _roundToRequestReview;
        private ReviewManager _reviewManager;
        private PlayReviewInfo _playReviewInfo;
        private bool _reviewedInThisSession;

        private LocalizedString _feedbackMailTitleString = new LocalizedString("000 - Fleepas", "feedback_mail_title");
        private LocalizedString _feedbackMailContentString = new LocalizedString("000 - Fleepas", "feedback_mail_content");


        // Defining a static shared instance variable so other scripts can access to the object
        private static GooglePlayReviewManager _sharedInstance;
        public static GooglePlayReviewManager SharedInstance => _sharedInstance;

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

            _reviewedInThisSession = false;
        }



        // Request Google Play Review Information
        // Google recommends to do it a few seconds before of actually need it
        public void RequestReviewInfo()
        {
            // Reset the object. It must be not null for the dialogs to appear
            // That only happens if all the conditions to request it are met
            _playReviewInfo = null;
            // Condition to meet // TODO: remove the if when SinglePlayerManager
            // is unified with SinglePlayerManagerGeospatial
            if (SinglePlayerManager.SharedInstance != null)
            {
                if (_roundToRequestReview == SinglePlayerManager.SharedInstance.MatchRound
                    && PlayerMode.SharedInstance.CanReviewTheApp
                    && !_reviewedInThisSession)
                {
                    // Create instance of ReviewManager
                    _reviewManager = new ReviewManager();
                    StartCoroutine(RRICoroutine());
                }
            }
        }


        private IEnumerator RRICoroutine()
        {
            if (_reviewManager != null)
            {
                var requestFlowOperation = _reviewManager.RequestReviewFlow();
                yield return requestFlowOperation;
                if (requestFlowOperation.Error != ReviewErrorCode.NoError)
                {
                    Logging.OmigariHP("Error while requesting Google Play Review Informartion" + requestFlowOperation.Error);
                    yield break;
                }
                _playReviewInfo = requestFlowOperation.GetResult();
            } else
            {
                Logging.OmigariHP("Google Review Manager couldn't be created");
            }

        }

        public bool CheckGoogleReviewAvailable()
        {
            return _playReviewInfo != null;
        }

        // Launch Review Dialog
        // NOTE: CURRENTLY CALLING THIS FUNCTION DIRECTLY FROM THE FIRST DIALOG
        public void LaunchInAppReview()
        {
            StartCoroutine(LIARCoroutine());
        }
        private IEnumerator LIARCoroutine() {
            if (_playReviewInfo != null) // This condition was alreadt checked in CheckGoogleReviewAvailable(), but just in case
            {
                var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
                yield return launchFlowOperation;
                if (launchFlowOperation.Error != ReviewErrorCode.NoError)
                {
                    Logging.OmigariHP("Error while launching Google Play Review Panel" + launchFlowOperation.Error);
                    yield break;
                }
                // Flagging review in DB
                SendGooglePlayReviewData(true);
                // Showing the next screen
                _nextScreen.SetActive(true);
            }
            else
            {
                Logging.OmigariHP("Google Play Review Information Not Ready");
                // Showing error screen
                _errorScreen.SetActive(true);
            }

        }

        public void SendGooglePlayReviewData(bool userGaveReview)
        {
            // User has decided to give a review or not
            MainActivityMessagingManager.SendGooglePlayReviewData(true, userGaveReview);
            // And this session so Unity won't show the message again
            _reviewedInThisSession = true;
        }

        public void NotSendGooglePlayReviewData()
        {
            // User didn't gave a definitive answered yet
            MainActivityMessagingManager.SendGooglePlayReviewData(false, false);
            // And this session so Unity won't show the message again
            _reviewedInThisSession = true;
        }

        public void SendFeedback()
        {
            HelperFunctions.SendMail(
                "info@fleepas.com",
                _feedbackMailTitleString.GetLocalizedString(),
                _feedbackMailContentString.GetLocalizedString()
                );
        }

    }
}
