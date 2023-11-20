using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
using UnityEngine.Serialization;

namespace Bercetech.Games.Fleepas
{
    public class UIScaleInAnimation : MonoBehaviour
    {
        // Easing curves demo:
        // https://codepen.io/jhnsnc/pen/LpVXGM
        [SerializeField]
        private int _order = 1;
        public int Order => _order;
        private bool _animationFinished;
        public bool AnimationFinished => _animationFinished;
        [SerializeField]
        private bool _scaleInAtTheBeginning = true; // False to just see a jumping effect
        [SerializeField]
        private bool _applyIntermediateScale = false;
        [SerializeField]
        private Vector3 _intermediateScale = Vector3.zero;
        public bool ScaleOutAtTheEnd = false; // The object is also disabled
        [SerializeField]
        private float _timeUntilScaleOut = 0f;
        [SerializeField]
        private bool _runOnlyTheFirstTime; // The object is also disabled
        [SerializeField]
        private float _time = 0.7f;
        private Vector3 _endScale;
        private UIScaleInAnimation[] _scaleIns;
        // Event with the new score and powerUpsLevels
        private Signal<(GameObject, int)> _scaleInAnimationFinished = new Signal<(GameObject, int)>();
        public Signal<(GameObject, int)> ScaleInAnimationFinished => _scaleInAnimationFinished;
        private int? _lastOrder;

        void OnEnable()
        {
            // The animation is never finished at this point
            _animationFinished = false;
            // Check if the gameobject has other components like this. In such case, we have to check their order
            // to know when to start this component
            _scaleIns = gameObject.GetComponents<UIScaleInAnimation>();
            if (_scaleIns.Length > 1)
            {
                // Check if there is at least one enabled component with more priority than this component
                var lastScaleIn = _scaleIns.Where(si => si.enabled && si.Order < _order && !si.AnimationFinished).OrderBy(si => si.Order).LastOrDefault();

                if (lastScaleIn == null)
                {
                    // If not component with more priority is found, we can launch the animation
                    Animate();
                } else
                {
                    // If there is one component with more priority, we must wait for that component to finish
                    _lastOrder = lastScaleIn.Order;
                }
            }
            else
            {
                Animate();
            }

        }

        private void Start()
        {
            // Subscribe to the event that tell us that there components in this gameobject have finished
            ScaleInAnimationFinished.Subscribe(si =>
            {
                if (si.Item1 == gameObject && si.Item2 == _lastOrder)
                    Animate();
            }).AddTo(gameObject);
        }

        // This is the animation logic
        // We must add onAnimationFinished at the end of every potential path
        private void Animate()
        {
            // Save the initial scale to reuse it in all cases except when scaling out an the end
            _endScale = transform.localScale;
            // Jumping effect: the object starts with a scale > 0. Intermediate Scale is
            // > that the original scale to achieve the jumping effect
            // Use Intermediate Scale = 0 to just scale out (doubling the time)
            if (_scaleInAtTheBeginning)
            {
                transform.localScale = Vector2.zero;
            }
            if (_applyIntermediateScale)
            {
                // Splitting time by two in each scaling
                transform
                    .LeanScale(_intermediateScale, _time / 2f).setEaseOutSine()
                        .setOnComplete(_ => ApplyLastScale(_time / 2f));
                return;
            }
            // When no intermediate scale, just check if must scale out or not
            // Use all the tiem for it
            ApplyLastScale(_time);
        }

        private void ApplyLastScale(float time)
        {
            if (ScaleOutAtTheEnd)
            {
                transform.LeanScale(Vector2.zero, time).setEaseOutSine().setDelay(_timeUntilScaleOut)
                .setOnComplete(_ =>
                {
                    if (gameObject.activeInHierarchy)
                        StartCoroutine(DisableAfterPlay(gameObject));
                });
            }
            else
            {
                transform.LeanScale(_endScale, time).setEaseOutBack()
                .setOnComplete(_ => onAnimatonFinished());
            }
        }

        public IEnumerator DisableAfterPlay(GameObject gameObject)
        {
            // If a sound is playing, wait for it to finish
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                while (audioSource.isPlaying)
                {
                    yield return null;
                }
            }
            // Deactivating gameobject
            gameObject.SetActive(false);
            // Recovering original scale
            transform.localScale = _endScale;
            onAnimatonFinished();
        }

        // On animation finished we set it as finished and fire the event to let
        // other potential components with less priority know that it is finished
        // ,so they can start their animations
        private void onAnimatonFinished()
        {
            _animationFinished = true;
            if (_scaleIns.Length > _order)
            {
                _scaleInAnimationFinished.Fire((gameObject, _order));
            }
            if (_runOnlyTheFirstTime)
                enabled = false;
        }
    }
}
