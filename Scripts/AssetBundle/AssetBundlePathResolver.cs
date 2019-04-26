using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ABSystem
{
    /// <summary>
    /// AB 打包及运行时路径解决器
    /// </summary>
    public class AssetBundlePathResolver
    {
        public static AssetBundlePathResolver instance;

        public AssetBundlePathResolver()
        {
            instance = this;
        }

        /// <summary>
        /// AB 保存的目录名字
        /// </summary>
        public virtual string BundleSaveDirName { get { return "AssetBundles"; } }

        /// <summary>
        /// Luac 保存的目录名字
        /// </summary>
        public virtual string LuacSaveDirName { get { return "Lua"; } }

#if UNITY_EDITOR
        /// <summary>
        /// AB 保存的路径
        /// </summary>
        public string BundleSavePath { get { return string.Format("{0}/{1}/{2}", "Publish", EditorUserBuildSettings.activeBuildTarget, BundleSaveDirName); } }
        /// <summary>
        /// Luac 保存的路径
        /// </summary>
        public string LuacSavePath { get { return string.Format("{0}/{1}/{2}", "Publish", EditorUserBuildSettings.activeBuildTarget, LuacSaveDirName); } }
        /// <summary>
        /// 在编辑器模型下将 abName 转为 Assets/... 路径
        /// 这样就可以不用打包直接用了
        /// </summary>
        /// <param name="abName"></param>
        /// <returns></returns>
        public virtual string GetEditorModePath(string abName)
        {
            //将 Assets.AA.BB.prefab 转为 Assets/AA/BB.prefab
            abName = abName.Replace(".", "/");
            int last = abName.LastIndexOf("/");

            if (last == -1)
                return abName;

            string path = string.Format("{0}.{1}", abName.Substring(0, last), abName.Substring(last + 1));
            return path;
        }
#endif

        /// <summary>
        /// 获取 AB 源文件路径（打包进安装包的）
        /// </summary>
        /// <param name="path"></param>
        /// <param name="forWWW"></param>
        /// <returns></returns>
        public virtual string GetBundleSourceFile(string path, bool forWWW = true)
        {
            string filePath = null;
#if UNITY_EDITOR
            if (forWWW)
                filePath = string.Format("file://{0}/Publish/StandaloneWindows64/{1}/{2}", new DirectoryInfo(Application.dataPath + "/..").FullName.Replace("\\", "/"), BundleSaveDirName, path);
            else
                filePath = string.Format("{0}/Publish/StandaloneWindows64/{1}/{2}", new DirectoryInfo(Application.dataPath + "/..").FullName.Replace("\\", "/"), BundleSaveDirName, path);
#elif UNITY_ANDROID
            if (forWWW)
                filePath = string.Format("jar:file://{0}!/assets/{1}/{2}", Application.dataPath, BundleSaveDirName, path);
            else
                filePath = string.Format("{0}!assets/{1}/{2}", Application.dataPath, BundleSaveDirName, path);
#elif UNITY_IOS
            if (forWWW)
                filePath = string.Format("file://{0}/Raw/{1}/{2}", Application.dataPath, BundleSaveDirName, path);
            else
                filePath = string.Format("{0}/Raw/{1}/{2}", Application.dataPath, BundleSaveDirName, path);
#else
            throw new System.NotImplementedException();
#endif
            return filePath;
        }

        /// <summary>
        /// AB 依赖信息文件名
        /// </summary>
        public virtual string DependFileName { get { return "dep.all"; } }

        DirectoryInfo cacheDir;

        /// <summary>
        /// 用于缓存AB的目录，要求可写
        /// </summary>
        public virtual string BundleCacheDir
        {
            get
            {
                if (cacheDir == null)
                {
// #if UNITY_EDITOR // TODO: YAO
//                     string dir = string.Format("{0}/{1}", Application.streamingAssetsPath, BundleSaveDirName);
// #else
					string dir = string.Format("{0}/AssetBundles", Application.persistentDataPath);
// #endif
                    cacheDir = new DirectoryInfo(dir);
                    if (!cacheDir.Exists)
                        cacheDir.Create();
                }
                return cacheDir.FullName;
            }
        }
    }
}