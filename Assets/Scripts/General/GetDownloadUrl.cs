using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Bercetech.Games.Fleepas
{
    public class GetDownloadUrl : MonoBehaviour
    {
        public string urlPath;
        void OnEnable()
        {
            MainActivityMessagingManager.GetDownloadUrl(urlPath);
        }

    }

}