#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// AddressableProfile Path

namespace ProjectM
{
    public class AddressableWrapper
    {
        public static string remotePath
        {
            get
            {
                string path = GlobalManager.Instance.cdnUrl;
                Debug.Log($"[Debug] runtime path : {path}");
                return path;
            }
        }


        public static string catalogVersion
        {
            get
            {
                #if UNITY_EDITOR || UNITY_EDITOR_OSX
                var rv = AppConfig.Instance.rv;
                Debug.Log($"[Debug] Rv : {rv}");
                return rv;
                #else
                var rv = AppConfig.Instance.rv;
                Debug.Log($"[Debug] Rv : {rv}");
                return rv;
                #endif
                
            }
        }

    }

}