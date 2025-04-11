#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using AppleAuth.Editor;

public class PostBuildProcessor
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            var projectPath = PBXProject.GetPBXProjectPath(path);
        
            // Adds entitlement depending on the Unity version used
#if UNITY_2019_3_OR_NEWER
            var project = new PBXProject();
            project.ReadFromString(System.IO.File.ReadAllText(projectPath));
            var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null, project.GetUnityMainTargetGuid());
            manager.AddSignInWithAppleWithCompatibility(project.GetUnityFrameworkTargetGuid());
            manager.WriteToFile();
#else
            var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", PBXProject.GetUnityTargetName());
            manager.AddSignInWithAppleWithCompatibility();
            manager.WriteToFile();
#endif
            
            // Path to Info.plist
            string plistPath = Path.Combine(path, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            // Get or create CFBundleURLTypes array
            PlistElementArray bundleUrlTypes = plist.root.CreateArray("CFBundleURLTypes");

            // Add a dictionary for Google URL scheme
            PlistElementDict dict = bundleUrlTypes.AddDict();
            PlistElementArray urlSchemes = dict.CreateArray("CFBundleURLSchemes");
            urlSchemes.AddString("com.googleusercontent.apps.1031285816702-lg7erc8qr173bmbp6cbdd501j52571hk"); // Adjust this for your project
            // Add GIDClientID key
            plist.root.SetString("GIDClientID", "1031285816702-lg7erc8qr173bmbp6cbdd501j52571hk.apps.googleusercontent.com"); // Adjust this too

            // Write back to Info.plist
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif