using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Bercetech.Games.Fleepas
{
    public class DownloadsReceiver : MonoBehaviour
    {
        private Signal<string> _downloadUrlReceived = new Signal<string>();
        public Signal<string> DownloadUrlReceived => _downloadUrlReceived;

        // Defining a static shared instance variable so other scripts can access to the object pool
        private static DownloadsReceiver _sharedInstance;
        public static DownloadsReceiver SharedInstance => _sharedInstance;

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

        public void ReceiveDownloadUrlFromAndroid(string downloadUrlData)
        {
            _downloadUrlReceived.Fire(downloadUrlData);
        }

    }
}
