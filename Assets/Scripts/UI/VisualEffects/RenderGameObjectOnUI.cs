using UnityEngine;
using UnityEngine.UI;



namespace Bercetech.Games.Fleepas
{
    [RequireComponent(typeof(Camera))]
    public class RenderGameObjectOnUI : MonoBehaviour
    {
        // Here reference the camera component of the particles camera
        [SerializeField] private Camera _gameObjectCamera;

        // Reference the RawImage in your UI
        [SerializeField] private RawImage _targetImage;

        private RenderTexture renderTexture;

        // This scripts takes the output of the selected camera, and renders it on the texture of
        // a raw image. We are using it to render particle effects on a canvas image
        private void Awake()
        {
            if (!_gameObjectCamera) _gameObjectCamera = GetComponent<Camera>();

            renderTexture = new RenderTexture(_gameObjectCamera.pixelWidth, _gameObjectCamera.pixelHeight, 32);
            _gameObjectCamera.targetTexture = renderTexture;
            _targetImage.texture = _gameObjectCamera.targetTexture;
        }
    }
}
