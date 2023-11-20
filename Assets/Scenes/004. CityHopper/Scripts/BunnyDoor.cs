using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bercetech.Games.Fleepas.CityBunny
{
    public class BunnyDoor : MonoBehaviour
    {
        [SerializeField]
        private GameObject _doorAxis;
        private int _openDoorLean;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void OpenDoor()
        {
            _openDoorLean = _doorAxis.LeanRotateAround(Vector3.up, 90f, 1).setEaseOutBounce().id;
            _audioSource.Play();
        }

        private void OnDisable()
        {
            LeanTween.cancel(_openDoorLean);
        }
    }
}
