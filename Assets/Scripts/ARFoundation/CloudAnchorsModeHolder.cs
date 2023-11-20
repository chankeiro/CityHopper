using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bercetech.Games.Fleepas
{

    public class CloudAnchorsModeHolder : MonoBehaviour
    {
        public enum CloudAnchorsMode
        {
            /// <summary>
            /// Doing nothing
            /// </summary>
            Idle,

            /// <summary>
            /// Hosting Cloud Anchors.
            /// </summary>
            Hosting,

            /// <summary>
            /// Resolving Cloud Anchors.
            /// </summary>
            Resolving,
        }
        private CloudAnchorsMode _cloudAnchorMode = CloudAnchorsMode.Idle;
        public CloudAnchorsMode CloudAnchorMode => _cloudAnchorMode;

        // Defining a static shared instance variable so other scripts can access to the object pool
        private static CloudAnchorsModeHolder _sharedInstance;
        public static CloudAnchorsModeHolder SharedInstance => _sharedInstance;

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
        }

        public void SetResolvingMode()
        {
            _cloudAnchorMode = CloudAnchorsMode.Resolving;
        }

        public void SetHostingMode()
        {
            _cloudAnchorMode = CloudAnchorsMode.Hosting;
        }

    }

}
