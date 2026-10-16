using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DxBuild
{
    /// <summary>
    /// Custom GameCI build entry point. Invoked with:
    ///   -customBuildPath &lt;path&gt; -buildTarget Android -projectPath &lt;...&gt;
    /// Sets all PlayerSettings programmatically so the repo needs no fragile
    /// hand-written ProjectSettings.asset, and creates an empty scene at build
    /// time so no scene YAML is committed either.
    /// </summary>
    public static class BuildScript
    {
        const string Company = "Dx";
        const string Product = "OPD Chaos Simulator";
        const string BundleId = "com.dx.hospitalsimulator";

        public static void Build()
        {
            var args = ParseArgs();

            string targetName = args.TryGetValue("buildTarget", out var bt) ? bt : "Android";
            string outPath = args.TryGetValue("customBuildPath", out var op)
                ? op
                : "build/Android/dxhospital.apk";

            BuildTarget target;
            try
            {
                target = (BuildTarget)Enum.Parse(typeof(BuildTarget), targetName);
            }
            catch
            {
                Debug.LogError($"Unknown buildTarget: {targetName}");
                EditorApplication.Exit(120);
                return;
            }

            ApplySettings(target);
            EnsureStandardShaderIncluded();

            var dir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            // Deterministic empty scene — the game is constructed in code at
            // runtime via [RuntimeInitializeOnLoadMethod]. No scene assets needed.
            const string scenePath = "Assets/_BuildScene.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { scenePath },
                target = target,
                locationPathName = outPath,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            Debug.Log($"Build result={s.result} size={s.totalSize} bytes errors={s.totalErrors} warnings={s.totalWarnings}");

            EditorApplication.Exit(s.result == BuildResult.Succeeded ? 0 : 1);
        }

        static void ApplySettings(BuildTarget target)
        {
            PlayerSettings.companyName = Company;
            PlayerSettings.productName = Product;
            PlayerSettings.bundleVersion = "0.1.0";

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            if (target == BuildTarget.Android)
            {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, BundleId);
                PlayerSettings.Android.bundleVersionCode = 1;
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
                // No custom keystore -> Unity signs with the debug key (installable now).
                PlayerSettings.Android.useCustomKeystore = false;
                // Faster, simpler builds for early iteration.
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, true);
            }

            Debug.Log($"PlayerSettings applied for {target}.");
        }

        /// <summary>
        /// Materials are created at runtime, so the Standard shader has no asset
        /// referencing it and would be stripped from the build (pink objects).
        /// Force it into "Always Included Shaders".
        /// </summary>
        static void EnsureStandardShaderIncluded()
        {
            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                Debug.LogWarning("Standard shader not found; runtime materials may render incorrectly.");
                return;
            }

            var gs = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("ProjectSettings/GraphicsSettings.asset");
            if (gs == null) return;

            var so = new SerializedObject(gs);
            var arr = so.FindProperty("m_AlwaysIncludedShaders");
            if (arr == null) return;

            for (int i = 0; i < arr.arraySize; i++)
                if (arr.GetArrayElementAtIndex(i).objectReferenceValue == shader) return;

            arr.InsertArrayElementAtIndex(arr.arraySize);
            arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = shader;
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("Added Standard shader to Always Included Shaders.");
        }

        static Dictionary<string, string> ParseArgs()
        {
            var result = new Dictionary<string, string>();
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith("-")) continue;
                string key = args[i].TrimStart('-');
                bool hasValue = i + 1 < args.Length && !args[i + 1].StartsWith("-");
                result[key] = hasValue ? args[i + 1].TrimStart('-') : "";
            }
            return result;
        }
    }
}
