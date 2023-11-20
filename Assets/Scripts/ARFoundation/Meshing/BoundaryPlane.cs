using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;


namespace Bercetech.Games.Fleepas
{
    public class BoundaryPlane : MonoBehaviour
    {

        [SerializeField]
        private float _minimumDistanceOppositeSide;
        [SerializeField]
        private string _planeTag;
        [SerializeField]
        private bool _ymaxPlane;
        [SerializeField]
        private bool _yminPlane;
        [SerializeField]
        private bool _xmaxPlane;
        [SerializeField]
        private bool _xminPlane;
        [SerializeField]
        private bool _zmaxPlane;
        [SerializeField]
        private bool _zminPlane;
        private float[] _meshBounds;
        public float[] MeshBounds => _meshBounds;
        private List<Bounds> _boundaryPlanesBounds = new List<Bounds>();
        public List<Bounds> BoundaryPlanesBounds => _boundaryPlanesBounds;

        // Defining a static shared instance variable so other scripts can access to the object pool
        private static BoundaryPlane _sharedInstance;
        public static BoundaryPlane SharedInstance => _sharedInstance;

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
        }

        public void UpdateARMeshBoundaryPlanes(GameObject arMesh)
        {
            // Update mesh limits
            GetARMeshBounds(arMesh);
            // Drawing the boundary plane
            GenerateARMeshBoundaryPlanes();
        }

        // Generate ceiling plane to encapsulate the ARMesh
        private void GenerateARMeshBoundaryPlanes()
        {
            // Disable current planes in case they are enabled
            //Assuming parent is the parent game object
            DisableBoundaryPlanes();
            // Get planes and set values
            if (_ymaxPlane)
            {
                // Pooling plane
                GameObject plane = ObjectPool.SharedInstace.GetPooledObject(_planeTag);
                // Setting it as son of current gameObject
                plane.transform.SetParent(gameObject.transform, false);
                // Setting position, scale and rotation
                plane.transform.position = new Vector3(
                    (_meshBounds[0] + _meshBounds[1]) / 2, // Middle point of xMax and xMin
                    Math.Max(_meshBounds[2] + 0.01f, _meshBounds[3] + _minimumDistanceOppositeSide), // Minimum height from yMin
                    (_meshBounds[4] + _meshBounds[5]) / 2); // Middle point of zMax and zMin 
                // Setting size to cover ARMesh
                plane.transform.localScale = new Vector3((_meshBounds[0] - _meshBounds[1]), 1, (_meshBounds[4] - _meshBounds[5]));
                // Rotate 180 as it is the ceiling
                plane.transform.rotation = Quaternion.Euler(0, 0, 180);
                // Activating plane
                plane.SetActive(true);
                // Once the plane is active, we can save the bounds of its collider too. Otherwise the bounds will have a size of 0!!!
                _boundaryPlanesBounds.Add(plane.GetComponent<BoxCollider>().bounds);
            }
            if (_yminPlane)
            {
                // Pooling plane
                GameObject plane = ObjectPool.SharedInstace.GetPooledObject(_planeTag);
                // Setting it as son of current gameObject
                plane.transform.SetParent(gameObject.transform, false);
                // Setting position, scale and rotation
                plane.transform.position = new Vector3(
                    (_meshBounds[0] + _meshBounds[1]) / 2, // Middle point of xMax and xMin
                    Math.Min(_meshBounds[3] - 0.01f, _meshBounds[2] - _minimumDistanceOppositeSide), // Maximum height from yMax
                    (_meshBounds[4] + _meshBounds[5]) / 2); // Middle point of zMax and zMin 
                // Setting size to cover ARMesh
                plane.transform.localScale = new Vector3((_meshBounds[0] - _meshBounds[1]), 1, (_meshBounds[4] - _meshBounds[5]));
                // Activating plane
                plane.SetActive(true);
                // Once the plane is active, we can save the bounds of its collider too. Otherwise the bounds will have a size of 0!!!
                _boundaryPlanesBounds.Add(plane.GetComponent<BoxCollider>().bounds);
            }
            if (_xmaxPlane)
            {
                // Pooling plane
                GameObject plane = ObjectPool.SharedInstace.GetPooledObject(_planeTag);
                // Setting it as son of current gameObject
                plane.transform.SetParent(gameObject.transform, false);
                // Setting position, scale and rotation

                plane.transform.position = new Vector3(
                    Math.Max(_meshBounds[0] + 0.01f, _meshBounds[1] + _minimumDistanceOppositeSide), 
                    (_meshBounds[2] + _meshBounds[3]) / 2, // Middle point of yMax and yMin
                    (_meshBounds[4] + _meshBounds[5]) / 2); // Middle point of zMax and zMin 
                // Setting size to cover ARMesh
                plane.transform.localScale = new Vector3((_meshBounds[2] - _meshBounds[3]), 1, (_meshBounds[4] - _meshBounds[5]));
                plane.transform.rotation = Quaternion.Euler(0, 0, 90); // Rotate 90
                // Activating plane
                plane.SetActive(true);
                // Once the plane is active, we can save the bounds of its collider too. Otherwise the bounds will have a size of 0!!!
                _boundaryPlanesBounds.Add(plane.GetComponent<BoxCollider>().bounds);
            }
            if (_xminPlane)
            {
                // Pooling plane
                GameObject plane = ObjectPool.SharedInstace.GetPooledObject(_planeTag);
                // Setting it as son of current gameObject
                plane.transform.SetParent(gameObject.transform, false);
                // Setting position, scale and rotation

                plane.transform.position = new Vector3(
                    Math.Min(_meshBounds[1] - 0.01f, _meshBounds[0] - _minimumDistanceOppositeSide),
                    (_meshBounds[2] + _meshBounds[3]) / 2, // Middle point of yMax and yMin
                    (_meshBounds[4] + _meshBounds[5]) / 2); // Middle point of zMax and zMin 
                // Setting size to cover ARMesh
                plane.transform.localScale = new Vector3((_meshBounds[2] - _meshBounds[3]), 1, (_meshBounds[4] - _meshBounds[5]));
                plane.transform.rotation = Quaternion.Euler(0, 0, -90); // Rotate -90
                // Activating plane
                plane.SetActive(true);
                // Once the plane is active, we can save the bounds of its collider too. Otherwise the bounds will have a size of 0!!!
                _boundaryPlanesBounds.Add(plane.GetComponent<BoxCollider>().bounds);
            }
            if (_zmaxPlane)
            {
                // Pooling plane
                GameObject plane = ObjectPool.SharedInstace.GetPooledObject(_planeTag);
                // Setting it as son of current gameObject
                plane.transform.SetParent(gameObject.transform, false);
                // Setting position, scale and rotation

                plane.transform.position = new Vector3(
                    (_meshBounds[0] + _meshBounds[1]) / 2, // Middle point of xMax and xMin
                    (_meshBounds[2] + _meshBounds[3]) / 2, // Middle point of yMax and yMin
                    Math.Max(_meshBounds[4] + 0.01f, _meshBounds[5] + _minimumDistanceOppositeSide));
                // Setting size to cover ARMesh
                plane.transform.localScale = new Vector3((_meshBounds[0] - _meshBounds[1]), 1, (_meshBounds[2] - _meshBounds[3]));
                plane.transform.rotation = Quaternion.Euler(-90, 0, 0); // Rotate -90
                // Activating plane
                plane.SetActive(true);
                // Once the plane is active, we can save the bounds of its collider too. Otherwise the bounds will have a size of 0!!!
                _boundaryPlanesBounds.Add(plane.GetComponent<BoxCollider>().bounds);
            }
            if (_zminPlane)
            {
                // Pooling plane
                GameObject plane = ObjectPool.SharedInstace.GetPooledObject(_planeTag);
                // Setting it as son of current gameObject
                plane.transform.SetParent(gameObject.transform, false);
                // Setting position, scale and rotation

                plane.transform.position = new Vector3(
                    (_meshBounds[0] + _meshBounds[1]) / 2, // Middle point of xMax and xMin
                    (_meshBounds[2] + _meshBounds[3]) / 2, // Middle point of yMax and yMin
                    Math.Min(_meshBounds[5] - 0.01f, _meshBounds[4] - _minimumDistanceOppositeSide));
                // Setting size to cover ARMesh
                plane.transform.localScale = new Vector3((_meshBounds[0] - _meshBounds[1]), 1, (_meshBounds[2] - _meshBounds[3]));
                plane.transform.rotation = Quaternion.Euler(90, 0, 0); // Rotate -90
                // Activating plane
                plane.SetActive(true);
                // Once the plane is active, we can save the bounds of its collider too. Otherwise the bounds will have a size of 0!!!
                _boundaryPlanesBounds.Add(plane.GetComponent<BoxCollider>().bounds);
            }
        }

        // Get mesh limits in each axis. 
        // These values will be used to generate planes that
        private void GetARMeshBounds(GameObject arMesh)
        {
            var BoundLimits = 99999;
            float[] boundsArray = {-BoundLimits, BoundLimits, -BoundLimits, BoundLimits, -BoundLimits, BoundLimits};
            foreach (Transform child in arMesh.transform)
            {
                if (child.GetComponent<MeshFilter>().sharedMesh.vertices.Count() == 0) // It shouldn't happen but I've seen some cases
                    continue; // jump to the next iteration of the loop
                var xMax = child.GetComponent<MeshFilter>().sharedMesh.vertices.Max(t => arMesh.transform.TransformPoint(t).x);
                boundsArray[0] = xMax > boundsArray[0] ? xMax : boundsArray[0];
                var xMin = child.GetComponent<MeshFilter>().sharedMesh.vertices.Min(t => arMesh.transform.TransformPoint(t).x);
                boundsArray[1] = xMin < boundsArray[1] ? xMin : boundsArray[1];
                var yMax = child.GetComponent<MeshFilter>().sharedMesh.vertices.Max(t => arMesh.transform.TransformPoint(t).y);
                boundsArray[2] = yMax > boundsArray[2] ? yMax : boundsArray[2];
                var yMin = child.GetComponent<MeshFilter>().sharedMesh.vertices.Min(t => arMesh.transform.TransformPoint(t).y);
                boundsArray[3] = yMin < boundsArray[3] ? yMin : boundsArray[3];
                var zMax = child.GetComponent<MeshFilter>().sharedMesh.vertices.Max(t => arMesh.transform.TransformPoint(t).z);
                boundsArray[4] = zMax > boundsArray[4] ? zMax : boundsArray[4];
                var zMin = child.GetComponent<MeshFilter>().sharedMesh.vertices.Min(t => arMesh.transform.TransformPoint(t).z);
                boundsArray[5] = zMin < boundsArray[5] ? zMin : boundsArray[5];
            }
            _meshBounds = boundsArray;

        }

        public void DisableBoundaryPlanes()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i).gameObject;
                if (child != null)
                    child.SetActive(false);
            }
        }

    }
}
