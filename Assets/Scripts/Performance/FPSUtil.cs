using UnityEngine;
using System.Collections;
using TMPro;

public class FPSUtil : MonoBehaviour
{

    private float _frequency = 1.0f;
    private TextMeshProUGUI _fpsLabel;

    private int _lastFrameCount;
    private float _lastTime;
    private WaitForSeconds _waitForSeconds;
    private float _timeSpan;
    private int _frameCount;



    void Start()
    {
        _waitForSeconds = new WaitForSeconds(_frequency);
        StartCoroutine(FPS());
        _fpsLabel = GetComponent<TextMeshProUGUI>();
    }

    private IEnumerator FPS()
    {
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

            // Display it
            _fpsLabel.text = string.Format("FPS: {0}", Mathf.RoundToInt(_frameCount / _timeSpan));

        }
    }


}
