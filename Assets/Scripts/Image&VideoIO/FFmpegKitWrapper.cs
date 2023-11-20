using UnityEngine;

namespace Bercetech.Games.Fleepas
{
    public class FFmpegKitWrapper
    {
        public static int Execute(string command)
        {
#if UNITY_ANDROID
            using (AndroidJavaClass configClass = new AndroidJavaClass("com.arthenica.ffmpegkit.FFmpegKitConfig"))
            {
                try
                {
                    AndroidJavaObject paramVal = new AndroidJavaClass("com.arthenica.ffmpegkit.Signal").GetStatic<AndroidJavaObject>("SIGXCPU");
                    configClass.CallStatic("ignoreSignal", new object[] { paramVal });

                    using (AndroidJavaClass ffmpeg = new AndroidJavaClass("com.arthenica.ffmpegkit.FFmpegKit"))
                    {
                        var code = ffmpeg.CallStatic<AndroidJavaObject>("execute", command);
                        var returnCode = code.Call<AndroidJavaObject>("getReturnCode");
                        return returnCode.Call<int>("getValue");
                    }
                }
                catch
                {
                    // Returning a code different from 0
                    Logging.OmigariHP("Error while executing ffmpeg");
                    return 999;
                }
            }
#elif UNITY_IOS
        // I didn't try this yet. Check this for the implementation
        // https://sourceexample.com/en/671cf50fddfec13eb5e1/#call-from-c--on-the-unity-side
        return MobileFFmpegIOS.Execute(command);
#else
        Logging.OmigariHP("This plataform is not supported by ffmpegkit");
        return 0;
#endif
        }


        public static int Cancel()
        {
#if UNITY_ANDROID
            using (AndroidJavaClass configClass = new AndroidJavaClass("com.arthenica.ffmpegkit.FFmpegKitConfig"))
            {
                using (AndroidJavaClass ffmpeg = new AndroidJavaClass("com.arthenica.ffmpegkit.FFmpegKit"))
                {
                    int code = ffmpeg.CallStatic<int>("cancel");
                    return code;
                }
            }
#elif UNITY_IOS
        return MobileFFmpegIOS.Cancel();
#else
        Logging.OmigariHP("This plataform is not supported by ffmpegkit");
        return 0;
#endif
        }
    }
}