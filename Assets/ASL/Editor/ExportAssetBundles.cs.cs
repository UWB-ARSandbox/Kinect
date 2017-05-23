using UnityEditor;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        BuildPipeline.BuildAssetBundles("Assets/ASL/StreamingAssets/AssetBundlesPC", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        BuildPipeline.BuildAssetBundles("Assets/ASL/StreamingAssets/AssetBundlesAndroid", BuildAssetBundleOptions.None, BuildTarget.Android);
        //BuildPipeline.BuildAssetBundles("Assets/ASL/StreamingAssets/AssetBundlesHololens", BuildAssetBundleOptions.None, BuildTarget.WSAPlayer);
    }
}
