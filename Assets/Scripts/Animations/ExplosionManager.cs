using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Bercetech.Games.Fleepas
{
    public class ExplosionManager : MonoBehaviour
    {

        [SerializeField]
        private GameObject[] _explosions;
        private int _explosionIndex;
        private ParticleSystem _explosionParticles;
        private Renderer[] _renderers;
        [SerializeField]
        private Renderer[] _notRenderAfterEnabling;
        private Collider[] _colliders;
        private Canvas[] _canvas;
        private bool _explosionCalled;

        private void OnEnable()
        {
            foreach (GameObject exp in _explosions)
                exp.SetActive(false);
            _explosionCalled = false;
            // Enabling Renderers and colliders
            if (_renderers == null)
            {
                _renderers = GetComponentsInChildren<Renderer>();
                _colliders = GetComponentsInChildren<Collider>();
                _canvas = GetComponentsInChildren<Canvas>();
            }
            foreach (Renderer rend in _renderers)
                if (!_notRenderAfterEnabling.Contains(rend))
                    rend.enabled = true;
            foreach (Collider coll in _colliders)
                coll.enabled = true;
            foreach (Canvas can in _canvas)
                can.enabled = true;
        }
        public void Explode(int explosionIndex)
        {
            _explosionIndex = explosionIndex;
            _explosionCalled = true;
            // Disabling renderers and collider
            foreach (Renderer rend in _renderers)
                rend.enabled = false;
            foreach (Collider coll in _colliders)
                coll.enabled = false;
            foreach (Canvas can in _canvas)
                can.enabled = false;
            // Enabling explosion
            _explosions[_explosionIndex].SetActive(true);
            _explosionParticles = _explosions[_explosionIndex].GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (_explosionParticles != null)
            {
                if (_explosionParticles.isStopped & _explosionCalled)
                {
                    // Disabling gameobject
                    _explosions[_explosionIndex].SetActive(false);
                    _explosionCalled = false;
                    StartCoroutine(HelperFunctions.DisableAfterPlay(gameObject, true));

                }
            }
        }
    }
}
