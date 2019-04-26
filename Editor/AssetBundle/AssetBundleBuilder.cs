using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ABSystem
{
    public class AssetBundleBuilder : ABBuilder
    {
        bool _isToSign = false;
        public AssetBundleBuilder(AssetBundlePathResolver resolver, bool isToSign = false)
            : base(resolver)
        {
            _isToSign = isToSign;
        }

        public override void Export()
        {
            base.Export();

            List<AssetBundleBuild> list = new List<AssetBundleBuild>();
            //标记所有 asset bundle name
            var all = AssetBundleUtils.GetAll();
            if(_isToSign) {
                // YAO: 不依赖Unity自动保证资源完整性，给所有资源手动分组
                for (int i = 0; i < all.Count; i++) {
                    AssetTarget target = all[i];
                    if (target.needSelfExport) {
                        AssetImporter asset = AssetImporter.GetAtPath(target.assetPath);
                        Debug.Log(asset.assetBundleName);
                        if(asset.assetBundleName == "") {
                            asset.assetBundleName = target.bundleName;
                            asset.assetBundleVariant = "";
                            Debug.Log(target.bundleShortName + " -> " + target.bundleName);
                        }
                    } else {
                        AssetTarget root = null;
                        HashSet<AssetTarget> rootSet = new HashSet<AssetTarget>();
                        target.GetRoot(rootSet);
                        if(rootSet.Count == 1) {
                            foreach(AssetTarget dep in rootSet) {
                                root = dep;
                            }
                        } else {
                            Debug.LogError(target.assetPath + " rootSet.Count != 1");
                        }
                        AssetImporter asset = AssetImporter.GetAtPath(target.assetPath);
                        if(asset.assetBundleName == "") {
                            asset.assetBundleName = root.bundleName;
                            asset.assetBundleVariant = "";
                            Debug.Log(target.bundleShortName + " -> " + root.bundleShortName);
                        }
                    }
                }
                AssetDatabase.Refresh();
                BuildPipeline.BuildAssetBundles(pathResolver.BundleSavePath, BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
            } else {
                for (int i = 0; i < all.Count; i++) {
                    AssetTarget target = all[i];
                    if (target.needSelfExport)
                    {
                        AssetBundleBuild build = new AssetBundleBuild();
                        build.assetBundleName = target.bundleName;
                        build.assetNames = new string[] { target.assetPath };
                        list.Add(build);
                        AssetImporter asset = AssetImporter.GetAtPath(target.assetPath);
                        asset.assetBundleName = target.bundleName;
                        asset.assetBundleVariant = "";
                        Debug.Log(target.bundleShortName + " -> " + target.bundleName);
                    }
                }
                BuildPipeline.BuildAssetBundles(pathResolver.BundleSavePath, list.ToArray(), BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
            }

            AssetBundle ab = AssetBundle.LoadFromFile(pathResolver.BundleSavePath + "/AssetBundles");
            
            AssetBundleManifest manifest = ab.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
            //hash
            for (int i = 0; i < all.Count; i++)
            {
                AssetTarget target = all[i];
                if (target.needSelfExport)
                {
                    Hash128 hash = manifest.GetAssetBundleHash(target.bundleName);
                    target.bundleCrc = hash.ToString();
                }
            }
            this.SaveDepAll(all);
            ab.Unload(true);
            this.RemoveUnused(all);

            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();

            // 处理lua
            FileUtil.DeleteFileOrDirectory(pathResolver.LuacSavePath);
            // TODO：加密
            FileUtil.CopyFileOrDirectory(AppConfigs.AssetsExportPath + "/" + AppConfigs.ScriptPath, pathResolver.LuacSavePath);
        }
    }
}