using UnityEngine;

namespace Bercetech.Games.Fleepas
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField]
        private float _rotationTime;
        [SerializeField]
        private Vector3 _rotationVector;
        private int _tweenRotateId;


        // Start is called before the first frame update
        void OnEnable()
        {
            _tweenRotateId = LeanTween.rotateAround(gameObject, _rotationVector, 360, _rotationTime).setRepeat(-1).id;
        }

        private void OnDisable()
        {
            LeanTween.cancel(_tweenRotateId);
        }
    }

}
