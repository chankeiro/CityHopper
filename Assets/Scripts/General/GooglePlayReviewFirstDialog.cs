using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Bercetech.Games.Fleepas
{
    public class GooglePlayReviewFirstDialog : MonoBehaviour
    {
        [SerializeField]
        private GameObject _hiddenSkipButton;
        private void OnEnable()
        {
            // If the review info is not available, it would mean that either there
            // was an error trying to retrieve it, or that we didn't ask for it
            // because conditions weren't met. In such case, go directly to the next screen
            // using the skip button
            if (!GooglePlayReviewManager.SharedInstance.CheckGoogleReviewAvailable())
            {
                // Move to next screen in case there aren't more rounds with new birds
                _hiddenSkipButton.GetComponent<Button>().onClick.Invoke();
            }
        }

        
    }
}
