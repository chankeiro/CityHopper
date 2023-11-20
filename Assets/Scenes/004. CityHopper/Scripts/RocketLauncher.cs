using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace Bercetech.Games.Fleepas.CityBunny
{
    public class RocketLauncher : MonoBehaviour
    {

        [SerializeField]
        private float _launchFrequency;
        [SerializeField]
        private string _rocketName;
        [SerializeField]
        private Vector3 _launchDirection;
        private SkinnedMeshRenderer _launcherMesh;
        private int _launchInitAnimation;
        private int _launchEndAnimation;

        private void Awake()
        {
            _launcherMesh = GetComponent<SkinnedMeshRenderer>();
        }

        void OnEnable()
        {
            Observable.Interval(TimeSpan.FromSeconds(_launchFrequency))
                .StartWith(0).TakeUntilDisable(gameObject).Subscribe(_ =>
                {
                    // Animate the launcher
                    _launchInitAnimation = LeanTween.value(0, 100, 1f).setEaseOutCubic().setOnUpdate((val) =>
                    {
                        // Prepare for the shot
                        _launcherMesh.SetBlendShapeWeight(0, val);
                    }).setOnComplete((_) =>
                    {
                        // And now shot
                        // Finish animation
                        _launchEndAnimation = LeanTween.value(100, 0, 0.25f).setEaseInExpo().setOnUpdate((val) =>
                        {
                            _launcherMesh.SetBlendShapeWeight(0, val);

                        }).setOnComplete((_) => {
                            // Get rocket from pool
                            GameObject rocket = ObjectPool.SharedInstace.GetPooledObjectByName(_rocketName);
                            // Start below launcher exit and with the same orientation 
                            rocket.transform.position = transform.position - 2f * Vector3.up;
                            rocket.transform.rotation = transform.rotation;
                            LeanTween.cancel(rocket); // NEed to disable any previous leanmove in case the rocket exploded
                            rocket.SetActive(true);
                            // Mode 100m in the selected direction
                            rocket.LeanMove(transform.position + 100*_launchDirection, 5).setOnComplete((_) => rocket.SetActive(false));
                        }).id;
                    }).id;

                }).AddTo(gameObject);
        }

        private void OnDisable()
        {
            LeanTween.cancel(_launchInitAnimation);
            LeanTween.cancel(_launchEndAnimation);
        }
    }
}
