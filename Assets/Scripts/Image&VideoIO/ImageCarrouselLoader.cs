using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Bercetech.Games.Fleepas
{
    
    public class ImageCarrouselLoader : MonoBehaviour
    {
        [SerializeField]
        private GameObject _nextButton;
        [SerializeField]
        private GameObject _previousButton;
        [SerializeField]
        private Texture _alertTexture;
        private int _screenShotId;
        public int ScreenShotId => _screenShotId;
        private int _maxScreenShotId;
        private bool _firstShotAfterEnable;
        // This variable saves the last click direction (previous or next)
        private bool _lastWasNext;


        // Update is called once per frame
        void OnEnable()
        {
            // Always show first screenShot when enabling the carrousel
           _screenShotId = 1;
            ActivateArrowButtons(_screenShotId);
            _firstShotAfterEnable = true;
            ChangeScreenShot();
        }

        public void OnClickButton(bool isNextButton)
        {
            _screenShotId += isNextButton? 1: -1;
            _lastWasNext = isNextButton;
            ActivateArrowButtons(_screenShotId);

            // Updating Screen shot
            ChangeScreenShot();
        }



        private void ChangeScreenShot()
        {
            StartCoroutine(ScreenShotSaver.SharedInstance.LoadScreenShot("ss_" + _screenShotId + ".jpg", true, t => {
                if (t != null)
                {
                    GetComponent<RawImage>().texture = t;
                }
                else
                {
                    // Show alternative texture in case of receiving null data
                    GetComponent<RawImage>().texture = _alertTexture;
                }
                // Slide from lateral
                if (!_firstShotAfterEnable) // To prevent animation OnEnable
                {
                    transform.localPosition = new Vector2(
                        _lastWasNext ? Screen.width : -1 * Screen.width,
                        transform.localPosition.y
                        );
                    transform.LeanMoveLocalX(0, 0.5f).setEaseOutCubic();
                } else
                {
                    _firstShotAfterEnable = false;
                }
            }));
        }

        // Enabling disabling arrows buttons depending on the situation
        public void ActivateArrowButtons(int screenShotId)
        {
            if (screenShotId == 1 & screenShotId == _maxScreenShotId)
            {
                _previousButton.SetActive(false);
                _nextButton.SetActive(false);
            }
            else if (screenShotId == 1)
            {
                _previousButton.SetActive(false);
                _nextButton.SetActive(true);
            }
            else if (screenShotId == _maxScreenShotId)
            {
                _previousButton.SetActive(true);
                _nextButton.SetActive(false);
            }
            else
            {
                _previousButton.SetActive(true);
                _nextButton.SetActive(true);
            }
        }

        public void SetMaximumScreenShotId(int maxScreenShotId)
        {
            _maxScreenShotId = maxScreenShotId;
        }

    }
}
