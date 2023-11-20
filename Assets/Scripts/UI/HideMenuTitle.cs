using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bercetech.Games.Fleepas
{
    public class HideMenuTitle : MonoBehaviour
    {

        [SerializeField]
        private GameObject _arIcon;
        [SerializeField]
        private GameObject[] _buttonsToEnable;
        [SerializeField]
        private UISlideInAnimation _slideInAnimation;
        [SerializeField]
        private GameObject _particlesSystem;
        [SerializeField]
        private float _imagesAspectRatio;


        private void OnEnable()
        {
            // Adjusting the scale of this element, to ensure that the images
            // in the child gameobjects fill the screen
            transform.localScale = (Camera.main.aspect/ _imagesAspectRatio) * Vector3.one;
        }

        public void HideTitle()
        {
            _arIcon.SetActive(true);
            foreach (var button in _buttonsToEnable)
                button.SetActive(true);
            _slideInAnimation.enabled = true;
            // Deactive partycle system before enabling ARDK session. Otherwise there is a glitch in the image
            // probably because the particle systems renders in a camera, and the ARDK camera is initialized at the same time
            // Even deactivating the particle system, the last frame keeps rendering on the canvas image
            if (_particlesSystem != null)
                _particlesSystem.SetActive(false); 
        }


    }
}
