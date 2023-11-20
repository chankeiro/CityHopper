using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using TMPro;
using System;

namespace Bercetech.Games.Fleepas
{
    /// <summary>
    /// A component that can be used to access the most recently received basic light estimation information
    /// for the physical environment as observed by an AR device.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class BasicLightEstimation : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The ARCameraManager which will produce frame events containing light estimation information.")]
        private ARCameraManager _arCameraManager;
        private Light _light;
        [SerializeField]
        private bool _writeValues;
        [SerializeField]
        private TextMeshProUGUI _brightnessValue;
        [SerializeField]
        private TextMeshProUGUI _lightBrightnessValue;
        [SerializeField]
        private TextMeshProUGUI _lightIntensityLumensValue;
        [SerializeField]
        private TextMeshProUGUI _colorTemperatureValue;
        [SerializeField]
        private TextMeshProUGUI _colorCorrectionValue;
        [SerializeField]
        private TextMeshProUGUI _lightColorValue;
        [SerializeField]
        private TextMeshProUGUI _lightDirectionValue;
        [SerializeField]
        private TextMeshProUGUI _sphericalHarmonicsValue;

        // Findings
        // Android - Xiaomi
        // - Enabling MainLightEstimation, MainLightDirection and/or Ambient Spherical Armonics will provide values for sphericalHarmonics, lightDirection and lightColor.
        // It seems that the provided values are more or less the same, independently of the mode. The lightColor RGBA seems to be more or 
        // less reactive to the real light of the scene, although sometimes in areas with a lot of values provides lower values than in areas
        // with less light. But in general it seems to work more or less OK. I don't know why, but some times the RGB values are above 1.
        // Regardinbg sphericalHarmonics, I am not sure if it has an actual impact, I cannot perceive it clearly. Perhaps with metallic surfaces makes more sense. I am not
        // using it for now.
        // Although in the documentation https://developers.google.com/ar/develop/unity-arf/lighting-estimation/developer-guide#enable_environmental_hdr_mode
        // says that to enable this light estimation modes you need to enable some of them in the
        // ARCameraManager first, but also add an AREnvironmentProbeManager, I receive values anyway without this manager. So I am not using it because
        // it probably improves performance, and I am not so interested in the probe effect, since I am not using metallic surfaces. And the environment
        // probes effect isn't so good in Android as it can be seen in the documentation images (it is blurry¡, but some effect exists anyway, I tested it just puttin a 100% metallic ball in the scene).
        // Conclusion:
        // 1. I am enabling MainLightEstimation and MainLightDirection modes in the ARCameraManager (althoug I guess with just one of them is enough, but there might be phones that only give values for
        // one of them). I am not enabling AmbientSphericalArmonics, just in case it reduces performance (although I didn't see any place in the documentation where it says it).
        // 2. I will modify the light direction, light color and also the light shadow Strength averaging the RGB values of the light color (from my point of view, less light also implies less shadow).
        
        // - Enabling AmbientColor and/or AmbientIntensity provides Color Correction and Brightness. Color Correction Alpha is always the same as the Brightness value.
        // However, it doesn't seem to change, even when focusing to the sun. Only goes to 0 y the camera lens are covered, but for the rest of the cases
        // the values are always between 0.4 and 0.6. Not using This.


        /// <summary>
        /// Get or set the <c>ARCameraManager</c>.
        /// </summary>
        public ARCameraManager cameraManager
        {
            get { return _arCameraManager; }
            set
            {
                if (_arCameraManager == value)
                    return;

                if (_arCameraManager != null)
                    _arCameraManager.frameReceived -= FrameChanged;

                _arCameraManager = value;

                if (_arCameraManager != null & enabled)
                    _arCameraManager.frameReceived += FrameChanged;
            }
        }

        /// <summary>
        /// The estimated brightness of the physical environment, if available.
        /// </summary>
        private float? _brightness;

        /// <summary>
        /// The estimated color temperature of the physical environment, if available.
        /// </summary>
        private float? _colorTemperature;

        /// <summary>
        /// The estimated color correction value of the physical environment, if available.
        /// </summary>
        private Color? _colorCorrection;

        /// <summary>
        /// The estimated direction of the main light of the physical environment, if available.
        /// </summary>
        private Vector3? _mainLightDirection;

        /// <summary>
        /// The estimated color of the main light of the physical environment, if available.
        /// </summary>
        private Color? _mainLightColor;
        private float _shadowStrength; // I get this value from the RGB values of _mainLightColor

        /// <summary>
        /// The estimated intensity in lumens of main light of the physical environment, if available.
        /// </summary>
        private float? _mainLightIntensityLumens;
        private float? _mainLightBrightness;

        /// <summary>
        /// The estimated spherical harmonics coefficients of the physical environment, if available.
        /// </summary>
        private SphericalHarmonicsL2? _sphericalHarmonics; 

        void Awake()
        {
            _light = GetComponent<Light>();
        }

        void OnEnable()
        {
            if (_arCameraManager != null)
                _arCameraManager.frameReceived += FrameChanged;
        }

        void OnDisable()
        {
            if (_arCameraManager != null)
                _arCameraManager.frameReceived -= FrameChanged;
        }
        //float average_brightness = 0.2126f * our_light.color.r + 0.7152f * our_light.color.g + 0.0722f * our_light.color.b;
        void FrameChanged(ARCameraFrameEventArgs args)
        {
            //if (args.lightEstimation.averageBrightness.HasValue)
            //{
            //    _brightness = args.lightEstimation.averageBrightness.Value;
            //    _light.intensity = _brightness.Value;
            //    if (_writeValues)
            //        _brightnessValue.text = Math.Round(_brightness.Value, 2).ToString();
            //}
            //else
            //{
            //    if (_writeValues)
            //        _brightnessValue.text = "";
            //}

            // It seems that it is only available in ARKIT
            //if (args.lightEstimation.averageColorTemperature.HasValue)
            //{
            //    _colorTemperature = args.lightEstimation.averageColorTemperature.Value;
            //    _light.colorTemperature = _colorTemperature.Value;
            //    if (_writeValues)
            //        _colorTemperatureValue.text = _colorTemperature.Value.ToString();
            //}
            //else
            //{
            //    if (_writeValues)
            //        _colorTemperatureValue.text = "";
            //}


            //if (args.lightEstimation.colorCorrection.HasValue)
            //{
            //    _colorCorrection = args.lightEstimation.colorCorrection.Value;
            //    _light.color = _colorCorrection.Value;
            //    if (_writeValues)
            //        _colorCorrectionValue.text = _colorCorrection.Value.ToString();
            //}
            //else
            //{
            //    if (_writeValues)
            //        _colorCorrectionValue.text = "";
            //}

            if (args.lightEstimation.mainLightDirection.HasValue)
            {
                _mainLightDirection = args.lightEstimation.mainLightDirection;
                _light.transform.rotation = Quaternion.LookRotation(_mainLightDirection.Value);
                if (_writeValues)
                    _lightDirectionValue.text = _mainLightDirection.Value.ToString();
            }
            else
            {
                if (_writeValues)
                    _lightDirectionValue.text = "";
            }

            if (args.lightEstimation.mainLightColor.HasValue)
            {
                _mainLightColor = args.lightEstimation.mainLightColor;
                _shadowStrength = Mathf.Min((_mainLightColor.Value.r + _mainLightColor.Value.g + _mainLightColor.Value.b) / 3f, 1f);
                // Need to cap it at 1 for every rgb value, because some times it exceeds that value and
                // the objects appear almost white. 
                _light.color = new Color(Mathf.Min(_mainLightColor.Value.r, 1f), Mathf.Min(_mainLightColor.Value.g, 1f), Mathf.Min(_mainLightColor.Value.b, 1f));
                _light.shadowStrength = _shadowStrength; 
                if (_writeValues)
                    _lightColorValue.text = _mainLightColor.Value.ToString();
            }
            else
            {
                if (_writeValues)
                    _lightColorValue.text = "";
            }

            //if (args.lightEstimation.averageMainLightBrightness.HasValue)
            //{
            //    _mainLightIntensityLumens = args.lightEstimation.mainLightIntensityLumens;
            //    _mainLightBrightness = args.lightEstimation.averageMainLightBrightness;
            //    _light.intensity = _mainLightBrightness.Value;
            //    if (_writeValues)
            //    {
            //        _lightIntensityLumensValue.text = Math.Round(_mainLightIntensityLumens.Value, 2).ToString();
            //        _lightBrightnessValue.text = Math.Round(_mainLightBrightness.Value, 2).ToString();
            //    }
            //}
            //else
            //{
            //    if (_writeValues)
            //    {
            //        _lightIntensityLumensValue.text = "";
            //        _lightBrightnessValue.text = "";
            //    }
            //}

            //if (args.lightEstimation.ambientSphericalHarmonics.HasValue)
            //{
            //    _sphericalHarmonics = args.lightEstimation.ambientSphericalHarmonics;
            //    RenderSettings.ambientMode = AmbientMode.Skybox;
            //    RenderSettings.ambientProbe = _sphericalHarmonics.Value;
            //    if (_writeValues)
            //        _sphericalHarmonicsValue.text = _sphericalHarmonics.Value.ToString();
            //} else
            //{
            //    if (_writeValues)
            //        _sphericalHarmonicsValue.text = "";
            //}

        }


    }
}