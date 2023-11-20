using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Bercetech.Games.Fleepas
{
    public class RulesVersion : MonoBehaviour
    {
        private int _version;
        public int Version => _version;

        // Defining a static shared instance variable so other scripts can access to the object
        private static RulesVersion _sharedInstance;
        public static RulesVersion SharedInstance => _sharedInstance;
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

        // This function must be called before activating the Rules screens, because they use
        // the value to know which rules go after/before
        public void SetRulesVersion(int version)
        {
            _version = version;
        }

    }
}
