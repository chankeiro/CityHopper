using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Bercetech.Games.Fleepas
{
    public class SignTarget : MonoBehaviour
    {
        [SerializeField]
        protected string _bulletTag;
        private AudioSource _audioSource;
        private Renderer _renderer;
        private Collider _collider;
        [SerializeField]
        private string _signEventName;
        [SerializeField]
        private float _signEventDuration;
        // Publish the hit Stream, providing additional data
        private static Signal<Tuple<string, float>> _signEvent = new Signal<Tuple<string, float>>();
        public static Signal<Tuple<string, float>> SignEvent => _signEvent;
        private int _tweenID;
        private int _tweenID2;


        private void Start()
        {
            _audioSource = gameObject.GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (_renderer == null)
                _renderer = gameObject.GetComponent<Renderer>();
            _renderer.enabled = true;
            if (_collider == null)
                _collider = gameObject.GetComponent<Collider>();
            _collider.enabled = true;
            // Start fading in
            _renderer.material.SetFloat("_Alpha", 0f);
            _tweenID = LeanTween.value(0f, 1f, 3f).setEaseInOutCubic().setOnUpdate((float val) =>
            {
                _renderer.material.SetFloat("_Alpha", val);
            }).id;
            // Infinite rotation movement
            _tweenID2 = gameObject.LeanRotateAround(Vector3.up, 180, 2).setRepeat(-1).id;
        }


        private void OnDisable()
        {
            // Stop tweening
            LeanTween.cancel(_tweenID);
            LeanTween.cancel(_tweenID2);
        }

        protected virtual void OnCollisionEnter(Collision collisionInfo)
        {
            // Check if a bullet collides with this object
            if (collisionInfo.collider.tag == _bulletTag)
            {
                _audioSource.PlayOneShot(_audioSource.clip);
                // Hide Object. Wait for the sound to finish
                StartCoroutine(HelperFunctions.DisableAfterPlay(gameObject));
                // Slow Targets Speed
                _signEvent.Fire(Tuple.Create( _signEventName, _signEventDuration));
            }
        }

    }

}
