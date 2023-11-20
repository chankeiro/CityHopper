using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

namespace Bercetech.Games.Fleepas
{
    public class CountDownClockShadowedIcon : MonoBehaviour
    {

        [SerializeField]
        private GameObject _iconMask;
        [SerializeField]
        private Image _countDownClockShadow;
        [SerializeField]
        private string _signEventName;
        private int _tweenCountDownId;

        private void Start()
        {
            // Check if the sign event has the configured name
            // and show icon in such case
            SignTarget.SignEvent.Subscribe(data =>
            {
                if (data.Item1 == _signEventName)
                    ShowIcon(data.Item2);
            }).AddTo(gameObject);

        }

        private void OnDisable()
        {
            // This is just in case the icon group is disabled
            // before the countdown is finished because the round finishes earlier
            if (_iconMask.activeSelf)
                _iconMask.SetActive(false);

            LeanTween.cancel(_tweenCountDownId);
        }

        private void ShowIcon(float duration)
        {
            _iconMask.SetActive(true);
            // Countdown to hide Icon
            Observable.Timer(TimeSpan.FromSeconds(duration))
                .TakeUntilDisable(gameObject)
                .Subscribe(_ =>
                {
                    // Disable After CountDown
                    _iconMask.SetActive(false);
                }).AddTo(gameObject);
            // Clock effect
            _countDownClockShadow.fillAmount = 0f;
            _tweenCountDownId = LeanTween.value(0, 1f, duration).setOnUpdate((float val) =>
            {
                _countDownClockShadow.fillAmount = val;
            }).id;
            
        }
    }
}
