using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;


namespace Bercetech.Games.Fleepas
{
    // Fading in video (helps to hide the initial black texture because of the loading time)
    public class FadingImage : MonoBehaviour
    {
        [SerializeField]
        private float _fadingTime = 0.5f;
        [SerializeField]
        private float _delay = 0f;
        [SerializeField]
        private FadeMode _fadeMode;
        [SerializeField]
        private ComponentType _componentType;
        [SerializeField]
        private bool _fadeOnEnable;
        private Color _imageColor;
        private float _alpha;
        private Image _image;
        private RawImage _rawImage;
        private CanvasGroup _canvasGroup;
        private bool fadingActive;
        private float startTime;
        private float _transparencyOut;
        private float _transparencyInOut;

        // CompositeDisposable is similar with List<IDisposable>
        // It will be used to gather all disposables active after the game is finished
        // so they can be disposed at that moment
        protected CompositeDisposable disposables = new CompositeDisposable();
        private void OnDestroy()
        {
            disposables.Clear();
        }

        private enum FadeMode
        {
            FadeOut,
            FadeInOut,
            FadeIn
        }

        private enum ComponentType
        {
            Image,
            RawImage,
            Canvasgroup
        }

        private void Awake()
        {
            fadingActive = false;
            // Setting components to avoid a lot of GetComponent calls during the 
            // in the fade method
            if (_componentType != ComponentType.Canvasgroup)
            {
               if (_componentType == ComponentType.Image)
                {
                    _image = GetComponent<Image>();
                    _imageColor = _image.color;
                } else
                {
                    _rawImage = GetComponent<RawImage>();
                    _imageColor = _rawImage.color;
                }
            } else
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            // If we want the fade effect when the object is enabled in the scene
            if (_fadeOnEnable)
            {
                LaunchFade();
            }
        }

        public void Fade()
        {
            if (!fadingActive)
            {
                LaunchFade();
            }
        }

        private void LaunchFade()
        {
            // Reset alpha before delay
            switch (_fadeMode)
            {
                case FadeMode.FadeInOut:
                    _alpha = 0;
                    break;
                case FadeMode.FadeOut:
                    _alpha = 1;
                    break;
                case FadeMode.FadeIn:
                    _alpha = 0;
                    break;
            }
            SetAlpha(_alpha);
            Observable.Timer(TimeSpan.FromSeconds(_delay)).TakeUntilDisable(gameObject).Subscribe(_ =>
            {
                startTime = Time.time;
                fadingActive = true;
            }).AddTo(disposables);
        }

        private void Update()
        {
            if (fadingActive)
            {
                // Different alpha values depending on the fade mode
                _transparencyOut = Math.Max(0f, Math.Min(1f, 1 - (Time.time - startTime) / _fadingTime));
                _transparencyInOut = (Time.time - startTime) < _fadingTime/2f
                ? Math.Min(1f, Mathf.Pow(Time.time - startTime, 1) / Mathf.Pow(_fadingTime/2f, 1))
                : Math.Max(0f, Math.Min(1f, 2 - (Time.time - startTime) / (_fadingTime/2f)));
                switch (_fadeMode)
                {
                    case FadeMode.FadeInOut:
                        _alpha = _transparencyInOut;
                        break;
                    case FadeMode.FadeOut:
                        _alpha = _transparencyOut;
                        break;
                    case FadeMode.FadeIn:
                        _alpha = 1 - _transparencyOut;
                        break;
                }
                SetAlpha(_alpha);
                if (_transparencyOut == 0)
                {
                    fadingActive = false;
                    // Hiding object when ends with alpha 0
                    if (_fadeMode != FadeMode.FadeIn)
                        gameObject.SetActive(false);
                }
            }
        }

        private void OnDisable()
        {
            fadingActive = false;
        }

        private void SetAlpha(float alpha)
        {
            if (_componentType != ComponentType.Canvasgroup)
            {
                if (_imageColor == null)
                    Logging.OmigariHP("ImageColor Null");
                _imageColor.a = _alpha;
                if (_componentType == ComponentType.Image)
                    _image.color = _imageColor;
                else
                    _rawImage.color = _imageColor;
            }
            else
                _canvasGroup.alpha = _alpha;
        }

        public float GetTotalFadingTime()
        {
            return _fadingTime + _delay;
        }

        public void FadeOnEnableOff()
        {
            _fadeOnEnable = false;
            switch (_fadeMode)
            {
                case FadeMode.FadeInOut:
                    _alpha = 1;
                    break;
                case FadeMode.FadeOut:
                    _alpha = 0;
                    break;
                case FadeMode.FadeIn:
                    _alpha = 1;
                    break;
            }
            SetAlpha(_alpha);
        }
    }
}
