using UnityEngine;

namespace Bercetech.Games.Fleepas
{ 

    public class UIAttachedToCamera : MonoBehaviour
    {
        [SerializeField]
        private GameObject _attachedObject;
        // This vector might be necessary to displace the center
        // of the object to a position that will be more suited to
        // use as reference for the attached UI
        [SerializeField]
        private Vector3 _attrachedObjectLocalCenter;
        [SerializeField]
        private Vector3 _UIOffsetFromLocalCenter;



        private void Update()
        {
            transform.rotation = Camera.main.transform.rotation* Quaternion.Euler(0, 180, 0); ;
            transform.position = _attachedObject.transform.position + _attachedObject.transform.TransformDirection(_attrachedObjectLocalCenter) + _UIOffsetFromLocalCenter;
        }

    }

}
