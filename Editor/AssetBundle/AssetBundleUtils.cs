using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ABSystem
{

    class AssetBundleUtils
    {
        public static AssetBundlePathResolver pathResolver;
        public static DirectoryInfo AssetDir = new DirectoryInfo(Application.dataPath);
        public static string AssetPath = AssetDir.FullName;
        public static DirectoryInfo ProjectDir = AssetDir.Parent;
        public static string ProjectPath = ProjectDir.FullName;

        static Dictionary<int, AssetTarget> _object2target;
        static Dictionary<string, AssetTarget> _assetPath2target;
        static Dictionary<string, string> _fileHashCache;

        public static void Init()
        {
            _object2target = new Dictionary<int, AssetTarget>();
            _assetPath2target = new Dictionary<string, AssetTarget>();
            _fileHashCache = new Dictionary<string, string>();
        }

        public static List<AssetTarget> GetAll()
        {
            return new List<AssetTarget>(_object2target.Values);
        }

        public static AssetTarget Load(Object o)
        {
            AssetTarget target = null;
            if (o != null)
            {
                int instanceId = o.GetInstanceID();

                if (_object2target.ContainsKey(instanceId))
                {
                    target = _object2target[instanceId];
                }
                else
                {
                    string assetPath = AssetDatabase.GetAssetPath(o);
                    string key = assetPath;
                    //Builtin，内置素材，path为空
                    if (string.IsNullOrEmpty(assetPath))
                        key = string.Format("Builtin______{0}", o.name);
                    else
                        key = string.Format("{0}/{1}", assetPath, instanceId);

                    if (_assetPath2target.ContainsKey(key))
                    {
                        target = _assetPath2target[key];
                    }
                    else
                    {
                        if (assetPath.StartsWith("Resources"))
                        {
                            assetPath = string.Format("{0}/{1}.{2}", assetPath, o.name, o.GetType().Name);
                        }
                        FileInfo file = new FileInfo(Path.Combine(ProjectPath, assetPath));
                        target = new AssetTarget(o, file, assetPath);
                        _object2target[instanceId] = target;
                        _assetPath2target[key] = target;
                    }
                }
            }
            return target;
        }

        public static AssetTarget Load(FileInfo file, System.Type t)
        {
            AssetTarget target = null;
            string fullPath = file.FullName;
            int index = fullPath.IndexOf("Assets");
            if (index != -1)
            {
                string assetPath = fullPath.Substring(index);
                if (_assetPath2target.ContainsKey(assetPath))
                {
                    target = _assetPath2target[assetPath];
                }
                else
                {
                    Object o = null;
                    if (t == null)
                        o = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    else
                        o = AssetDatabase.LoadAssetAtPath(assetPath, t);

                    if (o != null)
                    {
                        int instanceId = o.GetInstanceID();

                        if (_object2target.ContainsKey(instanceId))
                        {
                            target = _object2target[instanceId];
                        }
                        else
                        {
                            target = new AssetTarget(o, file, assetPath);
                            string key = string.Format("{0}/{1}", assetPath, instanceId);
                            _assetPath2target[key] = target;
                            _object2target[instanceId] = target;
                        }
                    }
                }
            }

            return target;
        }

        public static AssetTarget Load(FileInfo file)
        {
            return Load(file, null);
        }

        public static string ConvertToABName(string assetPath)
        {
            string bn = assetPath
                .Replace(AssetPath, "")
                .Replace('\\', '.')
                .Replace('/', '.')
                .Replace(" ", "_")
                .ToLower();
            return bn;
        }

        public static string GetFileHash(string path, bool force = false)
        {
            string _hexStr = null;
            if (_fileHashCache.ContainsKey(path) && !force)
            {
                _hexStr = _fileHashCache[path];
            }
            else if (File.Exists(path) == false)
            {
                _hexStr = "FileNotExists";
            }
            else
            {
                FileStream fs = new FileStream(path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);

                _hexStr = HashUtil.Get(fs);
                _fileHashCache[path] = _hexStr;
                fs.Close();
            }
            
            return _hexStr;
        }
    }
}
