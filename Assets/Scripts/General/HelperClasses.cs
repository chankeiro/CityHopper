using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using Bercetech.Games.Fleepas.BinarySerialization;
using UnityEngine.Localization;



namespace Bercetech.Games.Fleepas
{

    public class PlayerData
    {
        public string Uid;
        public string Name;
        public string UserSessionId;
        public float Score;
        public bool IsHost { get; private set; }
        public bool IsYou { get; private set; }
        public bool PlayedLastGame;
        public TimeSpan LastTimeAlive;
        public GameObject NameSign;
        public bool ScoreBonus;
        public int LastShowtWithoutHit;
        public int HitsInARow;

        public PlayerData(string uid, string name, string userSessionId, float score, bool isHost,
            bool isYou, bool playedLastGame, TimeSpan lastTimeAlive, GameObject nameSign, bool scoreBonus, int lastShowtWithoutHit)
        {
            Uid = uid;
            Name = name;
            UserSessionId = userSessionId; // Fleepas DB Session Entry of each user, not the AR Session Id, although coincident with the latter for host players
            Score = score;
            IsYou = isYou;
            IsHost = isHost;
            PlayedLastGame = playedLastGame;
            LastTimeAlive = lastTimeAlive;
            NameSign = nameSign;
            ScoreBonus = scoreBonus;
            LastShowtWithoutHit = lastShowtWithoutHit;
        }
    }

    public class PaintHit
    {
        public Vector4 PaintHitPosition;
        public Vector4 PaintHitNormal;
        public Color PaintHitColor;
        public float PaintHitSize;
        public float PaintHitInitialTime;
        public PaintHit(Vector4 paintHitPosition, Vector4 paintHitNormal, Color paintHitColor, float paintHitSize, float paintHitInitialTime)
        {
            PaintHitPosition = paintHitPosition;
            PaintHitNormal = paintHitNormal;
            PaintHitColor = paintHitColor;
            PaintHitSize = paintHitSize;
            PaintHitInitialTime = paintHitInitialTime;
        }
    }

    public class FleepasCloudMesh
    {
        public byte[] MeshData;
        public Pose MeshPose; // Local Pose vs cloud anchor
        public string CloudAnchorId; 
      
        public FleepasCloudMesh(byte[] meshData, Pose meshPose, string cloudAnchorId)
        {
            MeshData = meshData;
            MeshPose = meshPose;
            CloudAnchorId = cloudAnchorId;
        }
    }

    public class FleepasFormats
    {
        public static String DateFormat = "dd / MM / yyyy";
    }

    public class FleepSitePrize
    {
        public string PrizeId;
        public string Description;
        public int MinPoints;
        public int MinPosition;
        public int MaxPosition;
        public int MaxNumber;
        public int PrizesLeft;
        public FleepSitePrize(string prizeId, string description, int minPoints, int minPosition, int maxPosition, int maxNumber, int prizesLeft)
        {
            PrizeId = prizeId;
            Description = description;
            MinPoints = minPoints;
            MinPosition = minPosition;
            MaxPosition = maxPosition;
            MaxNumber = maxNumber;
            PrizesLeft = prizesLeft;
        }
    }

    public class HelperFunctions
    {
        public static IEnumerator DisableAfterPlay(GameObject gameObject, bool onlyWaitForSound = false)
        {
            if (!onlyWaitForSound)
            {
                // Stop rendering so it cannot be seen until destroyed
                var renderers = gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {
                    rend.enabled = false;
                }
                var colliders = gameObject.GetComponentsInChildren<Collider>();
                foreach (Collider coll in colliders)
                {
                    coll.enabled = false;
                }
            }
            // Wait for the sound to finish
            while (gameObject.GetComponent<AudioSource>().isPlaying)
            {
                yield return new WaitForSeconds(.2f); // Check only 5 times per second, to reduce overhead
            }
            gameObject.SetActive(false);
        }

        // Funtion to check internet connection
        public static IEnumerator CheckNetworkError(float secondsBetweenChecks, Action<Boolean> IsNetworkError) // Providing response as a callback
        {
            // URL to request
            var url = "";
            if (PlayerMode.SharedInstance.IsDebug)
                url = "https://storage.cloud.google.com/fleepas-dev.appspot.com/dummy.txt";
            else
                url = "https://storage.cloud.google.com/fleepas-4cc61.appspot.com/dummy.txt";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // Request and wait for the desired page.
                yield return new WaitForSeconds(secondsBetweenChecks); // Wait a short time before recheck
                yield return webRequest.SendWebRequest();
                // Checking Internet Connection error
                if (webRequest.error != null) Logging.Omigari("Network Error: " + webRequest.error);
                IsNetworkError(webRequest.error != null);
            }
        }

        public static byte[] SerializePose(Pose pose)
        {
            using (var stream = new MemoryStream())
            {
                using (var serializer = new BinarySerializer(stream))
                {
                    serializer.Serialize(pose.position);
                    serializer.Serialize(pose.rotation);
                    return stream.ToArray();
                }
            }
        }

        public static Pose DeserializePose(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                using (var deserializer = new BinaryDeserializer(stream))
                {
                    Pose pose;
                    pose.position = (Vector3)deserializer.Deserialize(); //Position
                    pose.rotation = (Quaternion)deserializer.Deserialize(); //Rotation

                    return pose;
                }
            }
        }

        public static sbyte[] ByteArrayToSbyteArray(byte[] ba)
        {
            return Array.ConvertAll(ba, b => unchecked((sbyte)b));
        }

        public static byte[] HexStringToByteArray(string hexS)
        {
            // Defining length of signed byte array
            sbyte[] sBA = new sbyte[hexS.Length / 2];
            // Two characters of the string correspond to one sByte
            for (int i = 0; i < hexS.Length / 2; ++i)
                sBA[i] = Convert.ToSByte(hexS[i * 2].ToString() + hexS[i * 2 + 1], 16);
            // From signed byte array to byte array
            return (byte[])(Array)sBA;
        }

        public static IEnumerator LoadFileFromUri(string fileUri, Action<byte[]> GetFile)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(fileUri))
            {
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Logging.OmigariHP(uwr.error);
                    if (GetFile != null)
                    {
                        GetFile(null);
                    }
                }
                else
                {
                    // Get downloaded asset 
                    if (GetFile != null)
                    {
                        // Byte array representation of the downloaded file
                        GetFile(uwr.downloadHandler.data);
                    }
                }
            }
        }

        public static void SendMail(string emailAddress, string emailSubject, string emailBody)
        {
            string address = emailAddress;

            string subject = MyEscapeURL(emailSubject);

            string body = MyEscapeURL(emailBody);

            Application.OpenURL("mailto:" + address + "?subject=" + subject + "&body=" + body);

        }

        public static string MyEscapeURL(string url)
        {
            return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
        }


        // Use it to prevent double clicks on buttons
        public static IEnumerator SpaceButtonClick(Action finishWait)
        {
            yield return new WaitForSeconds(0.2f);
            finishWait();
        }

        public static IEnumerator Wait(float secondsToWait, Action finishWait)
        {
            yield return new WaitForSeconds(secondsToWait);
            finishWait();
        }

        public static Vector4 Vector3ToVector4(Vector3 vector3)
        {
            return new Vector4(vector3.x, vector3.y, vector3.z, 0);
        }

        // Variable Strings
        private static LocalizedString _1stString = new LocalizedString("000 - Fleepas", "1st");
        private static LocalizedString _1stFemString = new LocalizedString("000 - Fleepas", "1st_fem");
        private static LocalizedString _2ndString = new LocalizedString("000 - Fleepas", "2nd");
        private static LocalizedString _2ndFemString = new LocalizedString("000 - Fleepas", "2nd_fem");
        private static LocalizedString _3rdString = new LocalizedString("000 - Fleepas", "3rd");
        private static LocalizedString _3rdFemString = new LocalizedString("000 - Fleepas", "3rd_fem");
        private static LocalizedString _thString = new LocalizedString("000 - Fleepas", "th");
        private static LocalizedString _thFemString = new LocalizedString("000 - Fleepas", "th_fem");
        public static string GetOrdinalPosition(int number, bool femenine = false)
        {
            if (number == 1)
            {
                if (femenine)
                    return _1stFemString.GetLocalizedString();
                else
                    return _1stString.GetLocalizedString();
            }
            else if (number == 2)
            {
                if (femenine)
                    return _2ndFemString.GetLocalizedString();
                else
                    return _2ndString.GetLocalizedString();
            }
            else if (number == 3)
            {
                if (femenine)
                    return _3rdFemString.GetLocalizedString();
                else
                    return _3rdString.GetLocalizedString();
            }
            else
            {
                if (femenine)
                    return number + _thFemString.GetLocalizedString();
                else
                    return number + _thString.GetLocalizedString();
            }

        }

    }

}
