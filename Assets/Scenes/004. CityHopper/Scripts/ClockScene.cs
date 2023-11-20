using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace Bercetech.Games.Fleepas.CityBunny
{
    public class ClockScene : Clock
    {
        [SerializeField]
        private int _timeIncreaseByDamage;
        private float _clockColorH;
        private float _clockColorS;
        private float _clockColorV;
        private int _leanColor;
        // Subscribe to the bunny damage to increase clock value
        override protected void Start()
        {
            base.Start();
            Color.RGBToHSV(_clockText.color, out _clockColorH, out _clockColorS, out _clockColorV);
            Bunny.DamageTaken.Subscribe(_ =>
            {
                // Add damage time and flash the clock
                _timePassed += _timeIncreaseByDamage;
                _leanColor = LeanTween.value(0f, 1f, 0.3f).setEaseInOutSine().setRepeat(3)
                .setOnUpdate((float val) =>
                {
                    _clockText.color = Color.HSVToRGB(_clockColorH, val, _clockColorV);
                })
                .setOnComplete((_) =>
                {
                    _clockText.color = Color.HSVToRGB(_clockColorH, _clockColorS, _clockColorV);
                }).id;

            }).AddTo(gameObject);
        }

        override protected void OnDisable()
        {
            base.OnDisable();
            // Reser color values
            LeanTween.cancel(_leanColor);
            _clockText.color = Color.HSVToRGB(_clockColorH, _clockColorS, _clockColorV);
        }



    }
}
