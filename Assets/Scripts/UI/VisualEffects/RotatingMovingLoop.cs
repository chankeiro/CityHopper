// Copyright 2021 Niantic, Inc. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bercetech.Games.Fleepas
{
    public class RotatingMovingLoop : MonoBehaviour
    {
        [SerializeField]
        private float _angularRotationX = 0f;
        [SerializeField]
        private float _angularRotationY = 30f;
        [SerializeField]
        private float _angularRotationZ = 0f;
        [SerializeField]
        private float _velocityX = 0f;
        [SerializeField]
        private float _velocityY = 0.001f;
        [SerializeField]
        private float _velocityZ = 0f;

        // Let's move it a little bit        
        void Update()
        {
            // Rotation on the vertical axis
            transform.Rotate(Vector3.right * (_angularRotationX * Time.deltaTime));
            transform.Rotate(Vector3.up * (_angularRotationY * Time.deltaTime));
            transform.Rotate(Vector3.forward * (_angularRotationZ * Time.deltaTime));
            
            // Vertical movement along the rotation
            var pos = transform.position;
            pos.x += _velocityX * Mathf.Sin(Vector3.SignedAngle(Vector3.forward, transform.forward, Vector3.right) * Mathf.PI / 180);
            pos.y += _velocityY * Mathf.Sin(Vector3.SignedAngle(Vector3.forward, transform.forward, Vector3.up) * Mathf.PI / 180);
            pos.z += _velocityZ * Mathf.Sin(Vector3.SignedAngle(Vector3.up, transform.up, Vector3.forward) * Mathf.PI / 180);
            transform.position = pos;
        }
    }
}
