using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Google.XR.ARCoreExtensions;
using System;
using UniRx;
using System.IO;
using SevenZip.Compression.LZMA;
using System.Linq;
using UnityEngine.Localization;


namespace Bercetech.Games.Fleepas
{
    public class PersistentCloudAnchorsManager : MonoBehaviour
    {

        /// <summary>
        /// The 3D object that represents a Cloud Anchor.
        /// </summary>
        [SerializeField]
        private GameObject _cloudAnchorPrefab;

        /// <summary>
        /// The game object that includes <see cref="MapQualityIndicator"/> to visualize
        /// map quality result.
        /// </summary>
        [SerializeField]
        private GameObject _mapQualityIndicatorPrefab;

        [SerializeField]
        private Camera _arCamera;
        /// <summary>
        /// The active ARRaycastManager used in the example.
        /// </summary>
        [SerializeField]
        private ARRaycastManager _raycastManager;
        /// <summary>
        /// The active ARAnchorManager used in the example.
        /// </summary>
        [SerializeField]
        private ARAnchorManager _anchorManager;
        [SerializeField]
        private ARPlaneManager _planeManager;
        [SerializeField]
        private LayerMask _arMeshChunksLayerMask;
        [SerializeField]
        private GameObject _arMesh; // This is the mes scanned with ARDK
        [SerializeField]
        private MeshMaterialChanger _meshMaterialChanger; // This is the mes scanned with ARDK
        [SerializeField]
        private MeshBuilder _meshBuilder;
        private Pose _meshLocalPose;

        /// <summary>
        /// True if the app is in the process of returning to home page due to an invalid state,
        /// otherwise false.
        /// </summary>
        private bool _arSessionHasError;
        [SerializeField]
        private GameObject _arSessionErrorAlert;
        [SerializeField]
        private TextMeshProUGUI _arSessionErrorAlertText;
        [SerializeField]
        private TextMeshProUGUI _instructionText;
        [SerializeField]
        private GameObject _instructionTextLayout;
        [SerializeField]
        private GameObject _instructionTextAlert;
        [SerializeField]
        private GameObject _instructionTextSpacer;
        [SerializeField]
        private GameObject _processingAnchorMessage;
        private string _instructionTextInitialMessage;
        private int _pauseUpdateInstructionText = 0;
        /// <summary>
        /// Display the tracking helper text when the session in not tracking.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI _arCoreTrackingAlertText;
        [SerializeField]
        private GameObject _networkErrorMessage;
        private bool _networkCoroutineRunning = false;

        /// <summary>
        /// The UI panel that allows the user to name the Cloud Anchor.
        /// </summary>
        [SerializeField]
        private GameObject _cloudAnchorDataPanel;
        [SerializeField]
        private GameObject _cloudAnchorScreenShots;
        [SerializeField]
        private GameObject _fleepSiteCreationWaitMessage;
        [SerializeField]
        private GameObject _fleepSiteCreationSuccess;
        [SerializeField]
        private GameObject _fleepSiteCreationFailure;
        [SerializeField]
        private TextMeshProUGUI _fleepSiteCreationFailureText;
        [SerializeField]
        private GameObject _fleepSiteGetDataFailure;
        [SerializeField]
        private TextMeshProUGUI _fleepSiteGetDataFailureText;
        private Signal _fleepSiteGetDataFinished = new();
        [SerializeField]
        private GameObject _cancelAndPlayButton;
        [SerializeField]
        private GameObject _singlePlayerStartGame;
        [SerializeField]
        private RawImage _fleepSiteImage;
        [SerializeField]
        private HideMenuTitle _hideMenuTitle;
        private int _screenShotId;
        public int ScreenShotId => _screenShotId;
        [SerializeField]
        private ImageCarrouselLoader _screenShotCarrousel;
        [SerializeField]
        private Texture _alertTexture;
        private bool _canHideTitle = false;
        /// <summary>
        /// The UI element that displays warning message for invalid input name.
        /// </summary>
        [SerializeField]
        private GameObject _incorrectCharacterWarning;
        [SerializeField]
        private GameObject _shortNameWarning;
        [SerializeField]
        private GameObject _longNameWarning;

        /// <summary>
        /// The input field for naming Cloud Anchor.
        /// </summary>
        [SerializeField]
        private TMP_InputField _cloudAnchorNameField;
        /// <summary>
        /// Public or Private FleepSite
        /// </summary>
        [SerializeField]
        private Slider _isPublicFleepSite;
        /// <summary>
        /// FleepSite Expiration Date
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI _expirationDateText;
        [SerializeField]
        private GameObject _longMappingWarning;
        [SerializeField]
        private int _longMappingWarningTime;

        /// <summary>
        /// The button to save the typed name.
        /// </summary>
        [SerializeField]
        private Button _saveCloudAnchorButton;

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.Relocalizing">.</see>
        /// </summary>
        private string _relocalizingMessage;

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.InsufficientLight">.</see>
        /// </summary>
        private string _insufficientLightMessage;

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.InsufficientLight">
        /// in Android S or above.</see>
        /// </summary>
        private string _insufficientLightMessageAndroidS;

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.InsufficientFeatures">.</see>
        /// </summary>
        private string _insufficientFeatureMessage;

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.ExcessiveMotion">.</see>
        /// </summary>
        private string _excessiveMotionMessage;

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.Unsupported">.</see>
        /// </summary>
        private string _unsupportedMessage;



        /// <summary>
        /// Android 12 (S) SDK version.
        /// </summary>
        private const int _androidSSDKVesion = 31;
        /// <summary>
        /// The timer to indicate whether the AR View has passed the start prepare time.
        /// </summary>
        private float _timeSinceStart;
        /// <summary>
        /// The time between this script is enable and ARCore session starts to host or resolve.
        /// </summary>
        private float _startPrepareTime;

        /// <summary>
        /// The MapQualityIndicator that attaches to the placed object.
        /// </summary>
        private MapQualityIndicator _qualityIndicator = null;


        // An ARAnchor indicating the 3D object has been placed on a flat surface and is waiting for hosting.
        // This is not an ARCloudAnchor, but a regular ARAnchor
        private ARAnchor _pendingAnchor = null;

        // This is the promise that we get after calling HostCloudAnchorAsync
        private HostCloudAnchorPromise _hostCloudAnchorPromise;
        // A list of Cloud Anchors Promises that have been created but are not yet ready to use
        // Their current state is PromiseState.Pending
        private List<HostCloudAnchorPromise> _pendingHostCloudAnchorsPromises = new List<HostCloudAnchorPromise>();
        // This is the HostCloudAnchorResult that we get from the hosting promise once it is in state Done
        private HostCloudAnchorResult _hostCloudAnchor = null;

        // This is the promise that we get after calling ResolveCloudAnchorAsync
        private ResolveCloudAnchorPromise _resolveCloudAnchorPromise;
        // A list of Resolve Cloud Anchors Promises that have been created but are not yet ready to use
        // Their current state is PromiseState.Pending. We also need the ClouchAnchorId, which is neither
        // available in the promise nor in the result, but we need it in several functions. That's why we use
        // a Tuple here
        private List<Tuple<string,ResolveCloudAnchorPromise>> _pendingResolveCloudAnchorsPromises = new List<Tuple<string, ResolveCloudAnchorPromise>>();
        // This is the ResolveCloudAnchorResult that we get from the resolving promise once it is in state Done
        private ResolveCloudAnchorResult _resolveCloudAnchor = null;

        private List<string> _cloudAnchorsIdToResolve = new List<string>();
        private List<Tuple<string, ARCloudAnchor>> _cloudAnchorsResolved = new List<Tuple<string, ARCloudAnchor>>();
        private Signal<Tuple<string, ARCloudAnchor>> _cloudAnchorIsResolved = new();
        private List<FleepasCloudMesh> _cloudMeshList = new List<FleepasCloudMesh>();
        [SerializeField]
        private int _cloudAnchorLifetime = 365;

        private Signal _mappingFinished = new();
        private AndroidJavaClass _versionInfo;

        // CompositeDisposable is similar with List<IDisposable>
        // It will be used to gather all disposables active after the game is finished
        // so they can be disposed at that moment
        protected CompositeDisposable disposables = new CompositeDisposable();
        private void OnDestroy()
        {
            disposables.Clear();
        }

        // Defining a static shared instance variable so other scripts can access to the object pool
        private static PersistentCloudAnchorsManager _sharedInstance;
        public static PersistentCloudAnchorsManager SharedInstance => _sharedInstance;

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
            // Checking the android version. It will be used for the tracking instructions
            _versionInfo = new AndroidJavaClass("android.os.Build$VERSION");
        }

        public void Start()
        {
            // Subscribe to cloud anchor resolved event
            _cloudAnchorIsResolved.Subscribe(cloudAnchor =>
            {
                // Check if exists an element that depends on that cloud anchor
                var cloudMesh = _cloudMeshList.Where(mesh => mesh.CloudAnchorId == cloudAnchor.Item1).FirstOrDefault();
                if (cloudMesh != null)
                    RebuildCloudMesh(_arMesh, cloudMesh.MeshData, cloudMesh.MeshPose, cloudAnchor.Item2);

            }).AddTo(disposables);

            // Initializing some messages
            _relocalizingMessage = new LocalizedString("000 - Fleepas", "fleepSite_relocalizing").GetLocalizedString();
            _insufficientLightMessage = new LocalizedString("000 - Fleepas", "fleepSite_dark").GetLocalizedString();
            _insufficientLightMessageAndroidS = new LocalizedString("000 - Fleepas", "fleepSite_no_light").GetLocalizedString();
            _insufficientFeatureMessage = new LocalizedString("000 - Fleepas", "fleepSite_no_feature").GetLocalizedString();
            _excessiveMotionMessage = new LocalizedString("000 - Fleepas", "fleepSite_fast_motion").GetLocalizedString();
            _unsupportedMessage = new LocalizedString("000 - Fleepas", "fleepSite_tracking_lost").GetLocalizedString();

    }

        public void OnEnable()
        {
            // Starting a count down that will be use to give ARCore some time to prepare
            _timeSinceStart = 0.0f;
            _arSessionHasError = false;
            _pendingAnchor = null;
            _qualityIndicator = null;
            _pendingHostCloudAnchorsPromises.Clear();
            _pendingResolveCloudAnchorsPromises.Clear();

            // Hiding screens 
            _processingAnchorMessage.SetActive(false);
            _cloudAnchorScreenShots.SetActive(false);
            _cloudAnchorDataPanel.SetActive(false);
            _incorrectCharacterWarning.SetActive(false);
            _shortNameWarning.SetActive(false);
            _longNameWarning.SetActive(false);
            // Hiding messages
            _instructionTextLayout.SetActive(false);
            _arCoreTrackingAlertText.gameObject.transform.parent.gameObject.SetActive(false);
            _fleepSiteImage.transform.parent.gameObject.SetActive(false);
            // Looking for planes in the scene, that will be used to put the cloud anchor
            //UpdatePlaneVisibility(true);
            // Activating Name Warnings
            OnInputFieldValueChanged();


            // Setting Instrunction test depending on the mode
            if (CloudAnchorsModeHolder.SharedInstance.CloudAnchorMode == CloudAnchorsModeHolder.CloudAnchorsMode.Hosting)
            {
                
                // Show play button
                _cancelAndPlayButton.SetActive(true);
                // Show instructions
                _instructionTextInitialMessage = new LocalizedString("000 - Fleepas", "fleepSite_initial_message").GetLocalizedString();
                ModifyInstructionText(_instructionTextInitialMessage, false);
                _instructionTextLayout.SetActive(true);
                // Set Mesh as invisible
                _meshMaterialChanger.SetMateriaOnARMesh(MeshMaterialChanger.ARMeshMaterialType.Invisible);
                // Time to run start tracking
                _startPrepareTime = 0;
            }
            else if (CloudAnchorsModeHolder.SharedInstance.CloudAnchorMode == CloudAnchorsModeHolder.CloudAnchorsMode.Resolving)
            {
                // Ask for FleepSite data to the native activity
                MainActivityMessagingManager.GetFleepSiteData(PlayerMode.SharedInstance.FleepSiteId);


                // Hide title and show first fleepsite image when data reception is finished AND a few seconds have passed
                // Wait a few seconds before hiding the title screen
                Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(_ =>
                {
                    if (_canHideTitle)
                        ShowFirstFleepSiteImage();
                    else
                        _canHideTitle = true;
                }).AddTo(disposables);

                // Subscribe to finished fleepsite data reception
                _fleepSiteGetDataFinished.Subscribe(_ =>
                {
                    if (_canHideTitle)
                        ShowFirstFleepSiteImage();
                    else
                        _canHideTitle = true;
                }).AddTo(disposables);

                // Time to run start tracking
                _startPrepareTime = 3;
            }
        }

        private void ShowFirstFleepSiteImage()
        {
            _hideMenuTitle.HideTitle();
            _fleepSiteImage.transform.parent.gameObject.SetActive(true);
        }

        public void Update()
        {
            // Give ARCore some time to prepare 
            if (_timeSinceStart < _startPrepareTime)
            {
                _timeSinceStart += Time.deltaTime;
                return;
            }

            // Check if there is some error with the ARCore Session
            ARCoreLifecycleUpdate();
            // Not following in case an error happens
            if (_arSessionHasError)
            {
                return;
            }
            // Display tracking helper message if necessary
            DisplayTrackingHelperMessage();
            if (CloudAnchorsModeHolder.SharedInstance.CloudAnchorMode == CloudAnchorsModeHolder.CloudAnchorsMode.Resolving)
            {
                ResolvingCloudAnchors();
            }
            else if (CloudAnchorsModeHolder.SharedInstance.CloudAnchorMode == CloudAnchorsModeHolder.CloudAnchorsMode.Hosting)
            {
                // Perform hit test and place an anchor on the hit test result
                // The player must touch the screen. Ignore the touch if it's pointing on UI objects.
                // The hosting process won't start till an anchor is defined
                if (Input.touchCount > 0
                    // Stop if any of the screens from a previous anchor saving process are opened. In such case, the user must
                    // cancel the screen to create a new anchor
                    & !_processingAnchorMessage.activeSelf & !_cloudAnchorDataPanel.activeSelf & !_cloudAnchorScreenShots.activeSelf 
                    & !_fleepSiteCreationWaitMessage.activeSelf & !_fleepSiteCreationSuccess.activeSelf & !_fleepSiteCreationFailure.activeSelf) 
    
                {
                    Touch touch;
                    if ((touch = Input.GetTouch(0)).phase == TouchPhase.Began & !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        // Perform hit test and place a pawn object.
                        //PerformHitTestOnPlane(touch.position);
                        PerformHitTestOnMesh(touch);
                    }
                }

                HostingCloudAnchor();
            }
            // To run these functions _pendingCloudAnchors must have at least an element,
            // so it only runs when an anchor is hosted or resolved. At the same time
            // HostingCloudAnchor /ResolvingCloudAnchor stop running when that happens
            UpdatePendingCloudAnchors();
            CheckNetwork();
        }



        private void PerformHitTestOnMesh(Touch touch)
        {

            // Calculating "ray" that goes from camera to mesh hitpoint
            GameObject ray = GetRayToHitPoint(touch);

            // Applying raycast with ray direction
            RaycastHit hitResult;
            var raycast = Physics.Raycast(ray.transform.position, ray.transform.up, out hitResult, 100, _arMeshChunksLayerMask, QueryTriggerInteraction.Collide);
            // Successful raycast returns true
            if (raycast)
            {
                // If there is already a pending anchor defined previously to set it here,
                // we must reset it
                if (_pendingAnchor != null)
                {
                    // Anchor and quality prefabs are childs that are also destroyed with the anchor
                    _pendingHostCloudAnchorsPromises.Clear();
                    Destroy(_pendingAnchor.gameObject);
                    _pendingAnchor = null;
                    // And we also cancel the ScreenShot capture process that was running with
                    // the previous pending anchor
                    _mappingFinished.Fire();
                }

                Logging.Omigari("Instantiating anchor");
                var cloudAnchor = Instantiate(_cloudAnchorPrefab,
                    hitResult.point + hitResult.normal *0.05f, // Positioning anchor a bit out of the mesh surface
                    Quaternion.LookRotation(new Vector3(ray.transform.up.x, 0, ray.transform.up.z))); // Looking at the camera, but in an horizontal plane
                cloudAnchor.AddComponent<ARAnchor>();
                _pendingAnchor = cloudAnchor.GetComponent<ARAnchor>();
                // We don't need the ray data anymore
                Destroy(ray);
            } else
            {
                // Inform the user that it doesn't exist mesh on the clicked area,
                // and come back to the current instruction message after a couple of seconds
                // or initial message in case there is not anchor yet
                _pauseUpdateInstructionText += 1; // This variable must come back to 0. If a user 
                // clicks on several empty areas, it will keep increasing, until all the timers are 
                // finished.
                ModifyInstructionText(
                    new LocalizedString("000 - Fleepas", "fleepSite_location_not_scanned").GetLocalizedString(),
                    true
                ); 
                Observable.Timer(TimeSpan.FromSeconds(3)).TakeUntilDisable(gameObject).Subscribe(_ =>
                {
                    _pauseUpdateInstructionText -= 1;
                    // Show initial message in case there is no anchor
                    if (_pauseUpdateInstructionText == 0 & _pendingAnchor == null)
                        ModifyInstructionText(_instructionTextInitialMessage, false);
                    
                }).AddTo(disposables);
            }

            if (_pendingAnchor != null)
            {
                Logging.Omigari("Attaching quality map to anchor");
                // Attach map quality indicator to this anchor.
                var indicatorGO = Instantiate(_mapQualityIndicatorPrefab, _pendingAnchor.transform);
                _qualityIndicator = indicatorGO.GetComponent<MapQualityIndicator>();
                _qualityIndicator.DrawIndicator(PlaneAlignment.HorizontalUp, _arCamera);

                Logging.Omigari("Waiting for sufficient mapping quaility...");
                // Start saving screenshots of the anchor every 0.1 map quality increments
                _screenShotId = 1;
                float mappingQuality = 0;
                Observable.Interval(TimeSpan.FromSeconds(1)).StartWith(0)
                    .TakeUntil(_mappingFinished).TakeUntilDisable(gameObject)
                    .Subscribe(_ =>
                {
                    // Maximum of 5 screenshots
                    if (_qualityIndicator.GetCurrentQuality >= mappingQuality && _screenShotId <= 5)
                    {
                        StartCoroutine(ScreenShotSaver.SharedInstance.SaveScreenShot(_screenShotId == 1));
                        _screenShotCarrousel.SetMaximumScreenShotId(_screenShotId);
                        _screenShotId += 1;
                        mappingQuality += 0.1f;
                    }

                }).AddTo(disposables);

                // Start the timer to show a warning in case the mapping process takes too long
                LaunchLongMappingTimer();
                // Hide plane generator so users can focus on the object they placed.
                //UpdatePlaneVisibility(false);
            }
        }

        private void PerformHitTestOnPlane(Vector2 touchPos)
        {
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            _raycastManager.Raycast(touchPos, hitResults, TrackableType.PlaneWithinPolygon);

            // If there was an anchor placed, then instantiate the corresponding object.
            var planeType = PlaneAlignment.HorizontalUp;
            if (hitResults.Count > 0)
            {
                ARPlane plane = _planeManager.GetPlane(hitResults[0].trackableId);
                if (plane == null)
                {
                    Logging.OmigariFormat("Failed to find the ARPlane with TrackableId {0}",
                        hitResults[0].trackableId);
                    return;
                }
                planeType = plane.alignment;
                var hitPose = hitResults[0].pose;
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    // Point the hitPose rotation roughly away from the raycast/camera
                    // to match ARCore.
                    hitPose.rotation.eulerAngles =
                        new Vector3(0.0f, _arCamera.transform.eulerAngles.y, 0.0f);
                }

                // If there is already a pending anchor defined previously to set it here,
                // we must reset it
                if (_pendingAnchor != null)
                {
                    // Anchor and quality prefabs are childs that are also destroyed with the anchor
                    Destroy(_pendingAnchor.gameObject); 
                    _pendingAnchor = null;
                }
                Logging.Omigari("Anchor attached");
                _pendingAnchor = _anchorManager.AttachAnchor(plane, hitPose);

            }

            if (_pendingAnchor != null)
            {
                Logging.Omigari("Instantiating anchor");
                // Instantiate anchor prefab
                Instantiate(_cloudAnchorPrefab, _pendingAnchor.transform);
                // Attach map quality indicator to this anchor.
                var indicatorGO = Instantiate(_mapQualityIndicatorPrefab, _pendingAnchor.transform);
                _qualityIndicator = indicatorGO.GetComponent<MapQualityIndicator>();
                _qualityIndicator.DrawIndicator(planeType, _arCamera);
                Logging.Omigari("Waiting for sufficient mapping quaility...");

                // Hide plane generator so users can focus on the object they placed.
                //UpdatePlaneVisibility(false);
            }
        }

        private void HostingCloudAnchor()
        {
            // There is no anchor to be hosted
            if (_pendingAnchor == null)
            {
                return;
            }

            // There is a pending or finished hosting task.
            if (_pendingHostCloudAnchorsPromises.Count > 0)
            {
                return;
            }
            // Can pass in ANY valid camera pose to the mapping quality API.
            // Ideally, the pose should represent users’ expected perspectives.
            FeatureMapQuality quality =_anchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
            Logging.Omigari("Current mapping quality: " + quality);
            // Updating qualiy indicator state
            _qualityIndicator.UpdateQualityState((int)quality);
            // Hosting instructions:
            var cameraDist = (_qualityIndicator.transform.position -
                _arCamera.transform.position).magnitude;
            if (_pauseUpdateInstructionText > 0 && !_qualityIndicator.ReachQualityThreshold)
                return;
            if (cameraDist < _qualityIndicator.Radius * 1.5f)
            {
                ModifyInstructionText(
                    new LocalizedString("000 - Fleepas", "fleepSite_too_close").GetLocalizedString(),
                    true
                );
                return;
            }
            if (cameraDist > 10.0f)
            {
                ModifyInstructionText(
                    new LocalizedString("000 - Fleepas", "fleepSite_too_far").GetLocalizedString(),
                    true
                );
                return;
            }
            if (_qualityIndicator.ReachTopviewAngle)
            {
                ModifyInstructionText(
                    new LocalizedString("000 - Fleepas", "fleeSite_top_view").GetLocalizedString(),
                    true
                );
                return;
            }
            if (!_qualityIndicator.ReachQualityThreshold)
            {
                ModifyInstructionText(
                    new LocalizedString("000 - Fleepas", "fleepSite_basic_instructions").GetLocalizedString(),
                    false
                );
                return;
            }

            // Start hosting:
            _mappingFinished.Fire();
            _instructionTextLayout.SetActive(false);
            _processingAnchorMessage.SetActive(true);
            Logging.Omigari(string.Format("FeatureMapQuality has reached {0}, triggering CreateCloudAnchor.",
                _anchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose())));

            // Creating Cloud Anchor
            // This is configurable up to 365 days when keyless authentication is used.
            _hostCloudAnchorPromise = _anchorManager.HostCloudAnchorAsync(_pendingAnchor, _cloudAnchorLifetime);
            // HostCloudAnchor will return a null in case it is not able to generate the cloud anchor
            if (_hostCloudAnchorPromise == null)
            {
                Logging.OmigariFormat("Failed to create a Cloud Anchor.");
                OnAnchorHostedFinished(false, CloudAnchorState.ErrorInternal);
            }
            else
            {
                _pendingHostCloudAnchorsPromises.Add(_hostCloudAnchorPromise);
            }
        }

        private void ResolvingCloudAnchors()
        {
            // Need to feed this list to start resolving
            if (_cloudAnchorsIdToResolve.Count == 0)
            {
                return;
            }
            // There are pending resolving tasks.
            if (_pendingResolveCloudAnchorsPromises.Count > 0)
            {
                return;
            }
            // ARCore session is not ready for resolving.
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                return;
            }
            Logging.OmigariFormat("Attempting to resolve {0} Cloud Anchor(s): {1}", _cloudAnchorsIdToResolve.Count,
                string.Join(",", _cloudAnchorsIdToResolve.ToArray()));
            foreach (string cloudAnchorId in _cloudAnchorsIdToResolve)
            {
                // Remove it from the list
                _cloudAnchorsIdToResolve = _cloudAnchorsIdToResolve.Where(id => id != cloudAnchorId).ToList();
                _resolveCloudAnchorPromise = _anchorManager.ResolveCloudAnchorAsync(cloudAnchorId);
                if (_resolveCloudAnchorPromise == null)
                {
                    Logging.OmigariFormat("Failed to resolve Cloud Anchor " + cloudAnchorId);
                    OnAnchorResolvedFinished(false, cloudAnchorId, null);
                }
                else
                {
                    _pendingResolveCloudAnchorsPromises.Add(Tuple.Create(cloudAnchorId, _resolveCloudAnchorPromise));
                }
            }
        }



        private void UpdatePendingCloudAnchors()
        {

            if (CloudAnchorsModeHolder.SharedInstance.CloudAnchorMode == CloudAnchorsModeHolder.CloudAnchorsMode.Hosting)
            {
                foreach (var hostCloudAnchorPromise in _pendingHostCloudAnchorsPromises)
                {
                    if (hostCloudAnchorPromise.State != PromiseState.Pending)
                    {
                        _hostCloudAnchor = hostCloudAnchorPromise.Result;
                        if (_hostCloudAnchor.CloudAnchorState == CloudAnchorState.Success)
                        {
                            Logging.OmigariFormat("Succeed to host the Cloud Anchor: {0}.", _hostCloudAnchor.CloudAnchorId);
                            OnAnchorHostedFinished(true, _hostCloudAnchor.CloudAnchorState);
                            // Destroying pending anchor so the script doesn't try to host it again
                            // We have to wait until this moment to not destroy anchor and quality prefabs (childs of the pending anchor)
                            _meshLocalPose = _pendingAnchor.transform.InverseTransformPose(new Pose(_arMesh.transform.position, _arMesh.transform.rotation));
                            Destroy(_pendingAnchor.gameObject);
                            _pendingAnchor = null;
                        }
                        else
                        {
                            Logging.OmigariFormat("Failed to host the Cloud Anchor with error {0}.", _hostCloudAnchor.CloudAnchorState);
                            OnAnchorHostedFinished(false, _hostCloudAnchor.CloudAnchorState);
                            // Destroying pending anchor so the script doesn't try to host it again
                            // We have to wait until this moment to not destroy anchor and quality prefabs (childs of the pending anchor)
                            Destroy(_pendingAnchor.gameObject);
                            _pendingAnchor = null;
                        }
                    }
                }
                // Removing all anchors but those pending
                _pendingHostCloudAnchorsPromises.RemoveAll(x => x.State != PromiseState.Pending);
            }
            if (CloudAnchorsModeHolder.SharedInstance.CloudAnchorMode == CloudAnchorsModeHolder.CloudAnchorsMode.Resolving)
            {
                foreach (var resolveCloudAnchorPromise in _pendingResolveCloudAnchorsPromises)
                {
                    if (resolveCloudAnchorPromise.Item2.State != PromiseState.Pending)
                    {
                        _resolveCloudAnchor = resolveCloudAnchorPromise.Item2.Result;
                        if (_resolveCloudAnchor.CloudAnchorState == CloudAnchorState.Success)
                        {
                            Logging.OmigariFormat("Succeed to resolve the Cloud Anchor: {0}", _resolveCloudAnchor.Anchor);
                            OnAnchorResolvedFinished(true, resolveCloudAnchorPromise.Item1, _resolveCloudAnchor);
                        }
                        else
                        {
                            Logging.OmigariFormat("Failed to resolve the Cloud Anchor {0} with error {1}.", resolveCloudAnchorPromise.Item1, _resolveCloudAnchor.CloudAnchorState);
                            OnAnchorResolvedFinished(false, resolveCloudAnchorPromise.Item1, _resolveCloudAnchor);
                        }
                    }
                }
                // Removing all anchors but those pending
                _pendingResolveCloudAnchorsPromises.RemoveAll(x => x.Item2.State != PromiseState.Pending);

            }
        }

        private void OnAnchorHostedFinished(bool success, CloudAnchorState cloudAnchorState)
        {
            if (success)
            {
                // Display screenshots screen and hide instruction bar.
                _processingAnchorMessage.SetActive(false);
                _cloudAnchorNameField.text = "";
                SetSaveButtonActive(false);
                _cloudAnchorScreenShots.SetActive(true);
            }
            else
            {
                if (cloudAnchorState == CloudAnchorState.ErrorHostingDatasetProcessingFailed)
                    ModifyInstructionText(
                        new LocalizedString("000 - Fleepas", "fleepSite_error_hosting").GetLocalizedString(),
                        true
                    );
                else
                {
                    _arSessionErrorAlertText.text = new LocalizedString("000 - Fleepas", "fleepSite_error_hosting_2").GetLocalizedString();
                    _arSessionErrorAlert.SetActive(true);
                }
                    
            }
        }

        private void OnAnchorResolvedFinished(bool success, string cloudAnchorId, ResolveCloudAnchorResult cloudAnchor)
        {
            // Hide FleepSite Image
            _fleepSiteImage.transform.parent.gameObject.SetActive(false);

            if (success)
            {
                var cloudAnchorResolved = Tuple.Create(cloudAnchorId, cloudAnchor.Anchor);
                // Add to list of resolved cloud anchors
                _cloudAnchorsResolved.Add(cloudAnchorResolved);
                // Fire event to resolve potential elements using this cloud anchor as reference
                _cloudAnchorIsResolved.Fire(cloudAnchorResolved);
            }
            else
            {
                CloudAnchorState cloudAnchorState = CloudAnchorState.ErrorInternal;
                if (cloudAnchor != null)
                    cloudAnchorState = cloudAnchor.CloudAnchorState;
                if (cloudAnchorState == CloudAnchorState.ErrorResolvingCloudIdNotFound || cloudAnchorState == CloudAnchorState.None)
                {
                    _arSessionErrorAlertText.text = new LocalizedString("000 - Fleepas", "fleepSite_not_found").GetLocalizedString();
                    _arSessionErrorAlert.SetActive(true);
                }
                else
                {
                    _arSessionErrorAlertText.text = new LocalizedString("000 - Fleepas", "fleepSite_error_resolving").GetLocalizedString();
                    _arSessionErrorAlert.SetActive(true);
                }
            }
        }

        private void UpdatePlaneVisibility(bool visible)
        {
            foreach (var plane in _planeManager.trackables)
            {
                plane.gameObject.SetActive(visible);
            }
        }

        private void ARCoreLifecycleUpdate()
        {
            if (_arSessionHasError)
            {
                return;
            }
            // Show alert in case AR Session throughs an error
            if (ARSession.state != ARSessionState.Ready &&
                ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                // Activating ar session alert message
                // This alert will take us out of the fleep
                _arSessionErrorAlert.SetActive(true);
                _arSessionHasError = true;
                
            }
        }

        private void DisplayTrackingHelperMessage()
        {
            if (_arSessionHasError || ARSession.notTrackingReason == NotTrackingReason.None
                || ARSession.notTrackingReason == NotTrackingReason.Initializing)
            {
                _arCoreTrackingAlertText.gameObject.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                _arCoreTrackingAlertText.gameObject.transform.parent.gameObject.SetActive(true);
                switch (ARSession.notTrackingReason)
                {
                    case NotTrackingReason.Relocalizing:
                        _arCoreTrackingAlertText.text = _relocalizingMessage;
                        return;
                    case NotTrackingReason.InsufficientLight:
                        if (_versionInfo.GetStatic<int>("SDK_INT") < _androidSSDKVesion)
                        {
                            _arCoreTrackingAlertText.text = _insufficientLightMessage;
                        }
                        else
                        {
                            _arCoreTrackingAlertText.text = _insufficientLightMessageAndroidS;
                        }

                        return;
                    case NotTrackingReason.InsufficientFeatures:
                        _arCoreTrackingAlertText.text = _insufficientFeatureMessage;
                        return;
                    case NotTrackingReason.ExcessiveMotion:
                        _arCoreTrackingAlertText.text = _excessiveMotionMessage;
                        return;
                    case NotTrackingReason.Unsupported:
                        _arCoreTrackingAlertText.text = _unsupportedMessage;
                        return;
                    default:
                        _arCoreTrackingAlertText.text = _unsupportedMessage;
                        return;
                }
            }
        }

        /// <summary>
        /// Get the camera pose for the current frame.
        /// </summary>
        /// <returns>The camera pose of the current frame.</returns>
        public Pose GetCameraPose()
        {
            return new Pose(_arCamera.transform.position, _arCamera.transform.rotation);
        }


        /// <summary>
        /// Callback handling the validation of the input field.
        /// </summary>
        /// <param name="inputString">The current value of the input field.</param>
        public void OnInputFieldValueChanged()
        {
            // Cloud Anchor name should only contain certain characters
            var regex = new Regex("^[a-zA-Z0-9ñÑáÁéÉíÍóÓúÚ .,;:+=><#%&()'/¡!¿?$€£¥₣₹(\n|\r|\r\n)_-]*$");
            _incorrectCharacterWarning.SetActive(!regex.IsMatch(_cloudAnchorNameField.text));
            _shortNameWarning.SetActive(_cloudAnchorNameField.text.Length < 6);
            _longNameWarning.SetActive(_cloudAnchorNameField.text.Length > 130);
            SetSaveButtonActive(!_incorrectCharacterWarning.activeSelf && !_shortNameWarning.activeSelf
                && !_longNameWarning.activeSelf);

        }

        private void SetSaveButtonActive(bool active)
        {
            _saveCloudAnchorButton.interactable = active;
            var textColor = _saveCloudAnchorButton.GetComponentInChildren<TextMeshProUGUI>().color;
            if (active)
                textColor.a = 1f; 
            else
                textColor.a = 0.3f;
            _saveCloudAnchorButton.GetComponentInChildren<TextMeshProUGUI>().color = textColor;
        }

        /// <summary>
        /// Callback handling "Ok" button click event for input field.
        /// </summary>
        /// 
        private bool _saveButtonEnabled = true;
        public void OnSaveButtonClicked()
        {
            if (_saveButtonEnabled)
            {
                // Hide Name Panel and shot waiting message
                _cloudAnchorDataPanel.SetActive(false);
                _fleepSiteCreationWaitMessage.SetActive(true);
                // Get Serialized Mesh
                StartCoroutine(_meshBuilder.DecomposeARMesh(_arMesh, serializedMesh =>
                {
                // Compress Serialized Mesh
                var compressedSerializedMeshPath = SaveCompressedSerializedMesh(serializedMesh);
                // Get serialized local Pose from mesh vs cloud anchor (the mesh Pose in the cloud anchor local space)
                // In this case the resultant byte array is shorter if we don't compress it.
                sbyte[] serializedMeshLocalPose = HelperFunctions.ByteArrayToSbyteArray(HelperFunctions.SerializePose(_meshLocalPose));
                // Send AR Mesh + pose, Selected Image forlder address, CloudAnchorId and Fleepsite title
                MainActivityMessagingManager.CreateSingleMeshFleepSite(
                        compressedSerializedMeshPath,
                        serializedMeshLocalPose,
                        ScreenShotSaver.SharedInstance.GetScreenShotPath(_screenShotCarrousel.ScreenShotId),
                        _hostCloudAnchor.CloudAnchorId,
                        _cloudAnchorNameField.text,
                        (_isPublicFleepSite.value == 1f),
                        (DateTime.ParseExact(_expirationDateText.text, FleepasFormats.DateFormat, null)
                        - DateTime.Today).Days, // Days to expiration date
                        _cloudAnchorLifetime
                    );
                }));
                // Using this function to avoid repeating Save Click
                // I think it shouldn't happen, but once I've seen the save image/mesh saved twice
                _saveButtonEnabled = false;
                StartCoroutine(
                    HelperFunctions.SpaceButtonClick(() => _saveButtonEnabled = true)
                );
            }

        }


        public string SaveCompressedSerializedMesh(byte[] serializedMesh)
        {
            // Compress Serialized Mesh
            byte[] compressedSerializedMesh = SevenZipHelper.Compress(serializedMesh);
            // Set temp directory
            var compressedSerializedMeshPath = Application.temporaryCachePath + "/compressedSerializedMesh";
            // Remove old files on first iteration           
            if (Directory.Exists(compressedSerializedMeshPath))
            {
                // Remove directory and its content if there was one previously
                Directory.Delete(compressedSerializedMeshPath, true);
            }            Directory.CreateDirectory(compressedSerializedMeshPath);

            // Save Mesh
            string filePath = Path.Combine(compressedSerializedMeshPath, "mesh.txt");
            File.WriteAllBytes(filePath, compressedSerializedMesh);
            // Return path
            return filePath;
        }


        public void OnCancelButtonClicked()
        {
            _cloudAnchorScreenShots.SetActive(false);
            _instructionTextLayout.SetActive(true);
            ModifyInstructionText(_instructionTextInitialMessage, false);
        }

        // Raycast on AR Mesh Function
        private GameObject GetRayToHitPoint(Touch touch)
        {
            // Creating ray as a Gameobject
            GameObject ray = new GameObject();
            // Getting the camera position and rotation as 
            ray.transform.position = _arCamera.transform.position;
            ray.transform.rotation = _arCamera.transform.rotation;
            // Setting the shooting direction rotation 
            var rotationAngles = RayRotationAngles(touch);
            // Rotate on each axis the previously calculated angles
            ray.transform.Rotate(new Vector3(-rotationAngles[0], rotationAngles[1], 0), Space.Self);
            // Last rotation to position the long dimension of the bullet in the shooting direction
            ray.transform.Rotate(new Vector3(90, 0, 0), relativeTo: Space.Self);

            return ray;

        }

        private float[] RayRotationAngles(Touch touch)
        {
            // Calculate ray direction towards the touch point
            // Aplying some trigonometry in order to calculate the rotation angles
            var normalizeTouchPosition = new Vector2(2 * touch.position.x / Screen.width - 1f, 2 * touch.position.y / Screen.height - 1f);
            var verticalSemiFieldOfView = _arCamera.fieldOfView / 2;
            var horizontalSemiFieldOfView = Camera.VerticalToHorizontalFieldOfView(_arCamera.fieldOfView, _arCamera.aspect) / 2;
            // Calculate angle to rotate local x-axis
            float xAngle = CalculateRotationAngle(normalizeTouchPosition.y, verticalSemiFieldOfView);
            float yAngle = CalculateRotationAngle(normalizeTouchPosition.x, horizontalSemiFieldOfView);

            float[] rotationAngles = new float[2] { xAngle, yAngle };

            return rotationAngles;
        }

        private float CalculateRotationAngle(float touchPosition, float fieldOfView)
        {
            return Mathf.Atan(touchPosition * Mathf.Tan(fieldOfView * Mathf.PI / 180)) * 180 / Mathf.PI;
        }

        private void CheckNetwork()
        {
            // Don't check if there are not pending anchors to host/resolve
            // because we wouldn't need the network otherwise
            if (_pendingHostCloudAnchorsPromises.Count == 0
                && _pendingResolveCloudAnchorsPromises.Count == 0)
            {
                if (_networkErrorMessage.activeSelf) 
                    _networkErrorMessage.SetActive(false);
                return;

            } else
            {
                if (!_networkCoroutineRunning)
                {
                    _networkCoroutineRunning = true;
                    StartCoroutine(HelperFunctions.CheckNetworkError(0.3f, isNetworkError =>
                    {
                        // Show alert in case of network 
                        _networkErrorMessage.SetActive(isNetworkError);
                        _networkCoroutineRunning = false;
                    }));
                }
            }
        }
        
        private void ModifyInstructionText(string message, bool showAlertImage)
        {
            _instructionText.text = message;
            _instructionTextAlert.SetActive(showAlertImage);
            _instructionTextSpacer.SetActive(showAlertImage);
        }

        public void ReceiveFleepSiteCreationResponseFromAndroid(string errorCode)
        {
            _fleepSiteCreationWaitMessage.SetActive(false);
            if (errorCode == "")
            {
                // Show success message
                _fleepSiteCreationSuccess.SetActive(true);
                // Hide the cancel & play button
                if (_cancelAndPlayButton != null)
                    _cancelAndPlayButton.SetActive(false);
            }
            else
            {
                // Change text and show alert
                if (errorCode == "SLOW_NETWORK" || errorCode == "NO_NETWORK")
                    _fleepSiteCreationFailureText.text = new LocalizedString("000 - Fleepas", "no_network").GetLocalizedString();
                else if (errorCode == "LOCATION_NULL" || errorCode == "LOCATION_NOT_ENABLED")
                    _fleepSiteCreationFailureText.text = new LocalizedString("000 - Fleepas", "no_location").GetLocalizedString();
                else
                    _fleepSiteCreationFailureText.text = new LocalizedString("000 - Fleepas", "fleepSite_error_creation").GetLocalizedString();
                _fleepSiteCreationFailure.SetActive(true);
            }
        }


        public void ReceiveFleepSiteGetDataResponseFromAndroid(string errorCode)
        {
            // Show error message in case we received one error. The fleepsite won't be loaded if that happens
            if (errorCode == "")
            {
                // Fire reception finished event
                _fleepSiteGetDataFinished.Fire();
            }
            else
            {
                // Change text and show alert
                if (errorCode == "SLOW_NETWORK" || errorCode == "NO_NETWORK")
                    _fleepSiteGetDataFailureText.text = new LocalizedString("000 - Fleepas", "no_network").GetLocalizedString();
                else if (errorCode == "INCORRECT_FLEEPSITE")
                    _fleepSiteGetDataFailureText.text = new LocalizedString("000 - Fleepas", "fleepSite_not_enabled").GetLocalizedString();
                else
                    _fleepSiteGetDataFailureText.text = new LocalizedString("000 - Fleepas", "fleepSite_error_get_data").GetLocalizedString();
                _fleepSiteGetDataFailure.SetActive(true);
            }
        }

        public void ReceiveFleepSiteDataFromAndroid(string getDataParamsString)
        {
            // Parsing received parameters
            var creationParams = getDataParamsString.Split('|');
            if (creationParams[0] == "cloudanchor")
            {
                // Loading cloud anchor image
                LoadFleepSiteImage(creationParams[2]);
                // Add cloudAnchorId to the resolving list
                _cloudAnchorsIdToResolve.Add(creationParams[1]);
            }
            if (creationParams[0] == "mesh")
            {
                // Params: 1: meshUri, 2. cloudAnchorId, 3. Mesh Pose Byte Array
                // Loading mesh
                StartCoroutine(HelperFunctions.LoadFileFromUri(creationParams[1], meshData => {
                    if (meshData == null)
                        _fleepSiteGetDataFailureText.text = new LocalizedString("000 - Fleepas", "fleepSite_error_get_data").GetLocalizedString();
                    else
                    {
                        // Continue if the meshdata is successfully downloaded
                        var uncompressedMeshData = SevenZipHelper.Decompress(meshData);
                        // Get Mesh Pose values
                        var arMeshHexStringPose = creationParams[3].Substring(creationParams[3].IndexOf("=") + 1, creationParams[3].IndexOf("}") - creationParams[3].IndexOf("=") - 2);
                        var arMeshLocalPose = HelperFunctions.DeserializePose(HelperFunctions.HexStringToByteArray(arMeshHexStringPose)); 

                        // If the cloud anchor of this mesh is already resolved, rebuild it
                        var cloudAnchor = _cloudAnchorsResolved.Where(ca => ca.Item1 == creationParams[2]).FirstOrDefault();
                        if (cloudAnchor != null)
                        {
                            RebuildCloudMesh(_arMesh, uncompressedMeshData, arMeshLocalPose, cloudAnchor.Item2);
                        } else
                        {
                            // Otherwise, add the mesh to the mesh list and wait for the corresponding cloud anchor to be resolved
                            var cloudMesh = new FleepasCloudMesh(uncompressedMeshData, arMeshLocalPose, creationParams[2]);
                            _cloudMeshList.Add(cloudMesh);   
                        }
                    }
                }));
            }
        }

        private void RebuildCloudMesh(GameObject parentARMesh, byte[] cloudMeshData, Pose cloudMeshPose, ARCloudAnchor cloudAnchor)
        {
            // Rebuilding the mesh
            StartCoroutine(_meshBuilder.RebuildMesh(cloudMeshData, parentARMesh, success =>
            {
                if (success)
                {
                    // Move rebuilt mesh under its cloud anchor
                    parentARMesh.transform.SetParent(cloudAnchor.transform);
                    // Change its local pose (vs the cloud anchor) accordingly
                    parentARMesh.transform.localPosition = cloudMeshPose.position;
                    parentARMesh.transform.localRotation = cloudMeshPose.rotation;
                    // Additional actions after resolving. It might change depending on the scene. Extend the class in such case
                    ActionsAfterRebuiding();

                } else
                {
                    // Show alert if mesh rebuilding fails
                    _fleepSiteGetDataFailureText.text = new LocalizedString("000 - Fleepas", "fleepSite_error_get_data").GetLocalizedString();
                    _fleepSiteGetDataFailure.SetActive(true);
                }

            }));

        }

        private void ActionsAfterRebuiding()
        {
            // Precalculate mesh paramteres
            ARMeshData.SharedInstance.PreCalculateARMeshData();
            // Show start game dialog
            _singlePlayerStartGame.SetActive(true);
        }

        private void LoadFleepSiteImage(string imageUrl)
        {
            StartCoroutine(ScreenShotSaver.SharedInstance.LoadScreenShot(imageUrl, false, t => {
                if (t != null)
                {
                    _fleepSiteImage.texture = t;
                }
                else
                {
                    // Show alternative texture in case of receiving null data
                    _fleepSiteImage.texture = _alertTexture;
                }
            }));
        }

        private Signal _resetTimer = new();
        public void LaunchLongMappingTimer()
        {
            _resetTimer.Fire();
            Observable.Timer(TimeSpan.FromSeconds(_longMappingWarningTime)).TakeUntil(_mappingFinished).TakeUntil(_resetTimer).Subscribe(_ =>
            {
                _longMappingWarning.SetActive(true);
            }).AddTo(disposables);
        }


        public void OnDisable()
        {
            // Cleaning
            if (_qualityIndicator != null)
            {
                Destroy(_qualityIndicator.gameObject);
                _qualityIndicator = null;
            }
            if (_pendingAnchor != null)
            {
                Destroy(_pendingAnchor.gameObject);
                _pendingAnchor = null;
            }
            if (_hostCloudAnchor != null)
            {

                _hostCloudAnchor = null;
            }
            if (_resolveCloudAnchor != null)
            {
                Destroy(_resolveCloudAnchor.Anchor.gameObject);
                _resolveCloudAnchor = null;
            }
            if (_hostCloudAnchorPromise != null)
            {
                _hostCloudAnchorPromise.Cancel();
            }
            if (_resolveCloudAnchorPromise != null)
            {
                _resolveCloudAnchorPromise.Cancel();
            }
            if (_pendingHostCloudAnchorsPromises.Count > 0)
            {
                _pendingHostCloudAnchorsPromises.Clear();
            }
            if (_pendingResolveCloudAnchorsPromises.Count > 0)
            {
                _pendingResolveCloudAnchorsPromises.Clear();
            }
            if (_cloudAnchorsResolved.Count > 0)
            {
                foreach (var anchor in _cloudAnchorsResolved)
                {
                    if (anchor.Item2.gameObject != null)
                    {
                        Destroy(anchor.Item2.gameObject);
                    }
                }
                _cloudAnchorsResolved.Clear();
            }
            _cloudAnchorsIdToResolve.Clear();
            _cloudMeshList.Clear();
            // Stop capturing ScreenShots
            _mappingFinished.Fire();
            //UpdatePlaneVisibility(false);
        }

     


        public void ChangePublicSliderValue()
        {
            // Change color and value of slider
            _isPublicFleepSite.value = 1 - _isPublicFleepSite.value;
            var sliderColors = _isPublicFleepSite.colors;
            if (_isPublicFleepSite.value == 1)
                sliderColors.disabledColor = new Color(0f, 0.8941f, 0.7254f, 1f); // Turn Green
            else
                sliderColors.disabledColor = new Color(0.977f, 0.523f, 0.668f, 1f); // Turn Red
            _isPublicFleepSite.colors = sliderColors;
        }
    }
}
