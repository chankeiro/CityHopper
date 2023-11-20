using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace Bercetech.Games.Fleepas.CityBunny
{
    public class Hammer : MonoBehaviour
    {
        [SerializeField]
        private GameObject _hammerAxis;
        [SerializeField]
        private float _cycleInitTimeOffSet;
        private int _leanRotateUp;
        private int _leanRotateDown;
        private AudioSource _audioSource;

        void Start()
        {
            Observable.Interval(TimeSpan.FromSeconds(5f)).StartWith(0).Subscribe(_ =>
            {
                _audioSource = GetComponent<AudioSource>();
                // Loop to hit the ground
                _leanRotateUp = _hammerAxis.LeanRotateZ(-90, 2.5f).setEaseInOutSine().setDelay(_cycleInitTimeOffSet)
                .setOnComplete(_ =>
                {
                    _leanRotateDown = _hammerAxis.LeanRotateZ(0, 0.2f).setEaseOutBounce().id;
                    if (!_audioSource.isPlaying)
                    {
                        _audioSource.Play();
                    }
                }).id;
            }).AddTo(gameObject);

        }

        private void OnDisable()
        {
            LeanTween.cancel(_leanRotateUp);
            LeanTween.cancel(_leanRotateDown);
        }

    }
}
