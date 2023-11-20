using UnityEngine;
using UniRx;
using TMPro;
using System;
using UnityEngine.Localization;


namespace Bercetech.Games.Fleepas
{
    public class SocialShareUtil : MonoBehaviour
    {
		[SerializeField]
		private GameObject _mainSharingScreen;
		[SerializeField]
		private GameObject _waitWhileVideoGenerationMessage;
		[SerializeField]
		private GameObject _videoGenerationFailedMessage;
		[SerializeField]
		private GameObject _videoSharedScreen;
		[SerializeField]
		private GameObject _videoNotSharedScreen;
		private GameObject _previousScreen;
		[SerializeField]
		private GameObject _noAppInstalledAlert;
		[SerializeField]
		private TextMeshProUGUI _bonusMessageMainSharingText;
		[SerializeField]
		private TextMeshProUGUI _bonusMessageVideoSharedText;

		// Variable Strings
		private LocalizedString _mainSharingString = new LocalizedString("000 - Fleepas", "main_sharing_message");
		private LocalizedString _videoSharedString = new LocalizedString("000 - Fleepas", "video_shared_message");
		private LocalizedString _sharedTitleString = new LocalizedString("000 - Fleepas", "shared_video_title");
		private LocalizedString _sharedTextString = new LocalizedString("000 - Fleepas", "shared_video_text");

		// Shared Video Event
		private static Signal _videoShared = new Signal();
		public static Signal VideoShared => _videoShared;

		private string _sharedText;
		private string _sharedTitle;
		private string _sharedURL;

		// CompositeDisposable is similar with List<IDisposable>
		// It will be used to gather all disposables active after the game is finished
		// so they can be disposed at that moment
		protected CompositeDisposable disposables = new CompositeDisposable();
		private void OnDestroy()
		{
			disposables.Clear();
		}

		private void Start()
        {
			// Set messages
			_sharedText = _sharedTextString.GetLocalizedString();
			_sharedTitle = _sharedTitleString.GetLocalizedString();
			_sharedURL = "https://fleepas.com#get-fleepas";
			_bonusMessageMainSharingText.text = _mainSharingString.GetLocalizedString((int)Math.Round(100 * (ScoreBonus.SharedInstace.ScoreBonusPercentage - 1)));
			_bonusMessageVideoSharedText.text = _videoSharedString.GetLocalizedString((int)Math.Round(100 * (ScoreBonus.SharedInstace.ScoreBonusPercentage - 1)));
		}


        public void CheckTarget(string target)
        {
			// Check if target is installed (Android only)
			if (NativeShare.TargetExists(target))
            {
				LaunchVideoGeneration(target);
			} else
            {
				showNoAppInstalledAlert();
			}
		}


		private void LaunchVideoGeneration(string target)
		{
			// Hiding sharing screen and showing waiting message
			_mainSharingScreen.SetActive(false);
			_waitWhileVideoGenerationMessage.SetActive(true);
			// Subscribing to encoding finished event
			ScreenRecorder.SharedInstance.EncodingFinished.TakeUntilDisable(_waitWhileVideoGenerationMessage).Subscribe(success =>
			{
				// Hide waiting message
				_waitWhileVideoGenerationMessage.SetActive(false);
				if (success)
				{
					ShareVideo(target);
				}
				else
				{
                    // Show video generation failed message
                    //_videoGenerationFailedMessage.SetActive(true);
					_videoSharedScreen.SetActive(true);
					VideoShared.Fire();
				}
			}).AddTo(disposables);

			// Generate video from the screen captures that are currently in ScreenRecorder script variables 
			ScreenRecorder.SharedInstance.GenerateVideo();

		}



		private void ShareVideo(string target)
        {
			// Share on target
			new NativeShare()
				.AddFile(ScreenRecorder.SharedInstance.VideoOutputPath)
				.SetText(_sharedText)
				.SetUrl(_sharedURL)
				.SetTitle(_sharedTitle)
				.AddTarget(target)
				.SetCallback((result, shareTarget) =>
				{
					Logging.Omigari("Share result: " + result + ", selected app: " + shareTarget);
					if (result != NativeShare.ShareResult.NotShared)
					{
						// Showing succesful sharing message and emiting event
						_videoSharedScreen.SetActive(true);
						VideoShared.Fire();
					}
					else
						// Showing alert message
						_videoNotSharedScreen.SetActive(true);
				})
				.Share();

		}



		public void setPreviousScreen(GameObject screen)
        {
			// Set the previous screen to come back in case of hitting back button
			// because this screen might be accessed from different parts
			_previousScreen = screen;
		}

		private void showNoAppInstalledAlert()
		{
			_mainSharingScreen.SetActive(false);
			_noAppInstalledAlert.SetActive(true);
		}

		public void openPreviousScreen()
		{
			if (_previousScreen != null)
				_previousScreen.SetActive(true);
		}


		// THIS IS FOR QUICK TESTS //
		//void Update()
		//{
		//    if (Input.GetMouseButtonDown(0))
		//    {
		//        StartCoroutine(TakeScreenshotAndShare());
		//    }

		//}

		//private IEnumerator TakeScreenshotAndShare()
		//{
		//    yield return new WaitForEndOfFrame();

		//    Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		//    ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		//    ss.Apply();

		//    string filePath = Path.Combine(Application.temporaryCachePath, "sharedimg.png");
		//    File.WriteAllBytes(filePath, ss.EncodeToPNG());

		//    // To avoid memory leaks
		//    Destroy(ss);


		//    // Share on WhatsApp only, if installed (Android only)
		//    if (NativeShare.TargetExists("com.whatsapp"))
		//    {
		//        new NativeShare().AddFile(filePath)
		//            .AddTarget("com.whatsapp")
		//            .SetText(_sharedText)
		//            .SetUrl(_sharedURL)
		//            .SetTitle(_sharedTitle)
		//            .SetCallback((result, shareTarget) =>
		//            {
		//                Logging.OmigariHP("Share result: " + result + ", selected app: " + shareTarget);
		//                if (result != NativeShare.ShareResult.NotShared)
		//                    // Showing succesful sharing message
		//                    _videoSharedScreen.SetActive(true);
		//                else
		//                    // Showing alert message
		//                    _videoNotSharedScreen.SetActive(true);
		//            }).Share();
		//    }
		//}


	}
}
