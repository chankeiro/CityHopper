using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;



namespace Bercetech.Games.Fleepas
{
    // This script is very important for other classes that
    // depend on the camera fov value, like the shoot class.
    [RequireComponent(typeof(Camera))]
    public class ARCameraFOV : MonoBehaviour
    {
        private Camera _arCamera;

        private void OnEnable()
        {
            _arCamera = GetComponent<Camera>();
            Debug.Log("FOV:" + _arCamera.fieldOfView);
        }

        // Update is called once per frame
        void Update()
        {
            // This is the logic for ARFoundation, but starting from vs 4.1.3 : "The
            // ARCameraBackground component now sets the camera's field of view. Because
            // the ARCameraBackground already overrides the camera's projection matrix,
            // this has no effect on ARFoundation. However, any code that reads the
            // camera's fieldOfView property will now read the correct value." Therefore,
            // it is not necessary to calculate it as with ARDK. I checked it and the value
            // for both AR systems are very similar. I keep the code here just in case.
            //if (!_isFOVARFCalculated & _arFoundationCameraManager.subsystem != null)
            //{
            //    XRCameraParams cameraParams = new XRCameraParams
            //    {
            //        zNear = _arCamera.nearClipPlane,
            //        zFar = _arCamera.farClipPlane,
            //        screenWidth = Screen.width,
            //        screenHeight = Screen.height,
            //        screenOrientation = Screen.orientation
            //    };

            //    XRCameraFrame cameraFrame;
            //    if (_arFoundationCameraManager.subsystem.TryGetLatestFrame(cameraParams, out cameraFrame))
            //    {
            //        var t = cameraFrame.projectionMatrix.m11;
            //        var fov = Mathf.Atan(1.0f / t) * 2.0f * Mathf.Rad2Deg;
            //        Debug.Log("AR matrix fov:" + fov + "/" + t);
            //        Debug.Log("Current fov: " + _arCamera.fieldOfView);
            //        _isFOVARFCalculated = true;
            //    }
            //    else
            //    {
            //        Debug.Log("AR matrix fov: FAILED");
            //    }
            //}

        }


    }
}
