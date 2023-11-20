using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;

namespace Bercetech.Games.Fleepas
{

    public class ParallaxMovement : MonoBehaviour
    {
        [SerializeField]
        private float _phoneAccelerationTimeStep = 0.2f;
        [SerializeField]
        private float _parallaxStrengthPositionX;
        [SerializeField]
        private float _parallaxStrengthPositionY;
        [SerializeField]
        private float _parallaxStrengthRotationX;
        [SerializeField]
        private float _parallaxStrengthRotationY;
        private Vector3 _initialPosition;
        private Vector2 _parallaxAim;
        // Start is called before the first frame update
        void Start()
        {
            _initialPosition = transform.localPosition;
            Observable.Interval(TimeSpan.FromSeconds(_phoneAccelerationTimeStep)).StartWith(0).TakeUntilDisable(gameObject).Subscribe(_ =>
            {
#if UNITY_EDITOR
                _parallaxAim = 2 * (Camera.main.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f, 0));
#else
                _parallaxAim = new Vector2(2 * Mathf.Clamp(1*Input.acceleration.x, -0.5f, 0.5f), 2 * Mathf.Clamp((1*(Input.acceleration.y + 0.5f)), -0.5f, 0.5f));
#endif
                gameObject.LeanMoveLocalX(_initialPosition.x + _parallaxStrengthPositionX * 1080 * _parallaxAim.x, _phoneAccelerationTimeStep);
                gameObject.LeanMoveLocalY(_initialPosition.y + _parallaxStrengthPositionY * 1920 * _parallaxAim.y, _phoneAccelerationTimeStep);
                gameObject.LeanRotateY(-1 * _parallaxStrengthRotationX * 90 * _parallaxAim.x, _phoneAccelerationTimeStep);
                gameObject.LeanRotateX(_parallaxStrengthRotationY * 90 * _parallaxAim.y, _phoneAccelerationTimeStep);
            }).AddTo(gameObject);




        }



        //// Update is called once per frame
        //void Update()
        //{
        //    //Debug.Log(-Input.acceleration.y + "/" + Input.acceleration.x);
        //    //_parallaxAim = 2 * (Camera.main.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f, 0));
        //    _parallaxAim = new Vector2(2*Mathf.Clamp(-Input.acceleration.x, -0.5f, 0.5f), 2 * Mathf.Clamp((-Input.acceleration.y -0.5f), -0.5f, 0.5f));
        //    gameObject.LeanMoveLocalX(_initialPosition.x + _parallaxStrengthPositionX * 1080 * _parallaxAim.x, Time.deltaTime);
        //    gameObject.LeanMoveLocalY(_initialPosition.y + _parallaxStrengthPositionY * 1920 * _parallaxAim.y, Time.deltaTime);
        //    gameObject.LeanRotateY(-1 * _parallaxStrengthRotationX * 90 * _parallaxAim.x, Time.deltaTime);
        //    gameObject.LeanRotateX(_parallaxStrengthRotationY * 90 * _parallaxAim.y, Time.deltaTime);
        //}
    }
}
