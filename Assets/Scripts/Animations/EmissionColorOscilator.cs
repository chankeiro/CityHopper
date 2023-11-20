using UnityEngine;

namespace Bercetech.Games.Fleepas
{
    public class EmissionColorOscilator : MonoBehaviour
    {
        [SerializeField]
        private int _materialIndex;
        [SerializeField]
        private float _time;
        [SerializeField]
        private float _emissionVariation;
        private int _tweenMaterialId;
        private Material _material;
        private Color _initialColor;

        private void Start()
        {
            _material = GetComponent<Renderer>().materials[_materialIndex];
            _initialColor = _material.GetColor("_EmissionColor");

        }

        // Start is called before the first frame update
        void OnEnable()
        {
            _tweenMaterialId = LeanTween.value(1f - _emissionVariation, 1 + _emissionVariation, _time)
                .setEaseInOutSine().setLoopPingPong()
                .setOnUpdate((float val) =>
                {
                    if (_material != null)
                        _material.SetColor("_EmissionColor", val* _initialColor);
                }).id;

        }

        private void OnDisable()
        {
            LeanTween.cancel(_tweenMaterialId);
        }
    }

}