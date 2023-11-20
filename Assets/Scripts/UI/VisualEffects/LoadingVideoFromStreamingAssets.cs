using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Collections;
using UnityEngine.UI;



[RequireComponent(typeof(VideoPlayer))]
public class LoadingVideoFromStreamingAssets : MonoBehaviour
{
    [SerializeField]
    public string _videoFilename;
    [SerializeField]
    private Shader _videoAlphaShader;
    private Material _videoAlphaMaterial;
    [SerializeField]
    private int _size;
    [SerializeField]
    private float _playbackSpeed;
    [SerializeField]
    private float _minDarknessToAlpha = 0; 
    [SerializeField]
    private float _maxDarknessToAlpha = 0.05f;
    // Spring Effect: Faster in the middle, slower at the beginning and end
    [SerializeField]
    private bool _springEffect = false;
    private RenderTexture _baseVideoTexture;
    private CustomRenderTexture _shaderVideoTexture;
    private VideoPlayer _vp;

    public string GetFileLocation(string relativePath)
    {
        // WARNING: IOS MIGHT NEED THE COMMENTED VERSION
        return Path.Combine(Application.streamingAssetsPath, relativePath);
        //return "file://" + Path.Combine(Application.streamingAssetsPath, relativePath);
    }

    void Awake()
    {
        // Generate Video Player
        _vp = gameObject.GetComponent<VideoPlayer>();
        _vp.playOnAwake = false;
        _vp.waitForFirstFrame = true;
        _vp.isLooping = true;
        _vp.skipOnDrop = true;

        // Getting Videos from Streaming Assets. Otherwise Android (at least) cannot play them
        // if they are embedded in the exported Unity package
        _vp.url = GetFileLocation(_videoFilename);
        _baseVideoTexture = new RenderTexture(_size, _size, 24, RenderTextureFormat.ARGBHalf);
        // Assigning video to the base texture
        _vp.targetTexture = _baseVideoTexture;
        if (!_springEffect)
            _vp.playbackSpeed = _playbackSpeed;

        // Android can´t play vp8 videos with transparency from Unity. 
        // Therefore, a shader (present in videoMaterial) is applied to the base video texture, to 
        // get a new texture where the pixels in the darkness range are converted to transparent
        // IOS and Unity Editor can play them, so they don´t need the shader to be applied

        // Reasign variables as workaround to prevent  Waring 0414 (private field assigned but not used).
        // alternative is to use a "#pragma warning disable 0414" to disable that kind of warnings, but
        // I don´t like it
        float minDarknessToAlpha = _minDarknessToAlpha;
        float maxDarknessToAlpha = _maxDarknessToAlpha;

#if UNITY_ANDROID
        _shaderVideoTexture = new CustomRenderTexture(_size, _size, RenderTextureFormat.ARGBHalf);
        _videoAlphaMaterial = new Material(_videoAlphaShader);
        _shaderVideoTexture.material = _videoAlphaMaterial;
        _shaderVideoTexture.material.SetTexture("_MainTex", _baseVideoTexture);
        _shaderVideoTexture.material.SetFloat("_MinDarkness", minDarknessToAlpha);
        _shaderVideoTexture.material.SetFloat("_MaxDarkness", maxDarknessToAlpha);
        _shaderVideoTexture.initializationMode = CustomRenderTextureUpdateMode.OnLoad;
        _shaderVideoTexture.updateMode = CustomRenderTextureUpdateMode.Realtime;
        GetComponent<RawImage>().texture = _shaderVideoTexture;
#else
        GetComponent<RawImage>().texture = _baseVideoTexture;
#endif
    }


    private void OnEnable()
    {
        // Play when the component is enabled
        StartCoroutine(PlayVideo());

    }

    private void Update()
    {
        // Faster in the middle, slower at the beginning and end
        if (_springEffect)
            _vp.playbackSpeed = _playbackSpeed * (Mathf.Pow(-1 * Mathf.Pow((float)(_vp.time - (_vp.length / 2)), 2) + Mathf.Pow((float)_vp.length / 2, 2), 0.75f) + 0.2f);
    }

    private IEnumerator PlayVideo()
    {

        GetComponent<VideoPlayer>().Play();
        yield return null;
    }

    private void OnDisable()
    {
        //Release textures from memmory since they are not garbage collected like normal managed types.
        _baseVideoTexture.Release();
#if UNITY_ANDROID && !UNITY_EDITOR
        _shaderVideoTexture.Release();
#endif
    }

}




