using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bercetech.Games.Fleepas
{
    public class ScaleOut : MonoBehaviour
    {

        private Vector3 _initialScale;
        private int _leanScale;
        [SerializeField]
        private bool _disableOnScaleOut;


        void OnEnable()
        {
            if (_initialScale == Vector3.zero)
            {
                _initialScale = transform.localScale;
            }

            // Scaling Out Explosion            
            _leanScale = transform.LeanScale(Vector3.zero, 0.2f).setOnComplete((_) =>
            {
                if (_disableOnScaleOut)
                    StartCoroutine(HelperFunctions.DisableAfterPlay(gameObject));
            }).id;
        }

        private void OnDisable()
        {
            LeanTween.cancel(_leanScale);
            // Recovering initial scale
            transform.localScale = _initialScale;
            // Reenable renderers
            var renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.enabled = true;
            }
        }
    }
}
