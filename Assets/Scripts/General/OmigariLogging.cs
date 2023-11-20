using System.Diagnostics;

// Use ENABLE_LOG, ENABLE_HP_LOG and ARDK_DEBUG on Player/OtherSettings/Script Compilation
namespace Bercetech.Games.Fleepas
{
    public static class Logging
    {
        [Conditional("ENABLE_LOG")]
        static public void Omigari(object message)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            UnityEngine.Debug.Log(message);
#endif
        }
        [Conditional("ENABLE_LOG")]
        static public void OmigariFormat(string message, params object[] args)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            UnityEngine.Debug.LogFormat(message, args);
#endif
        }
        [Conditional("ENABLE_HP_LOG")]
        static public void OmigariHP(object message)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            UnityEngine.Debug.Log(message);
#endif
        }
    }
}
