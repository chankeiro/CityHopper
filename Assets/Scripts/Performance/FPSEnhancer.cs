using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bercetech.Games.Fleepas
{
    public class FPSEnhancer : MonoBehaviour
    {
        [SerializeField]
        private float _fpsCheckFrequency = 3.0f;
        private int _lastFrameCount;
        private float _lastTime;
        private WaitForSeconds _waitForSeconds;
        private float _timeSpan;
        private int _frameCount;
        private float _estimatedFPS;

        // Defining a static shared instance variable so other scripts can access to the object
        private static FPSEnhancer _sharedInstance;
        public static FPSEnhancer SharedInstance => _sharedInstance;

        void Awake()
        {
            // If there is an instance, and it's not me, delete myself.
            if (_sharedInstance != null && _sharedInstance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _sharedInstance = this;
            }

            // The Screen DPIs can be configured at Player Settings/Resolution and Presentation
            // for instance, with a Resolution Scaling Mode = Fixed DPI -> Target DPI = 300
            // that should be enough resolution for this kind of experience. But currently we are not using it.
            // Not modifying Target DPI o Quality Setting at runtime because the screen gets frozen

            Logging.Omigari("resolution:" + Screen.currentResolution);

            _waitForSeconds = new WaitForSeconds(_fpsCheckFrequency);
            // Starting with 60 as framerate
            Application.targetFrameRate = 60;
            StartCoroutine(FPS());

        }

        private IEnumerator FPS()
        {
            // Checking every X seconds if we must increase or decrease the target FPS
            while (true)
            {
                // Capture frame-per-second
                _lastFrameCount = Time.frameCount;
                // Time.Time is paused when the application is paused, but Time.realtimeSinceStartup is not paused.
                // Therefore, we use Time.Time here
                _lastTime = Time.time;
                yield return _waitForSeconds;
                _timeSpan = Time.time - _lastTime;
                _frameCount = Time.frameCount - _lastFrameCount;
                _estimatedFPS = _frameCount / _timeSpan;
                Logging.Omigari("estimatedFPS: " + _estimatedFPS);
                if (_estimatedFPS < 45f & Application.targetFrameRate == 60)
                {
                    Logging.Omigari("reducing FPS");
                    Application.targetFrameRate = 30;
                    // This affects to Project Settings/Time/Fixed Timestep
                    // It must match the target framerate, although 0.02 is ok for 60FPS
                    Time.fixedDeltaTime = 0.0334f;
                    continue;
                }
                if (_estimatedFPS > 29f & Application.targetFrameRate == 30)
                {
                    Logging.Omigari("Increasing FPS");
                    Time.fixedDeltaTime = 0.0167f;
                    Application.targetFrameRate = 60;
                    continue;
                }
            }
        }
    }
}
