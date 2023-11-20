using UnityEngine;

namespace Bercetech.Games.Fleepas
{
    public class PositionOscilator : MonoBehaviour
    {
        [SerializeField]
        private float _time;
        [SerializeField]
        private Vector3 _movement;
        private int _tweenMoveXId;
        private int _tweenMoveYId;
        private int _tweenMoveZId;


        // Start is called before the first frame update
        void OnEnable()
        {
            if (_movement.x != 0)
                _tweenMoveXId = LeanTween.moveLocalX(gameObject, transform.localPosition.x + _movement.x , _time).setEaseInOutSine().setLoopPingPong().id;
            if (_movement.y != 0)
                _tweenMoveYId = LeanTween.moveLocalY(gameObject, transform.localPosition.y + _movement.y, _time).setEaseInOutSine().setLoopPingPong().id;
            if (_movement.z != 0)
                _tweenMoveZId = LeanTween.moveLocalZ(gameObject, transform.localPosition.z + _movement.z, _time).setEaseInOutSine().setLoopPingPong().id;
        }

        private void OnDisable()
        {
            LeanTween.cancel(_tweenMoveXId);
            LeanTween.cancel(_tweenMoveYId);
            LeanTween.cancel(_tweenMoveZId);
        }
    }

}