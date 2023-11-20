using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Google.XR.ARCoreExtensions;
using Google.XR.ARCoreExtensions.Internal;
using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Google.XR.ARCoreExtensions.Samples.Geospatial
{
    public class GeoPath : MonoBehaviour
    {
        [SerializeField]
        private Vector4[] _pathCoordinatesAndTime; // Latitude, Longitude, Altitude from Terrain and Time to next path stop
        [SerializeField]
        private AREarthManager _earthManager; 

        private int _pathIndex;

        private GeospatialPose _geospatialPose = new();
        private Pose _pose;
        private float _speed;

        // Geospatial Session Parameters
        private bool _isSessionReady;
        private TrackingState _earthTrackingState;
        private GeospatialPose _cameraPose;
        private ARGeospatialCreatorAnchor _arGeoCreatorAnchor;

        // Start is called before the first frame update
        void Start()
        {
            _pathIndex = 0;
            _arGeoCreatorAnchor = GetComponent<ARGeospatialCreatorAnchor>();
            Debug.Log("GeoCreator:" + _arGeoCreatorAnchor.Latitude + "/" + _arGeoCreatorAnchor.Longitude + "/" + _arGeoCreatorAnchor.Altitude + "/" + _arGeoCreatorAnchor.AltitudeOffset + "/" + _arGeoCreatorAnchor.AltType);

        }

        // Update is called once per frame
        void Update()
        {
            if (isEarthStateReady())
            {
                // Initialize poses
                if (_geospatialPose.Altitude == 0)
                {
                    _pose = new Pose(_arGeoCreatorAnchor.transform.position, _arGeoCreatorAnchor.transform.rotation);
                    _geospatialPose = _earthManager.Convert(_pose); 
                    Debug.Log("GeoCreator:" + _arGeoCreatorAnchor.Latitude + "/" + _arGeoCreatorAnchor.Longitude + "/" + _arGeoCreatorAnchor.Altitude + "/" + _arGeoCreatorAnchor.AltitudeOffset + "/" + _arGeoCreatorAnchor.AltType);
                    Debug.Log("GeoSpatial Pose:" + _geospatialPose.Latitude + "/" + _geospatialPose.Longitude + "/" + _geospatialPose.Altitude);
                }

                if (Vector3.Distance(transform.position, _pose.position) > 1) {
                    transform.position = Vector3.MoveTowards(transform.position, _pose.position, _speed * Time.deltaTime);
                } else if (_pathIndex < _pathCoordinatesAndTime.Length)
                {
                    // Getting new pose from the next path point coordinates
                    getNextPose();
                    _pathIndex += 1;
                } 
            } 
        }


        private void getNextPose()
        {
            _pose = new Pose(_arGeoCreatorAnchor.transform.position, _arGeoCreatorAnchor.transform.rotation);
            Debug.Log("Index:" + _pathIndex);
            _geospatialPose = _earthManager.Convert(_pose);
            Debug.Log("GeoSpatial Pose1:" + _geospatialPose.Latitude + "/" + _geospatialPose.Longitude + "/" + _geospatialPose.Altitude);
            _geospatialPose.Latitude = _pathCoordinatesAndTime[_pathIndex].x;
            _geospatialPose.Longitude = _pathCoordinatesAndTime[_pathIndex].y;
            //_geospatialPose.Altitude = _arGeoCreatorAnchor.Altitude;

            Debug.Log("GeoSpatial Pose2:" + _geospatialPose.Latitude + "/" + _geospatialPose.Longitude + "/" + _geospatialPose.Altitude);
            _pose = _earthManager.Convert(_geospatialPose);
            _speed = Vector3.Distance(transform.position, _pose.position) / _pathCoordinatesAndTime[_pathIndex].w;
        }

        private bool isEarthStateReady()
        {
            // If EarthState is not enabled, return false
            if (_earthManager.EarthState == EarthState.ErrorEarthNotReady)
            {
                return false;
            }
            else if (_earthManager.EarthState != EarthState.Enabled)
            {
                return false;
            }
            // If the session is not ready, or the camera pose doesn't
            // have enough accuracy, return false
            _isSessionReady = ARSession.state == ARSessionState.SessionTracking &&
               Input.location.status == LocationServiceStatus.Running;
            _earthTrackingState = _earthManager.EarthTrackingState;
            _cameraPose = _earthTrackingState == TrackingState.Tracking ?
                _earthManager.CameraGeospatialPose : new GeospatialPose();
            if (!_isSessionReady || _earthTrackingState != TrackingState.Tracking ||
                _cameraPose.OrientationYawAccuracy > 15.0 ||
                _cameraPose.HorizontalAccuracy > 10)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
