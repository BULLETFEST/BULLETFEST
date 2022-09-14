using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;

public class BuildVersionProcessor : IPreprocessBuildWithReport
{
  public int callbackOrder => 0;

  public void OnPreprocessBuild(BuildReport report)
  {
    Version version = new(FindCurrentVersion());
    version.IncreaseVersion();
    PlayerSettings.bundleVersion = version.GetVersionString();
    PlayerSettings.SplashScreen.show = false;

    BuildTargetGroup bt = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

#if UNITY_EDITOR_LINUX
    PlayerSettings.SetScriptingBackend(bt, ScriptingImplementation.IL2CPP);
    PlayerSettings.SetIl2CppCompilerConfiguration(bt, Il2CppCompilerConfiguration.Master);
    PlayerSettings.SetManagedStrippingLevel(bt, ManagedStrippingLevel.High);
    EditorUserBuildSettings.il2CppCodeGeneration = Il2CppCodeGeneration.OptimizeSpeed;
#elif UNITY_EDITOR_WIN
    PlayerSettings.SetScriptingBackend(bt, ScriptingImplementation.Mono2x);
    PlayerSettings.SetIl2CppCompilerConfiguration(bt, Il2CppCompilerConfiguration.Master);
    PlayerSettings.SetManagedStrippingLevel(bt, ManagedStrippingLevel.Disabled);
#endif

#if UNITY_STANDALONE_OSX
    UnityEditor.OSXStandalone.UserBuildSettings.architecture = UnityEditor.OSXStandalone.MacOSArchitecture.x64ARM64;
#endif
  }

  private string FindCurrentVersion()
  {
    return PlayerSettings.bundleVersion;
  }
}
