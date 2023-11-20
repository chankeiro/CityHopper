using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bercetech.Games.Fleepas
{
    public class RulesScreenConfiguration : MonoBehaviour
    {

        [SerializeField]
        private GameObject _nextScreenV1;
        [SerializeField]
        private GameObject _nextScreenV2;
        [SerializeField]
        private GameObject _nextScreenV3;
        [SerializeField]
        private GameObject _nextScreenV4;
        [SerializeField]
        private GameObject _nextScreenV5;
        private GameObject _nextScreen;
        [SerializeField]
        private GameObject _backScreenV1;
        [SerializeField]
        private GameObject _backScreenV2;
        [SerializeField]
        private GameObject _backScreenV3;
        [SerializeField]
        private GameObject _backScreenV4;
        [SerializeField]
        private GameObject _backScreenV5;
        private GameObject _backScreen;
        // Start is called before the first frame update

        public void OnEnable()
        {
            if (RulesVersion.SharedInstance.Version == 1)
            {
                _nextScreen = _nextScreenV1;
                _backScreen = _backScreenV1;
            } 
            else if (RulesVersion.SharedInstance.Version == 2)
            {
                _nextScreen = _nextScreenV2;
                _backScreen = _backScreenV2;
            }
            else if (RulesVersion.SharedInstance.Version == 3)
            {
                _nextScreen = _nextScreenV3;
                _backScreen = _backScreenV3;
            }
            else if (RulesVersion.SharedInstance.Version == 4)
            {
                _nextScreen = _nextScreenV4;
                _backScreen = _backScreenV4;
            }
            else
            {
                _nextScreen = _nextScreenV5;
                _backScreen = _backScreenV5;
            }

        }

        public void ShowNextScreen(int slideDirection)
        {
            var slideAnimation = _nextScreen.GetComponent<UISlideInAnimation>();
            if (slideAnimation != null)
                slideAnimation.Slide(slideDirection);
            else
                _nextScreen.SetActive(true);

        }

        public void ShowBackScreen(int slideDirection)
        {
            var slideAnimation = _backScreen.GetComponent<UISlideInAnimation>();
            if (slideAnimation != null)
                slideAnimation.Slide(slideDirection);
            else
                _backScreen.SetActive(true);
        }

    }
}
