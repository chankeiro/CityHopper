using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UniRx;
using System.Globalization;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;



namespace Bercetech.Games.Fleepas
{

    /// <summary>
    /// Captures frames from a Unity camera and audio in real time
    /// and writes them to disk.
    /// Generates a video with them
    /// </summary>
    /// 

    public class ScreenRecorder : MonoBehaviour
    {
        // Public Properties
        [SerializeField]
        private int _frameRate = 30; // number of frames to capture per second
        [SerializeField]
        private int _frameHeight = 1280;
        private int _frameWidth;
        [SerializeField]
        private float _timeToCountdownFinish;
        [SerializeField]
        private float _recordingDuration;

        // Recording variables
        private bool recordingStarted;
        private Signal recordFinished = new Signal();
        private int screenWidth;
        private int screenHeight;
        private string screenCapturesPath;
        private string audioCapturePath;

        // Texture Readback Objects
        private RenderTexture tempRenderTexture;
        private RenderTexture tempReducedRenderTexture;
        private NativeArray<byte> _rawTextureData;
        private int _textureDataLength;
        private GraphicsFormat _graphicsFormat;
        //private WaitForEndOfFrame _waitForEndOfFrame;

        // Timing Data
        private float captureFrameTime;
        private int frameNumber = 0;
        private float timeNow;

        // Encoding Variables
        private string videoOutputPath;
        public string VideoOutputPath => videoOutputPath;
        private Thread encoderThread = null;
        private bool threadIsProcessing;
        private bool isEncodedVideoFinished;
        private Signal<bool> encodingFinished = new Signal<bool>();
        public Signal<bool> EncodingFinished => encodingFinished;

        // CompositeDisposable is similar with List<IDisposable>
        // It will be used to gather all disposables active after the game is finished
        // so they can be disposed at that moment
        protected CompositeDisposable disposables = new CompositeDisposable();


        // Defining a static shared instance variable so other scripts can access to the object
        private static ScreenRecorder _sharedInstance;
        public static ScreenRecorder SharedInstance => _sharedInstance;
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

        private void Start()
        {
            // Prepare the data directory
            screenCapturesPath = Application.temporaryCachePath + "/ScreenRecorder";
            audioCapturePath = Application.temporaryCachePath + "/audio_capture.wav";
            videoOutputPath = screenCapturesPath + "/video_capture.mp4";

            // Set screen size initial values
            // Forcing alway screenHeight to be the longest dimension, independently on the screen orientation. Therefore
            // the result is good only in case of portrait. Need to improve this, but first, the game must allow landscape
            // mode for all the screens, which currently doesn't.
            screenWidth = Screen.currentResolution.width > Screen.currentResolution.height ? Screen.currentResolution.height : Screen.currentResolution.width;
            screenHeight = Screen.currentResolution.width > Screen.currentResolution.height ? Screen.currentResolution.width: Screen.currentResolution.height;

            // Set rendertextures and texture2D that will be used during the capture
            // This first rendertexture will be used to capture the actual screen.
            tempRenderTexture = new RenderTexture(screenWidth, screenHeight, 0);
            // This second rendertexture will be used to reduce the size of the first texture, from the original size to the screen to
            // a smaller size, so the algorithm is more efficiente
            // FrameWidth must be even for ffmpeg not throwing an error
            _frameWidth = (_frameHeight * screenWidth / screenHeight / 2) * 2;
            tempReducedRenderTexture = new RenderTexture(_frameWidth, _frameHeight, 0);
            // Getting the graphics format of a Texture with format RGB24 (don't know another way to get it)
            // We use the same textureformat later in the AsyncGPUReadback function
            _graphicsFormat = new Texture2D(0, 0, TextureFormat.RGB24, false).graphicsFormat;
            // Generate empty NativeArray of a prestablished length, that we'll use later to save the texture data
            // In case of the length being above IntMax (higher resolution or more seconds), you might have to split
            // in in more than one native array. With the current use case it is not necessary, except for UNITY_EDITOR
            // which might use the resolution of a big monitor and may overflow
            var textureLength =
#if !UNITY_EDITOR
                _frameWidth * _frameHeight // Number of pixels
                * 3 // 3 bytes per pixel because we are using RGB24 (1 byte per color channel)
                * (_frameRate + 1) // number of frames per second (plus a little more, since the final value might be slightly above)
                * (int)Math.Ceiling(_recordingDuration);
#else
                1;
#endif
            _rawTextureData = new NativeArray<byte>(textureLength, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            // Set the WaitForEndOfFrame once, it migth save some time when looping
            //_waitForEndOfFrame = new WaitForEndOfFrame();

            // Start recording when the starting time is reached
            Clock.SharedInstance.TimePassedMessage.Subscribe(timePassed =>
            {

                if (timePassed == 0)
                {
                    recordFinished.Fire(); // Finish any previous recording that might be still running
                    isEncodedVideoFinished = false; // As we start a new match, we forget about any previous video that could have been encoded in previous games
                    recordingStarted = false; // Reset recoding started flag when the game starts
                }

                // Start capturing images and sound after the recording time
                if (!recordingStarted & timePassed >= (Clock.SharedInstance.TotalSeconds - _timeToCountdownFinish) * 1000)
                {
                    Logging.Omigari("Capturing to: " + screenCapturesPath + "/");
                    // Capturing images
                    GetScreenCaptures();
                    // Recording wav clip
                    AudioRecorder.SharedInstance.StartAudioRecording.Fire(audioCapturePath);
                    recordingStarted = true;


                    // Finish after recording duration
                    Observable.Timer(TimeSpan.FromSeconds(_recordingDuration)).Subscribe(_ =>
                    {
                        recordFinished.Fire();
                        if (AudioRecorder.SharedInstance.enabled)
                            AudioRecorder.SharedInstance.StopAudioRecording.Fire();

                    }).AddTo(disposables);

                }
            }).AddTo(disposables);
        }

        private void GetScreenCaptures()
        {

            // Reseting frame number
            frameNumber = 0;

            // Screen capture interval
            captureFrameTime = 1.0f / (float)_frameRate;

            // Getting the time at the beginning of the frame
            timeNow = Time.time;
            Observable.EveryEndOfFrame().StartWith(0).TakeUntil(recordFinished).Subscribe(_ =>
            {
#if !UNITY_EDITOR
                RenderImage();
#endif
            }).AddTo(disposables);

        }


        private void RenderImage()
        {

            //// Wait till the end of the frame to do the rest of the process
            // NOT NEED THIS NOW SINCE WE ARE RUNNING IT ON EveryEndOfFrame
            //yield return _waitForEndOfFrame;

            // We capture the Screen every thime the captureFrameTime has passed. We even ask for a smaller factor of that, to account for
            // frame deltatimes variations, and in fact it helps to get more screenshots. But very often we won't get the target frame rate,
            // speacially in lower end devices, probably because there are more variations, or sometimes the FPS drops. But normally we will be very close.
            // Anyway, when the screen captures are encoded in a video, we must adjust the frames per seconds
            // of the video, so it is exactly the same as the frames per second saved by the script (totalframes/recordingDuration)

            if ((Time.time - timeNow) >= captureFrameTime * 0.95f) // Factor shouldn't be very low. Capturing too much data could overflow _rawTextureData
            {
                // Updating time
                timeNow = Time.time;
                // Capture screen
                ScreenCapture.CaptureScreenshotIntoRenderTexture(tempRenderTexture);
                // Copy to the render texture with reduced size.
#if UNITY_EDITOR
                // Need to flip the image with Unity Editor
                Graphics.Blit(tempRenderTexture, tempReducedRenderTexture, new Vector2(1f, -1f), new Vector2(0.0f, 1f));
#else
			    Graphics.Blit(tempRenderTexture, tempReducedRenderTexture); 
#endif
                // Copy the render texture in the GPU to a texture format, so we can work with it in the callback
                // WARNING: need >= Unity 2021.2 to use AsyncGPUReadback with OpenGL platforms
                AsyncGPUReadback.Request(tempReducedRenderTexture, 0, TextureFormat.RGB24, OnCompleteReadback);


                //Less performant
                //ScreenCapture.CaptureScreenshotIntoRenderTexture(tempRenderTexture);
                //Graphics.Blit(tempRenderTexture, tempReducedRenderTexture, new Vector2(1f, -1f), new Vector2(0.0f, 1f));
                //RenderTexture.active = tempReducedRenderTexture;
                //StartCoroutine(ReadPixelsTexture2D(tempTexture2D));
                //RenderTexture.ReleaseTemporary(tempRenderTexture);
                //RenderTexture.ReleaseTemporary(tempReducedRenderTexture);

            }

        }


        // This is called after every AsyncGPUReadback call
        void OnCompleteReadback(AsyncGPUReadbackRequest request)
        {
            if (request.hasError == false && request.GetData<byte>().IsCreated && _rawTextureData.IsCreated)
            {
                // All request have the same length. Getting them just the first time
                if (_textureDataLength == 0)
                    _textureDataLength = request.GetData<byte>().Length;
                // Concatenate NativeArrays in a big one
                NativeArray<byte>.Copy(request.GetData<byte>(), 0, _rawTextureData, _textureDataLength * frameNumber, _textureDataLength);

                // Less performant, because it involves more steps
                // Transforming the texture to raw texture data. We cannot enqueue the Texture2D directly,
                // since that type is not preserved in a queue (or a list)
                //tempTexture2D.LoadRawTextureData(request.GetData<uint>());
                //rawTextureList.Add(tempTexture2D.GetRawTextureData());

                frameNumber++;
            }
        }


        // Final encoding step
        public void GenerateVideo()
        {
            // Only generate video if it wasn't already generated previously for this match
            if (!isEncodedVideoFinished)
            {
                // Cancel any previous job that might be still running
                CancelGenerateVideo();
                if (frameNumber > 0)
                {
                    Logging.Omigari("Video Encoding Started");
                    Logging.Omigari("Total frames recorded: " + frameNumber);
                    if (System.IO.Directory.Exists(screenCapturesPath))
                    {
                        // Remove directory and its content if there was one previously
                        System.IO.Directory.Delete(screenCapturesPath, true);
                    }
                    System.IO.Directory.CreateDirectory(screenCapturesPath);

                    // Encode video and audio in a different thread, since it is a heavy task
                    // Kill the encoder thread if it was running from a previous execution
                    if (encoderThread != null && (threadIsProcessing || encoderThread.IsAlive))
                    {
                        threadIsProcessing = false;
                        encoderThread.Interrupt();
                    }
                    // Start a new encoder thread
                    threadIsProcessing = true;
                    encoderThread = new Thread(EncodeAudioVideo);
                    encoderThread.Start();

                }
                else
                {
                    encodingFinished.Fire(false);
                }
            }
            else
            {
                encodingFinished.Fire(true);
            }
        }

        private void EncodeAudioVideo()
        {

            // Save Screen Captures to disk
            for (int i = 0; i < frameNumber; i++)
            {
                // Generate file path
                string path = screenCapturesPath + "/frame" + GetFrameNumberString(i) + ".jpg";

                // Encoding texture data previously gathered in the big NativeArray
                using var encoded = ImageConversion.EncodeNativeArrayToJPG(_rawTextureData.GetSubArray(i * _textureDataLength, _textureDataLength)
                    , _graphicsFormat, (uint)_frameWidth, (uint)_frameHeight);
                // Writing in disk
                File.WriteAllBytes(path, encoded.ToArray());

                // Similar performance, but EncodeToJPG must be run in the main thread
                // Load the previously enqueed raw texture in a Texture2D
                //tempTexture2D.LoadRawTextureData(_rawTextureList.GetSubArray(i * _textureDataLength, _textureDataLength));
                //tempTexture2D.Apply();
                //// Writing in disk
                //File.WriteAllBytes(path, tempTexture2D.EncodeToJPG());

                // Less performant (with async await)
                //var stream = new FileStream(path, FileMode.OpenOrCreate);
                //var bytes = tempTexture2D.EncodeToJPG();
                //await stream.WriteAsync(bytes, 0, bytes.Length);
            }

            // We have to attach the thread to the Android Java Native Interface since we will be 
            // doing Java calls to the FFmpegKit plugin
            AndroidJNI.AttachCurrentThread();

            // Create video with ffmpeg
            float correctedFrameRate = (float)frameNumber / (float)_recordingDuration;
            var input = screenCapturesPath + "/frame%4d.jpg";

            // Ffmpeg command to convert the audio and screen captures to video
            // Gets a NumberFormatInfo associated with the en-US culture, to force "." as decimal separator
            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

            var command = "-r " + correctedFrameRate.ToString("N", nfi) + " -f image2 -i " + input + " -i " + audioCapturePath  // audio and video input
            + " -vf \"fade=t=out:st=" + ((float)_recordingDuration - 0.5).ToString("N", nfi) + ":d=0.6\" -af \"afade=t=out:st=" + ((float)_recordingDuration - 0.5).ToString("N", nfi) + // End Fading Effect
            ":d=0.6\"  -c:v copy -c:a aac -vcodec libx264 -preset ultrafast -crf 21 -pix_fmt yuv420p " + videoOutputPath; // rest of parameters
            // Call Ffmpegkit command (wrapper to call the methods of the FFmpegKit plugin)
            int returnCode = FFmpegKitWrapper.Execute(command);

            // Detaching the thread
            AndroidJNI.DetachCurrentThread();

            if (returnCode == 0)
            {
                Logging.Omigari("Video encoded succesfully");
                // Executing from Main (UI) Thread, otherwise, the UI event doesn't arrive
                UnityMainThreadDispatcher.Instance().Enqueue(() => encodingFinished.Fire(true));
                // Setting the following variable to true we know that there is a video already encoded for this match,
                // so we don't need to encode it again. It will be reset when a new match starts.
                isEncodedVideoFinished = true;
                // Delete Audio
                System.IO.File.Delete(audioCapturePath);
            }
            else
            {
                Logging.Omigari("There was an error encoding the video");
                // Executing from Main (UI) Thread, otherwise, the UI event doesn't arrive
                UnityMainThreadDispatcher.Instance().Enqueue(() => encodingFinished.Fire(false));
            }

            threadIsProcessing = false;

        }

        public void CancelGenerateVideo()
        {
            if (encoderThread != null && threadIsProcessing)
            {
                encoderThread.Abort();
                //encoderThread.Interrupt();

                Logging.Omigari("Video Generation Interrupted");
            }
        }


        // Max of 9999 frames (around 5m33s with 30fps)
        private string GetFrameNumberString(int frameNumber)
        {
            if (frameNumber < 10)
            {
                return "000" + frameNumber;
            }
            else if (frameNumber < 100)
            {
                return "00" + frameNumber;
            }
            else if (frameNumber < 1000)
            {
                return "0" + frameNumber;
            }
            else
            {
                return frameNumber.ToString();
            }
        }

        private void OnDestroy()
        {
            // Destroy textures to avoid leakage
            Destroy(tempRenderTexture);
            Destroy(tempReducedRenderTexture);
            _rawTextureData.Dispose();

            // Destroy disposables
            disposables.Clear();
        }

    }
}