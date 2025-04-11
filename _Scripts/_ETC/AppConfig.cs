using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace ProjectM
{
    public enum ServerType
    {
        Dev, Stage, Live, Review
    }
    [GlobalConfig("Assets/Resources/Codberg/")]
    public class AppConfig : GlobalConfig<AppConfig>
    {
        public ServerType Server;
        public List<string> domain;
        // public List<string> ServerUrl;
        // public List<string> cdnUrl;
        
        public string hash;
        public string androidID;
        public string iosID;
        public string appVersion;
        public string rv;

        public string appsFlyerDevKey;

        public string Domain
        {
            get
            {
                if (Server == ServerType.Dev)
                    return domain[0];
                else if(Server == ServerType.Stage)
                    return domain[1];
                else
                    return domain[2];
                // return Server == ServerType.Dev ? domain[0] : domain[1]; // dev path : beta path
            }
        }

        public bool Development
        {
            get { return Server == ServerType.Dev || Server == ServerType.Stage; }
        }
        
        // public string GetCdnUrl
        // {
        //     get
        //     {
        //         return cdnUrl;
        //         // return Server == ServerType.Live ? cdnUrl[1] : cdnUrl[0]; // live path : beta path
        //     }
        // }
        
        #if UNITY_EDITOR
        [MenuItem("Tools/Codberg/AppConfig")]
        static void Show()
        {
            var settings = LoadAsset<AppConfig>();
        
            EditorGUIUtility.PingObject(settings);
        }

        static T LoadAsset<T>() where T : UnityEngine.Object
        {
            var guid = AssetDatabase.FindAssets( "t:" + typeof (T).Name).FirstOrDefault();
            if ( string .IsNullOrEmpty(guid))
            {
                throw  new System.IO.FileNotFoundException($"자산을 찾을 수 없습니다 {typeof(T).Name}" );
            }

            var filePath = AssetDatabase.GUIDToAssetPath(guid);
            if ( string .IsNullOrEmpty (filePath))
            {
                throw  new System.IO.FileNotFoundException($"자산 경로를 찾을 수 없다 {guid}" );
            }

            return AssetDatabase.LoadAssetAtPath<T>(filePath);
        }
        #endif
    }
}