using UnityEngine;
using TMPro;

namespace Bercetech.Games.Fleepas
{
    public class AlphaTextOscilator : MonoBehaviour
    {
        [SerializeField]
        private float _time;
        [SerializeField]
        private float _minAlpha;
        private TextMeshProUGUI _text;
        private Color _textColor;
        private int _tweenAlpha;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _textColor = _text.color;
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            _tweenAlpha = LeanTween.value(1, _minAlpha, _time).setEaseInOutSine().setLoopPingPong()
                .setOnUpdate((alpha) =>
                {
                    _textColor.a = alpha;
                    _text.color = _textColor;
                }).id;
        }

        private void OnDisable()
        {
            LeanTween.cancel(_tweenAlpha);
            _textColor.a = 1f;
            _text.color = _textColor;

        }
    }

}