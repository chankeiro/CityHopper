using UnityEngine;
using UnityEngine.Video;
using UniRx;
using TMPro;
using System.IO;

namespace Bercetech.Games.Fleepas
{
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(GetDownloadUrl))]
    public class LoadVideoUrl : MonoBehaviour
    {

        private VideoPlayer _videoPlayer;
        private GetDownloadUrl _urlDownloader;
        private string _videoDirectory;
        private string _videoPath;
        private string[] _urlParams;
        [SerializeField]
        private GameObject _downloadWarning;
        [SerializeField]
        private GameObject _loadingVideoMessage;
        [SerializeField]
        private TextMeshProUGUI _downloadWarningText;

        void Start()
        {
            _videoPlayer = GetComponent<VideoPlayer>();
            _urlDownloader = GetComponent<GetDownloadUrl>();

            DownloadsReceiver.SharedInstance.DownloadUrlReceived.Subscribe(urlData =>
           {
               // Getting params
               // 0 - urlPath configured in urlDownloader
               // 1 - download url
               _urlParams = urlData.Split('|');
               // Check if the video already exists to not download it again
               _videoDirectory = Application.temporaryCachePath + "/Video";
               _videoPath = Path.Combine(_videoDirectory, Path.GetFileName(_urlParams[0]));
               if (File.Exists(_videoPath))
               {
                   // Play video and stop here
                   _videoPlayer.url = _videoPath;
                   return;
               }
               // Otherwise, proceed to downlaod it
               // Only load url if the gameobjecting managing the VideoPlayer is active
               // and the path corresponds with the same path of the downloader
               if (gameObject.activeSelf && _urlParams[0] == _urlDownloader.urlPath)
               {
                   switch (_urlParams[1])
                   {
                       // Show error messages in case the param is an error string
                       case "SLOW_NETWORK":
                       case "NO_NETWORK":
                           if (_downloadWarningText != null)
                           {
                               _downloadWarningText.text = "The video could not be downloaded because your Internet connection doesn't work or is very slow";
                               _loadingVideoMessage.SetActive(false);
                               _downloadWarning.SetActive(true);
                           }
                           break;
                       case "CLOUD_STORAGE_ERROR":
                           if (_downloadWarningText != null)
                           {
                               _downloadWarningText.text = "There was a problem while downloading the video";
                               _loadingVideoMessage.SetActive(false);
                               _downloadWarning.SetActive(true);
                           }
                           break;
                       default:
                           // Loading Video File
                           StartCoroutine(HelperFunctions.LoadFileFromUri(_urlParams[1], videoData => {
                               if (videoData == null)
                               {
                                   _downloadWarningText.text = "There was a problem while downloading the video";
                                   _loadingVideoMessage.SetActive(false);
                                   _downloadWarning.SetActive(true);
                               }
                               else
                               {
                                   // Create directory if it doesn't exist yet
                                   if (!Directory.Exists(_videoDirectory))
                                   {
                                       Directory.CreateDirectory(_videoDirectory);
                                   }
                                   File.WriteAllBytes(_videoPath, videoData);
                                   // Load video in videoplayer once the file is saved
                                   _videoPlayer.url = _videoPath;
                               }
                           }));
                           break;
                   }
               }
           }).AddTo(gameObject);
        }

        private void OnEnable()
        {
            // Showing loading message
            _loadingVideoMessage.SetActive(true);
        }

        private void OnDisable()
        {
            // Release video texture and hide warnings
            _videoPlayer.targetTexture.Release();
            if (_downloadWarning.activeSelf)
                _downloadWarning.SetActive(false);
        }

        private void Update()
        {
            // Check when the video starts to hide the loading message
            if (_videoPlayer.isPlaying && _loadingVideoMessage.activeSelf)
            {
                _loadingVideoMessage.SetActive(false);
            }
        }
    }
}

