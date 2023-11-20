using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace Bercetech.Games.Fleepas.CityBunny
{
    public class HammerHitSide : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.name == "Bunny")
            {
                // Play Hit
                _audioSource.Play();
            }

        }

    }
}
