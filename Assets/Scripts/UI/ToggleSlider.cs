using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Bercetech.Games.Fleepas
{
    public class ToggleSlider : MonoBehaviour
    {

        private Slider _slider;

        // Update is called once per frame

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }
        public void Toggle()
        {
            _slider.value = -_slider.value + 1;
        }
    }
}
