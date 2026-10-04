#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Reconstructed.CloudBuild
{
    /// <summary>Unity Build Automation entry point. Refuses to substitute missing recovered scenes.</summary>
    public static class BuildAndroid
    {
        private const string PackageId = "com.hoptimistgames.jujutsukaisenfightinggame";
        private const string RequiredUnity = "2022.3.62f3";
        private const string SceneConfigPath = "Assets/CloudBuild/RecoveredBuildScenes.json";

        [Serializable] private sealed class SceneConfig { public string unityVersion; public List<string> scenes; }

        public static void PerformBuild()
        {
            ValidateEditor();
            string[] scenes = LoadAndValidateScenes();
            ConfigureAndroid();

            string output = Environment.GetEnvironmentVariable("BUILD_PATH");
            if (string.IsNullOrEmpty(output)) output = "Builds/Jujutsu_Endless_Upgraded.apk";
            string absoluteOutput = Path.GetFullPath(output);
            Directory.CreateDirectory(Path.GetDirectoryName(absoluteOutput));

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = absoluteOutput,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.StrictMode
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Android build failed: " + report.summary.result + ". See Unity build log.");
            Debug.Log("ANDROID_BUILD_OUTPUT=" + absoluteOutput);
        }

        private static void ValidateEditor()
        {
            if (!Application.unityVersion.StartsWith(RequiredUnity, StringComparison.Ordinal))
                throw new BuildFailedException("Wrong Unity version. Required " + RequiredUnity + "; running " + Application.unityVersion + ".");
            if (!File.Exists(Path.Combine(Application.dataPath, "CloudBuild/RecoveredBuildScenes.json")))
                throw new BuildFailedException("Missing recovered scene manifest: " + SceneConfigPath);
        }

        private static string[] LoadAndValidateScenes()
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(SceneConfigPath);
            if (asset == null) throw new BuildFailedException("Cannot load " + SceneConfigPath + ".");
            SceneConfig config = JsonUtility.FromJson<SceneConfig>(asset.text);
            if (config == null || config.scenes == null || config.scenes.Count == 0)
                throw new BuildFailedException("Recovered scene manifest contains no scenes.");
            var result = new List<string>(config.scenes.Count);
            foreach (string scene in config.scenes)
            {
                if (string.IsNullOrEmpty(scene) || !scene.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                    throw new BuildFailedException("Invalid recovered scene path: " + scene);
                if (!File.Exists(Path.Combine(Directory.GetParent(Application.dataPath).FullName, scene)))
                {
                    Debug.LogWarning("Recovered scene is not present; skipping it: " + scene);
                    continue;
                }
                result.Add(scene);
            }
            if (result.Count == 0) throw new BuildFailedException("No build scenes were found in the reconstructed project.");
            return result.ToArray();
        }

        private static void ConfigureAndroid()
        {
            PlayerSettings.productName = "Jujutsu Kaisen Fighting Game - Endless";
            PlayerSettings.companyName = "Hoptimist Games";
            PlayerSettings.bundleVersion = "0.8-endless";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageId);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.connectProfiler = false;
        }
    }
}
#endif
