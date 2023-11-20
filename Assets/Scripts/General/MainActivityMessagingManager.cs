using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Bercetech.Games.Fleepas
{
    public static class MainActivityMessagingManager
    {

        public enum unityTimeMessages
        {
            SAVE_SESSION,
            SESSION_ENABLED_FINISHED,
            SESSION_NOT_ENABLED_FINISHED,
        };

        public static void SendSessionDataToMainActivity(unityTimeMessages messageType, string concatPlayerUIds = "", int numberActivePlayers = 0)
        {
        #if UNITY_ANDROID
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.bercetech.fleepas.unity.OverrideUnityActivity");
                AndroidJavaObject overrideActivity = jc.GetStatic<AndroidJavaObject>("instance");
                switch (messageType)
                {
                    case unityTimeMessages.SAVE_SESSION:
                        // Last parameter means if it is also the last update before closing the Unity Activity
                        overrideActivity.Call("saveSessionData", Time.realtimeSinceStartup, concatPlayerUIds, numberActivePlayers, false);
                        break;
                    case unityTimeMessages.SESSION_ENABLED_FINISHED:
                        // Last parameter means if it is also the last update before closing the Unity Activity
                        overrideActivity.Call("saveSessionData", Time.realtimeSinceStartup, concatPlayerUIds, numberActivePlayers, true);
                        break;
                    case unityTimeMessages.SESSION_NOT_ENABLED_FINISHED:
                        // Last parameter means if it is also the last update before closing the Unity Activity. In this case session duration is 0.
                        overrideActivity.Call("saveSessionData", 0.0f, concatPlayerUIds, numberActivePlayers, true);
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.OmigariHP("Exception when sending session data message from Unity to main unity activity: " + e.Message);
            }
        #elif UNITY_IOS || UNITY_TVOS
            NativeAPI.saveSessionDuration(ARSessionId, Time.realtimeSinceStartup);
        #endif
        }



        public static void SendMatchDataToMainActivity(int matchId, string userUid, string userName, string userSessionId, int score,
            bool isPlayer, bool isFinished, int ranking, int fleepPoints, int matchRound, bool roundStarted, bool roundFinished, bool bestMatch)
        {
#if UNITY_ANDROID
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.bercetech.fleepas.unity.MainUnityActivity");
                AndroidJavaObject overrideActivity = jc.GetStatic<AndroidJavaObject>("instance");

                // Transform userUid to null if necessary
                var userUidAndroid = userUid;
                if (userUid == "null") userUidAndroid = null;

                object[] messageParams = new object[13];
                messageParams[0] = matchId;
                messageParams[1] = userUidAndroid;
                messageParams[2] = userName;
                messageParams[3] = userSessionId;
                messageParams[4] = score;
                messageParams[5] = isPlayer;
                messageParams[6] = isFinished;
                messageParams[7] = ranking;
                messageParams[8] = fleepPoints;
                messageParams[9] = matchRound;
                messageParams[10] = roundStarted;
                messageParams[11] = roundFinished;
                messageParams[12] = bestMatch;

                overrideActivity.Call("saveMatchData", messageParams);

                // This way is also valid:
                //overrideActivity.Call("saveMatchData", matchId, userUid, userName, score, isPlaying, isFinished);

                //// In order to be able to send a null in the score value (to distinguish if the player is currently playing a match)
                //// I tried to use integer params like this:
                //AndroidJavaObject scoreJava = new AndroidJavaObject("java.lang.Integer", score);
                //// And like this
                //AndroidJavaClass integerClass = new AndroidJavaClass("java.lang.Integer");
                //AndroidJavaObject scoreJava = integerClass.CallStatic<AndroidJavaObject>("valueOf", score);
                //// Or this (with helper function)
                //ToAndroidInteger(score);
                //// But all of them gave the same signature error (not sure why and didn't find a solution on internet)
                //// Finally, I send 0s + another variable telling if the player is playing or not
            }
            catch (Exception e)
            {
                Logging.OmigariHP("Exception when sending match data message from Unity to main unity activity: " + e.Message);
            }
            #elif UNITY_IOS || UNITY_TVOS
                NativeAPI.saveSessionDuration(ARSessionId, Time.realtimeSinceStartup);
            #endif
        }

        public static void GetUserFleepSiteRanking(int score)
        {
#if UNITY_ANDROID
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.bercetech.fleepas.unity.MainUnityActivity");
                AndroidJavaObject overrideActivity = jc.GetStatic<AndroidJavaObject>("instance");

                object[] messageParams = new object[1];
                messageParams[0] = score;
                overrideActivity.Call("getUserFleepSiteRanking", messageParams);
            }
            catch (Exception e)
            {
                Logging.OmigariHP("Exception when getting user FleepSite Ranking: " + e.Message);
            }
#elif UNITY_IOS || UNITY_TVOS
                NativeAPI.saveSessionDuration(ARSessionId, Time.realtimeSinceStartup);
#endif
        }

        public static void CheckFleepSitePrizeLimitReached(string prizeId, int maxNumber)
        {
#if UNITY_ANDROID
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.bercetech.fleepas.unity.MainUnityActivity");
                AndroidJavaObject overrideActivity = jc.GetStatic<AndroidJavaObject>("instance");

                object[] messageParams = new object[2];
                messageParams[0] = prizeId;
                messageParams[1] = maxNumber;
                overrideActivity.Call("checkFleepSitePrizeLimitReached", messageParams);
            }
            catch (Exception e)
            {
                Logging.OmigariHP("Exception when getting user FleepSite Prize Limit: " + e.Message);
            }
#elif UNITY_IOS || UNITY_TVOS
                NativeAPI.saveSessionDuration(ARSessionId, Time.realtimeSinceStartup);
#endif
        }

        public static void CreateSingleMeshFleepSite(string compressedSerializedMeshPath, sbyte[] serializedMeshLocalPose, string screenShotPath,
            string cloudAnchorId, string fleepSiteTitle, bool isPublic, int daysToExpire, int anchorLifeTime)
        {
#if UNITY_ANDROID
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.bercetech.fleepas.unity.OverrideUnityActivity");
                AndroidJavaObject overrideActivity = jc.GetStatic<AndroidJavaObject>("instance");

                object[] messageParams = new object[8];
                messageParams[0] = compressedSerializedMeshPath;
                messageParams[1] = serializedMeshLocalPose;
                messageParams[2] = screenShotPath;
                messageParams[3] = cloudAnchorId;
                messageParams[4] = fleepSiteTitle;
                messageParams[5] = isPublic;
                messageParams[6] = daysToExpire;
                messageParams[7] = anchorLifeTime;

                overrideActivity.Call("createSingleMeshFleepSite", messageParams);
            }
            catch (Exception e)
            {
                Logging.OmigariHP("Exception when sending message from Unity to main unity activity to create single mesh FleepSite: " + e.Message);
            }
#elif UNITY_IOS || UNITY_TVOS
                NativeAPI.saveSessionDuration(ARSessionId, Time.realtimeSinceStartup);
#endif
        }


        public static void GetFleepSiteData(string fleepSiteId)
        {
#if UNITY_ANDROID
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.bercetech.fleepas.unity.OverrideUnityActivity");
                AndroidJavaObject overrideActivity = jc.GetStatic<AndroidJavaObject>("instance");

                overrideActivity.Call("getFleepSiteCloudAnchorData", fleepSiteId);
            }
            catch (Exception e)
            {
                Logging.OmigariHP("Exception when sending message from Unity to main unity activity to get FleepSite data" + e.Message);
            }
#elif UNITY_IOS || UNITY_TVOS
                NativeAPI.saveSessionDuration(ARSessionId, Time.realtimeSinceStartup);
#endif
        }

        public static void GetDownloadUrl(string urlPath)
        {
#if UNITY_ANDROID
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.bercetech.fleepas.unity.OverrideUnityActivity");
                AndroidJavaObject overrideActivity = jc.GetStatic<AndroidJavaObject>("instance");

                overrideActivity.Call("getCloudStorageDownloadUrl", urlPath);
            }
            catch (Exception e)
            {
                Logging.OmigariHP("Exception when sending message from Unity to main unity activity to get Download Url" + e.Message);
            }
#elif UNITY_IOS || UNITY_TVOS
                NativeAPI.saveSessionDuration(ARSessionId, Time.realtimeSinceStartup);
#endif
        }

        public static void SendGooglePlayReviewData(bool userHasAnswered, bool userGaveReview)
        {
#if UNITY_ANDROID
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.bercetech.fleepas.unity.OverrideUnityActivity");
                AndroidJavaObject overrideActivity = jc.GetStatic<AndroidJavaObject>("instance");

                object[] messageParams = new object[2];
                messageParams[0] = userHasAnswered;
                messageParams[1] = userGaveReview;
                overrideActivity.Call("sendGooglePlayReviewData", messageParams);

            }
            catch (Exception e)
            {
                Logging.OmigariHP("Exception when sending Google Play Review from Unity to main unity activity" + e.Message);
            }
#elif UNITY_IOS || UNITY_TVOS
                NativeAPI.saveSessionDuration(ARSessionId, Time.realtimeSinceStartup);
#endif
        }

        // Helper Class from:
        // https://github.com/yandexmobile/metrica-plugin-unity/blob/master/YandexMetricaPluginSample/Assets/AppMetrica/YandexAppMetricaAndroid.cs
        public static AndroidJavaObject ToAndroidInteger(this int? self)
        {
            AndroidJavaObject integer = null;
            if (self.HasValue)
            {
                using (var integerClass = new AndroidJavaClass("java.lang.Integer"))
                {
                    integer = integerClass.CallStatic<AndroidJavaObject>("valueOf", self);
                }
            }
            return integer;
        }


    }
}