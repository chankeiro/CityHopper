using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Networking;

using Unity.Collections;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using System.Threading;
using UniRx;
using System.Globalization;




namespace Bercetech.Games.Fleepas
{
    public class ScreenShotSaver : MonoBehaviour
    {
        private string _screenCapturesPath;
        private int _screenWidth;
        private int _screenHeight;
        private int _screenShotId;
        // Texture Readback Objects
        private RenderTexture _tempRenderTexture;
        private RenderTexture _tempReducedRenderTexture;
        private WaitForEndOfFrame _waitForEndOfFrame;
        [SerializeField]
        private int _captureWidth = 768;
        [SerializeField]
        private int _captureHeight = 768;
        private float _screenToCaptureRatio;

        // Defining a static shared instance variable so other scripts can access to the object pool
        private static ScreenShotSaver _sharedInstance;
        public static ScreenShotSaver SharedInstance => _sharedInstance;

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
        void Start()
        {
            // Set image directory
            _screenCapturesPath = Application.temporaryCachePath + "/ScreenShots";
            // Get Screen size
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            // Set rendertextures and texture2D that will be used during the capture
            // This first rendertexture will be used to capture the actual screen.
            _tempRenderTexture = new RenderTexture(_screenWidth, _screenHeight, 0);
            // This second rendertexture will be used to reduce the size of the first texture, from the original size to the screen to
            // the desired size, so the algorithm is more efficiente
            // Frameheight must be even for ffmpeg not throwing an error
            _tempReducedRenderTexture = new RenderTexture(_captureWidth, _captureHeight, 0);
            // Set the WaitForEndOfFrame once, it migth save some time when looping
            _waitForEndOfFrame = new WaitForEndOfFrame();
            // Ratio used in formulas
            _screenToCaptureRatio = 1f * (_screenWidth * _captureHeight) / (_screenHeight * _captureWidth);
            // Initializing screen shot id
            _screenShotId = 1;

        }



        public IEnumerator SaveScreenShot(bool firstScreenShot)
        {
            if (firstScreenShot) { 
                _screenShotId = 0;
                // Remove old files on first iteration
                yield return null;
                if (Directory.Exists(_screenCapturesPath))
                {
                    // Remove directory and its content if there was one previously
                    Directory.Delete(_screenCapturesPath, true);
                }
                Directory.CreateDirectory(_screenCapturesPath);
            }

            // Wait till the end of the frame to do the rest of the process
            yield return _waitForEndOfFrame;

            // Capture screen
            ScreenCapture.CaptureScreenshotIntoRenderTexture(_tempRenderTexture);
            // Copy to the render texture with reduced size.
#if UNITY_EDITOR
            // Need to flip the image with Unity Editor
            Graphics.Blit(_tempRenderTexture, _tempReducedRenderTexture,
                new Vector2(1f, -_screenToCaptureRatio), // Scale
                new Vector2(0.0f, (1 + _screenToCaptureRatio)/2f)); //Offset
#else
            Graphics.Blit(_tempRenderTexture, _tempReducedRenderTexture,
                new Vector2(1f, _screenToCaptureRatio), // Scale
                new Vector2(0.0f, (1 - _screenToCaptureRatio)/ 2f)); //Offset
#endif

            // Copy the render texture in the GPU to a texture format, so we can work with it in the callback
            // WARNING: need >= Unity 2021.2 to use AsyncGPUReadback with OpenGL platforms
            AsyncGPUReadback.Request(_tempReducedRenderTexture, 0, TextureFormat.RGB24, OnCompleteReadback);

        }

        // This is called after every AsyncGPUReadback call
        void OnCompleteReadback(AsyncGPUReadbackRequest request)
        {
            if (request.hasError == false && request.GetData<uint>().IsCreated)
            {
                // Transforming the texture to raw texture data. We cannot enqueue the Texture2D directly,
                // since that type is not preserved in a queue (or a list)
                Texture2D tex = new Texture2D(_captureWidth, _captureHeight, TextureFormat.RGB24, false);
                tex.LoadRawTextureData(request.GetData<uint>());
                // Increasing screenShotId just before writing, to ensure that we do not write two files with the
                // same id (in case SaveScreenShot is called before the previous call is finished, although normally it shouldn't
                // happen unless this function is called with a very high frequency)
                _screenShotId += 1;
                string filePath = Path.Combine(_screenCapturesPath, "ss_" + _screenShotId + ".jpg");
                File.WriteAllBytes(filePath, tex.EncodeToJPG());
                // To avoid memory leaks
                Destroy(tex);
            }
        }

        // This version is less performand and doesn't work very vell
        public IEnumerator SaveScreenShot2(int screenShotId)
        {

            // Remove old files on first iteration
            if (screenShotId == 1)
            {
                yield return null;
                if (Directory.Exists(_screenCapturesPath))
                {
                    // Remove directory and its content if there was one previously
                    Directory.Delete(_screenCapturesPath, true);
                }
                Directory.CreateDirectory(_screenCapturesPath);
            }
            yield return _waitForEndOfFrame;
            // Get squared image
            Texture2D ss = new Texture2D(_screenWidth, _screenWidth, TextureFormat.RGB24, false);
            ss.ReadPixels(new Rect(0, (_screenHeight - _screenWidth) / 2, _screenWidth, _screenWidth), 0, 0);
            ss.Apply();
            // Scale image to 768x768
            ss = TextureScaler.scaled(ss, _captureWidth, _captureHeight);

            string filePath = Path.Combine(_screenCapturesPath, "ss_" + screenShotId + ".jpg");
            File.WriteAllBytes(filePath, ss.EncodeToJPG());

            // To avoid memory leaks
            Destroy(ss);

        }


        public IEnumerator LoadScreenShot(string file, bool fromLocal, Action<Texture> GetTexture)
        {
            string filePath = "";
            if (fromLocal)
            {
                // Need to add file:// in case of local files
                filePath = "file://" + Path.Combine(_screenCapturesPath, file);
            } else
            {
                filePath = file;
            }
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filePath))
            {
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Logging.OmigariHP(uwr.error);
                    if (GetTexture != null)
                    {
                        GetTexture(null);
                    }
                }
                else
                {
                    // Get downloaded asset bundle
                    if (GetTexture != null)
                    {
                        GetTexture(DownloadHandlerTexture.GetContent(uwr));
                    }
                }
            }
        }

        public string GetScreenShotPath(int screenShotId)
        {
            return Path.Combine(_screenCapturesPath, "ss_" + screenShotId + ".jpg");
            
        }

        public void testSaveScreenShot()
        {
            StartCoroutine(SaveScreenShot(true));
        }

    }
}
