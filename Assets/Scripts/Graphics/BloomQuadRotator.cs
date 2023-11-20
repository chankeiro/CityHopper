using UnityEngine;

namespace Bercetech.Games.Fleepas
{ 
    [ExecuteAlways] // This will execute it in Scene window too 
    public class BloomQuadRotator : MonoBehaviour
    {
        private Vector3 _cameraPos;
        private Vector3 _yOrientation;
        private Vector3 _lookDir;
        private Quaternion _lookR;
        private float _yOffset = 180;
        //This is useful for elements that are not spherical
        [SerializeField]
        private bool _useConfiguredScale;
        // This is the object that will be used to orient the quad
        // in case we need to scale it
        [SerializeField]
        private GameObject _referenceObjectForOrientation;
        [SerializeField]
        // This is the longitudinal direction of the reference object
        // which will correspond with the _yScale dimension
        private Vector3 _yDimensionInReferenceObject;
        [SerializeField]
        private float _xScale = 1f;
        [SerializeField]
        private float _yScale = 1f;

        private void OnWillRenderObject()
        {
            _cameraPos = Camera.current.transform.position;
            _lookDir = (_cameraPos - transform.position).normalized;
            if (_useConfiguredScale)
            {
                _yOrientation = _referenceObjectForOrientation.transform.rotation * _yDimensionInReferenceObject;
                // Change the shape depending on the looking angle. This is useful for elements that are not spherical
                transform.localScale = new Vector3(_xScale,
                    (_yScale - _xScale) * Vector3.Cross(_lookDir, _yOrientation).magnitude + _xScale,
                    1f);
                // Rotate looking at the _lookDir and using as upward vector the forward vector of the father
                // this is useful for elements that are not spherical
            }
            _lookR = Quaternion.LookRotation(_lookDir, Camera.current.transform.up);
            ////The yOffset value is degrees added to the rotation of the object to face the camera;
            ////this is needed because the forward vector of quads corresponds to its backface.
            ////That means if a quad is facing you, if backface culling is on(which it is by default)
            ////you won't even see it! In contrast, the forward vector of planes corresponds to its
            ////front face. Why it is like this I have no idea.
            transform.rotation = _lookR * Quaternion.Euler(0, _yOffset, 0);

            //transform.LookAt(_cameraPos);

        }

    }

}
