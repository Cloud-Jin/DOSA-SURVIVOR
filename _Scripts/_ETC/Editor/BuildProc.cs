#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectM;
using UnityEngine;

public class BuildProc
{
    [MenuItem("Tools/MMT/Build/QA/AAA")]
    public static void BuildAndroid_AAA()
    {
        var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup("ui");
        var path = ContentUpdateScript.GetContentStateDataPath(false);
        var result = ContentUpdateScript.BuildContentUpdate(group.Settings, path);
        if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.LogError(result.Error);
        }
    }
    
    // 메뉴에 추가해서 테스트 해볼 수 있게
    [MenuItem("Tools/MMT/Build/Aos")]
    public static void BuildAndroid()
    {
        // 안드로이드용 설정
        PlayerSettings.Android.keystoreName = "Keystore/mammothplay.keystore";
        PlayerSettings.Android.keystorePass = "dkagh2023!!";
        PlayerSettings.Android.keyaliasName = "dosa";
        PlayerSettings.Android.keyaliasPass = "dkagh2023!!";

        PlayerSettings.Android.bundleVersionCode = int.Parse(CommandLineReader.GetCustomArgument("VersionCode"));
        
        bool isAAB = bool.Parse(CommandLineReader.GetCustomArgument("aab"));
        EditorUserBuildSettings.buildAppBundle = isAAB;
        
        // PlayerSettings.
        // BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        // buildPlayerOptions.locationPathName = "build_aos/make_again.apk";
        var timeNow = DateTime.Now.ToString(("MMdd HH:mm:ss"));
        string fileExtension = (isAAB) ? "aab" : "apk";
        GenericSetting();
        GenericBuild(FindEnabledEditorScenes(), $"./Build/Android/Dosa {timeNow}.{fileExtension}", BuildTarget.Android, BuildOptions.None);
    }
    
     // 메뉴에 추가해서 테스트 해볼 수 있게
    [MenuItem("Tools/MMT/Build/Ios")]
    public static void BuildIos()
    {
        PlayerSettings.iOS.buildNumber = CommandLineReader.GetCustomArgument("VersionCode");
       GenericSetting();
       GenericBuild(FindEnabledEditorScenes(), $"./Build/IOS/", BuildTarget.iOS, BuildOptions.None);
    }

    // [MenuItem("Tools/MMT/Build/Profile")]
    // private static void SetProfile()
    // {
    //     EditorUserBuildSettings.buildAppBundle = false;
    //     string profile = "Live";
    //     var settings = AddressableAssetSettingsDefaultObject.Settings;
    //     string profileId = settings.profileSettings.GetProfileId(profile);
    //     if (string.IsNullOrEmpty(profileId))
    //         Debug.LogWarning($"Couldn't find a profile named, {profile}, " + $"using current profile instead.");
    //     else
    //         settings.activeProfileId = profileId;
    // }


    
    private static void GenericBuild(string[] scenes, string targetPath, BuildTarget buildTarget, BuildOptions buildOptions)
    {
        var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
        EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
        BuildPipeline.BuildPlayer(scenes, targetPath, buildTarget, buildOptions);
    }

    static void GenericSetting()
    {
        // CommandLineReader 별도 클래스를 써서, 추가 파라미터를 받아온다.
        // 세팅 코드.
        string server = CommandLineReader.GetCustomArgument("Server");
        AppConfig.Instance.Server = server.ToEnum<ServerType>();
        AppConfig.Instance.rv = CommandLineReader.GetCustomArgument("Rv");
        AddressableAssetSettingsDefaultObject.Settings.OverridePlayerVersion = "Bundle";      // 카탈로그
        PlayerSettings.bundleVersion = CommandLineReader.GetCustomArgument("Version");        // 앱 버전 ( 1.0.0 )
        AppConfig.Instance.appVersion = CommandLineReader.GetCustomArgument("Version");
        
        // 프로필 세팅
        string profile = CommandLineReader.GetCustomArgument("Profile");
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        string profileId = settings.profileSettings.GetProfileId(profile);
        if (string.IsNullOrEmpty(profileId))
            Debug.LogWarning($"Couldn't find a profile named, {profile}, " + $"using current profile instead.");
        else
            settings.activeProfileId = profileId;
        
        AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex = 2;
        AssetDatabase.Refresh();
        // 스플래쉬
        PlayerSettings.SplashScreen.showUnityLogo = false;

        EditorUtility.SetDirty(AppConfig.Instance);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    private static string[] FindEnabledEditorScenes() 
    {
        // 프로젝트 세팅 씬 추가.
        List<string> EditorScenes = new List<string>();
        foreach(var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
                continue;
              
            EditorScenes.Add(scene.path);
        }

        return EditorScenes.ToArray();
    }
}
