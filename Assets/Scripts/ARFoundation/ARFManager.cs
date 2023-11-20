using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.Management;



namespace Bercetech.Games.Fleepas
{
    public class ARFManager : MonoBehaviour
    {
        
        [SerializeField]
        private ARSession _arfSession;
        [SerializeField]
        private ARCoreExtensions _arCoreExtensions;
        [SerializeField]
        private ARCameraManager _arfCameraManager;
        [SerializeField]
        private ARCameraBackground _arfCameraBackground;
        [SerializeField]
        private TrackedPoseDriver _trackedPoseDriver;
        [SerializeField]
        private ARInputManager _arInputManager;
        [SerializeField]
        private ARAnchorManager _arAnchorManager;
        [SerializeField]
        private PersistentCloudAnchorsManager _cloudAnchorManager;
        [SerializeField]
        private ARMeshManager _arMeshManager;
        [SerializeField]
        private AROcclusionManager _arOcclusionManager;


        private Signal _meshingRestarted = new Signal();
        public Signal MeshingRestarted => _meshingRestarted;

        // Defining a static shared instance variable so other scripts can access to the object
        private static ARFManager _sharedInstance;
        public static ARFManager SharedInstance => _sharedInstance;

        private void Awake()
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

        public void EnableARFoundation()
        {

            // Enabling ARFoundation and rest of necessary managers (exactly in this order)
            // XR Management controls the lifecycle of subsystems. Components in AR Foundation,
            // such as ARSession or ARPlaneManager, turn subsystems on and off, but do not create
            // or destroy them. Therefore, subsystems can persist across many scenes. They are
            // automatically created on app startup, but are not destroyed during a scene switch.
            // This allows you to keep the same session alive between scenes, for example
            // But in case we need to reset some elements, as cloud anchors between sessions, we
            // need to deinitilize the current XR Loader and initialize it after loading the
            // new session (I am not sure if it is really necessary, at least not in my experience
            // but reseting anyway to clean any AR Component that might be duplicated)
            //if (!XRGeneralSettings.Instance.Manager.activeLoader)
            //{
            //    XRGeneralSettings.Instance.Manager.InitializeLoaderSync(); //This creates all subsystems.
            //    XRGeneralSettings.Instance.Manager.StartSubsystems();
            //}
            // Reseting session to check if prevents some black screen when reloading, and some SIGSEGV crashes. 
            // Need to do a follow-up. However. When introducing a new AR element or manager, it is better to test
            // without reseting, to ensure that we are cleaning the scene when unloading Unity, without needing to reset it.
            // It is not really necessary but it gives me peace of mind and I think it is safer
            _arfSession.Reset();
            // Theoretically is equivalent to LoaderUtility.Initialize()/Deinitialize()
            _arfCameraManager.enabled = true;           
            _arfCameraBackground.enabled = true;
            _arfSession.enabled = true;
            // Enable ARCore Extensions after enabling ARSession. Also before anchor manager,
            // otherwise, the feature quality estimation in cloud anchor might not work correctly
            // in some cases
#if !UNITY_EDITOR
            _arCoreExtensions.enabled = true; 
#endif
            _trackedPoseDriver.enabled = true;
            _arInputManager.enabled = true;
#if !UNITY_EDITOR
            _arAnchorManager.enabled = true; 
#endif
        }

        public void DisableARFoundation()
        {
            if (_arfSession.enabled)
            {
                // Not sure about the order of these
                _arInputManager.enabled = false;
                _trackedPoseDriver.enabled = false;
                _arCoreExtensions.enabled = false;
                _arAnchorManager.enabled = false;
                // Need to remove all the trackables. AR Cloud Anchors are also included here. If we
                // don't remove them, in case the user reloads a scene with cloud anchors that were resolved
                // previously in other session, the anchors will resolved automatically, but in reality
                // they won't be properly located
                foreach (var anchor in _arAnchorManager.trackables)
                    Destroy(anchor);
                _cloudAnchorManager.enabled = false; 
                _arMeshManager.enabled = false;
                _arMeshManager.DestroyAllMeshes(); // Not really needed because, usually we reload the scene
                                                   // and this meshes are destroyed when that happens, but just in case we don't
                _arOcclusionManager.enabled = false;
                _arfSession.enabled = false;
                _arfCameraBackground.enabled = false;
                _arfCameraManager.enabled = false;
                //_arfSession.Reset(); // It doesn't seem to crash ARCore but sometimes (after playing for around 5 min) it kills the process
                // and the app restarts. Avoid using it, better try to reset all managers.
                //XRGeneralSettings.Instance.Manager.StopSubsystems();
                //XRGeneralSettings.Instance.Manager.DeinitializeLoader(); // This destroys all subsystems
                //Theoretically is equivalent to LoaderUtility.Deinitialize(). Using it crashes ARCore sometimes
            }
        }

        public void EnableMeshing()
        {
            _arMeshManager.enabled = true;

            // This manager provides the depth. Necessary for meshing with Lighthip, but not in Editor. 
#if !UNITY_EDITOR
            _arOcclusionManager.enabled = true; 
#else
            _arfSession.Reset(); // However, in Editor we need to reset the ARSession for the meshing to work, don't know why
#endif
        }

        public void DisableMeshing()
        {
            // It is enough with just disabling the managers, but I am not sure if 
            // stopping the susbsystem helps with the smoothnes of the camera video. Sometimes it looks
            // like it might, but it might be just that the phone is overloaded with other tasks. Need to
            // test it more, but keeping it for now, because it doesn't have any other impact. In case of 
            // reloading the fleep, the subsystem will be restarted once the managers are reenabled
            if (_arMeshManager.subsystem != null)
            {
                _arMeshManager.enabled = false;
                _arMeshManager.subsystem.Stop();
            }
            if (_arOcclusionManager.subsystem != null)
            {
                _arOcclusionManager.enabled = false;
                _arOcclusionManager.subsystem.Stop();
            }

            //_arMeshManager.enabled = false;
            //_arOcclusionManager.enabled = false;
        }


        public void ResetMeshing()
        {
            
            _arMeshManager.enabled = false;
            _arMeshManager.DestroyAllMeshes();
            _arMeshManager.enabled = true;
            _meshingRestarted.Fire();

        }

    }

}
