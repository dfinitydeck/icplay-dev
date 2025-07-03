/*
 * CocosCreator Game Framework
 * Copyright (c) 2022 by Murphy , All Rights Reserved. 
 * 
 * @Author: Murphy
 * @Date: 2022-04-18 14:11:24
 * @LastEditTime: 2024-04-08 10:30:52
 * @Description: Resource Management
 */


// import {
//     _decorator, Asset, js, Node, Prefab, SpriteFrame, Material, Texture2D, AnimationClip, TiledMapAsset,
//     AudioClip, sp, sys, assetManager, Font, BitmapFont, JsonAsset, resources, AssetManager
// } from 'cc';

// import { isValid, SpriteAtlas } from 'cc';

// import Fire from '../../extend/fire';
// import { FrameworkComponent } from '../../global/interfaces';
// import { FireID } from '../../global/fireID';
// import { ViewShowLevel } from '../../global/enums';

// declare type ProcessDecorator = (completedCount: number, totalCount: number, item: any) => void;
// declare type CompletedDecorator = (error: Error, resource: any) => void;

// declare type ResDecorator = { ref: number, level: number, asset: Asset }
// declare type WaitingDecorator = { onProcess: ProcessDecorator, onCompleted: CompletedDecorator }

// // Resources waiting to be cleared depth
// const RES_CLEAR_DEPTH = -5
// // Monitor resources waiting to be cleared depth
// const MONITOR_CLEAR_DEPTH = -30
// // Clear refresh interval
// const CLEAR_UPDATE_DELAY = 20
// // Garbage collection time interval
// const GARBAGE_COLLECT_DELAY = 200

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.U2D;
using System.IO;

namespace Framework
{
    public class ResourceManager : SingletonManager<ResourceManager>, IModuleInterface
    {
        public void Init(Action<bool> onInitEnd)
        {
            Addressables.InitializeAsync().Completed += (handle) =>
            {
                onInitEnd?.Invoke(true);
                Debug.Log("Addressables.InitializeAsync End!!");
            };
        }

        public void Run(Action<bool> onRunEnd)
        {
            onRunEnd?.Invoke(true);

        }

        public bool CheckUpdate()
        {
            bool ret = false;
            //Start connecting to server to check for updates
            var checkOp = Addressables.CheckForCatalogUpdates(false);
            //Check completed, verify results
            List<string> catalogs = checkOp.WaitForCompletion();
            if (checkOp.Status == AsyncOperationStatus.Succeeded)
            {
                if (catalogs != null && catalogs.Count > 0)
                {
                    Debug.Log("download catalogs start");

                    var updateOp = Addressables.UpdateCatalogs(catalogs, false);
                    updateOp.WaitForCompletion();

                    ret = true;
                    Addressables.Release(updateOp);

                    Debug.Log("download catalogs finish");
                }
            }
            Addressables.Release(checkOp);
            return ret;
        }

        // Download resources
        public void DownloadAssets(Action<int> onProgress, Action onCompleted)
        {
            // Addressables.DownloadDependenciesAsync
            // Addressables.GetDownloadSizeAsync
            // var downloadOp = Addressables.DownloadDependenciesAsync(Addressables.LoadResourceLocationsAsync("Assets/AddressableAssetsData/AddressableAssetSettings.asset").Result, Addressables.MergeMode.Intersection, false);
            // downloadOp.Completed += (handle) =>
            // {
            //     onCompleted?.Invoke();
            // };
            // downloadOp.DownloadProgressChanged += (handle) =>
            // {
            //     onProgress?.Invoke((int)(handle.PercentComplete * 100));
            // };
        }

        public void PreloadAssets()
        {
            // Addressables.DownloadDependenciesAsync
            // Addressables.GetDownloadSizeAsync
        }

        // Get resource path (different paths for different environments)
        public string GetAssetsPath(string path)
        {
            var url = Path.Combine(string.Format("Assets/GameAssets/{0}", path));       // new System.Uri(
         //   Debug.LogWarning("GetAssetsPath: " + url);

            return url;
        }

        // Instantiate GameObject
        public GameObject InstantGameObject(GameObject obj, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default, bool isClone = true, bool isReset = true)
        {
            GameObject go;
            if (isClone)
            {
                go = Instantiate(obj);
                BaseMotiner.monitor(go);
            }
            else
            {
                go = obj;
            }

            if (go != null)
            {
                if (parent != null)
                {
                    go.transform.SetParent(parent);
                }

                if (isReset)
                {
                    go.transform.localPosition = position;
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = rotation;
                }
            }
            return go;
        }

        #region Basic interfaces (load scenes and resources)

#if !UNITY_WEBGL
        // Synchronous scene loading
        public void LoadScene(string path)
        {
            path = GetAssetsPath(path); //string.Format(GetAssetsPath(), path);
            var op = Addressables.LoadSceneAsync(path);
            op.WaitForCompletion();
            Addressables.Release(op);
        }
#endif

        // Asynchronous scene loading
        public async void LoadSceneAsync(string path)
        {
            path = GetAssetsPath(path); //string.Format(GetAssetsPath(), path);
            await Addressables.LoadSceneAsync(path);
        }

        // Callback scene loading
        public void LoadSceneCallback(string path, Action cb)
        {
            path = GetAssetsPath(path); //string.Format(GetAssetsPath(), path);
            Addressables.LoadSceneAsync(path).Completed += (handle) =>
            {
                cb?.Invoke();
            };
        }

#if !UNITY_WEBGL
        // Synchronous resource loading
        public T LoadGameAsset<T>(string path) where T : UnityEngine.Object
        {
            path = GetAssetsPath(path);
            var op = Addressables.LoadAssetAsync<T>(path);
            T asset = op.WaitForCompletion();

            Addressables.Release(op);
            return asset;
        }
#endif

        // Asynchronous resource loading
        public async UniTask<T> LoadGameAssetAsync<T>(string path) where T : UnityEngine.Object
        {
            // var url = new Uri(Path.Combine(string.Format(GetAssetsPath(), path)));
            path = GetAssetsPath(path); //;
            var asset = await Addressables.LoadAssetAsync<T>(path).ToUniTask();
            return asset as T;
        }

        // Callback resource loading
        public void LoadGameAssetCallback<T>(string path, Action<T> cb) where T : UnityEngine.Object
        {
            path = GetAssetsPath(path); //string.Format(GetAssetsPath(), path);
            // todo: Is there an unreleased event here?
            Addressables.LoadAssetAsync<T>(path).Completed += (handle) =>
            {
                cb?.Invoke(handle.Result);
            };
        }

        #endregion


        #region Other resource loading interfaces

#if !UNITY_WEBGL
        // Synchronous resource loading
        public GameObject LoadGameObject(string path)
        {
            var asset = LoadGameAsset<GameObject>(path);
            GameObject go = Instantiate(asset);
            go.name = path.getFileName();
            BaseMotiner.monitor(go);
            return go;
        }
#endif

        // Asynchronous resource loading
        public async UniTask<GameObject> LoadGameObjectAsync(string path)
        {
            var asset = await LoadGameAssetAsync<GameObject>(path);
            GameObject go = Instantiate(asset);
            go.name = path.getFileName();
            BaseMotiner.monitor(go);
            return go;
        }

        // Callback resource loading
        public void LoadGameObjectCallback(string path, Action<GameObject> cb)
        {
            LoadGameAssetCallback<GameObject>(path, (GameObject asset) =>
            {
                GameObject go = Instantiate(asset);
                go.name = path.getFileName();
                BaseMotiner.monitor(go);
                cb?.Invoke(go);
            });
        }

#if !UNITY_WEBGL
        // Load text
        public TextAsset LoadTextAsset(string path)
        {
            return LoadGameAsset<TextAsset>(path);
        }
#endif

        // Asynchronous load text
        public async UniTask<TextAsset> LoadTextAssetAsync(string path)
        {
            return await LoadGameAssetAsync<TextAsset>(path);
        }

        // Callback load text
        public void LoadTextAssetCallback(string path, Action<TextAsset> cb)
        {
            LoadGameAssetCallback<TextAsset>(path, cb);
        }

#if !UNITY_WEBGL
        // Synchronous load font
        public Font LoadFont(string path)
        {
            return LoadGameAsset<Font>(path);
        }
#endif

        // Asynchronous load font
        public async UniTask<Font> LoadFontAsync(string path)
        {
            return await LoadGameAssetAsync<Font>(path);
        }

        // Callback load font
        public void LoadFontCallback(string path, Action<Font> cb)
        {
            LoadGameAssetCallback<Font>(path, cb);
        }

#if !UNITY_WEBGL
        // Synchronous load material
        public Material LoadMaterial(string path)
        {
            return LoadGameAsset<Material>(path);
        }
#endif

        // Asynchronous load material
        public async UniTask<Material> LoadMaterialAsync(string path)
        {
            return await LoadGameAssetAsync<Material>(path);
        }

        // Callback load material
        public void LoadMaterialCallback(string path, Action<Material> cb)
        {
            LoadGameAssetCallback<Material>(path, cb);
        }

#if !UNITY_WEBGL
        // Synchronous load sprite
        public Sprite LoadSprite(string path)
        {
            return LoadGameAsset<Sprite>(path);
        }
#endif

        // Asynchronous load sprite
        public async UniTask<Sprite> LoadSpriteAsync(string path)
        {
            return await LoadGameAssetAsync<Sprite>(path);
        }

        // Callback load sprite
        public void LoadSpriteCallback(string path, Action<Sprite> cb)
        {
            LoadGameAssetCallback<Sprite>(path, cb);
        }

#if !UNITY_WEBGL
        // Synchronous load sprite atlas
        public SpriteAtlas LoadSpriteAtlas(string path)
        {
            return LoadGameAsset<SpriteAtlas>(path);
        }
#endif

        // Asynchronous load sprite atlas
        public async UniTask<SpriteAtlas> LoadSpriteAtlasAsync(string path)
        {
            return await LoadGameAssetAsync<SpriteAtlas>(path);
        }

        // Callback load sprite atlas
        public void LoadSpriteAtlasCallback(string path, Action<SpriteAtlas> cb)
        {
            LoadGameAssetCallback<SpriteAtlas>(path, cb);
        }

#if !UNITY_WEBGL
        // Synchronous load animation
        public AnimationClip LoadAnimationClip(string path)
        {
            return LoadGameAsset<AnimationClip>(path);
        }
#endif

        // Asynchronous load animation
        public async UniTask<AnimationClip> LoadAnimationClipAsync(string path)
        {
            return await LoadGameAssetAsync<AnimationClip>(path);
        }

        // Callback load animation
        public void LoadAnimationClipCallback(string path, Action<AnimationClip> cb)
        {
            LoadGameAssetCallback<AnimationClip>(path, cb);
        }

#if !UNITY_WEBGL
        // Synchronous load texture
        public Texture2D LoadTexture2D(string path)
        {
            return LoadGameAsset<Texture2D>(path);
        }
#endif

        // Asynchronous load texture
        public async UniTask<Texture2D> LoadTexture2DAsync(string path)
        {
            return await LoadGameAssetAsync<Texture2D>(path);
        }

        // Callback load texture
        public void LoadTexture2DCallback(string path, Action<Texture2D> cb)
        {
            LoadGameAssetCallback<Texture2D>(path, cb);
        }

#if !UNITY_WEBGL
        // Synchronous load audio
        public AudioClip LoadAudioClip(string path)
        {
            return LoadGameAsset<AudioClip>(path);
        }
#endif
        public async UniTask<AudioClip> LoadAudioClipAsync(string path)
        {
            return await LoadGameAssetAsync<AudioClip>(path);
        }

        public void LoadAudioClipCallback(string path, Action<AudioClip> cb)
        {
            LoadGameAssetCallback<AudioClip>(path, cb);
        }

#if !UNITY_WEBGL
        // Synchronous load prefab
        public T LoadPrefab<T>(string path, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            GameObject obj = LoadGameAsset<GameObject>(path);
            var go = InstantGameObject(obj, parent, position, rotation, false);
            return (go != null) ? go.GetComponent<T>() : default;
        }
#endif

        // Asynchronous load prefab
        public async UniTask<GameObject> LoadPrefabAsync(string path, Transform parent)
        {
            GameObject obj = await LoadGameObjectAsync(path);
            return InstantGameObject(obj, parent, default, default, false, false);
        }

        public async UniTask<GameObject> LoadPrefabAsync(string path, Transform parent, Vector3 position, Quaternion rotation)
        {
            GameObject obj = await LoadGameObjectAsync(path);
            return InstantGameObject(obj, parent, position, rotation, false);
        }

        public async UniTask<T> LoadPrefabAsync<T>(string path, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            GameObject obj = await LoadGameObjectAsync(path);
            var go = InstantGameObject(obj, parent, position, rotation, false);
            return (go != null) ? go.GetComponent<T>() : default;
        }

        // Callback load prefab
        public void LoadPrefabCallback<T>(string path, Action<T> cb, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            LoadGameObjectCallback(path, (GameObject obj) =>
            {
                var go = InstantGameObject(obj, parent, position, rotation, false);
                cb?.Invoke((go != null) ? go.GetComponent<T>() : default);
            });
        }

        public void LoadPrefabCallback(string path, Action<GameObject> cb, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            LoadGameObjectCallback(path, (GameObject obj) =>
            {
                var go = InstantGameObject(obj, parent, position, rotation, false);
                cb?.Invoke(go ?? null);
            });
        }

        // Asynchronous instantiate component referenced prefab
        public async UniTask<T> InstantPrefabAsync<T>(AssetReference refAsset, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            var asset = await refAsset.InstantiateAsync(position, rotation, parent).ToUniTask();
            return (typeof(T).Name == "GameObject") ? (T)(object)asset : asset.GetComponent<T>();
            // var go = InstantGameObject(asset, parent, position, rotation);
            // return (go != null) ? go.GetComponent<T>() : default;
        }

        // Callback instantiate component referenced prefab
        public void InstantPrefabCallback<T>(AssetReference refAsset, Action<T> cb, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            refAsset.InstantiateAsync(position, rotation, parent).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = InstantGameObject(handle.Result, parent, position, rotation);
                    cb?.Invoke((go != null) ? go.GetComponent<T>() : default);
                }
                else
                {
                    Debug.LogError("InstantPrefabCallback failed: " + refAsset.Asset.name);
                }
            };
        }

#if !UNITY_WEBGL
        // Load image
        public Image LoadImage(string path, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            var sprite = LoadGameAsset<Sprite>(path);
            if (sprite != null)
            {
                var obj = new GameObject(path.getFileName());
                var go = InstantGameObject(obj, parent, position, rotation, false);
                if (go != null)
                {
                    var image = go.AddComponent<Image>();
                    image.sprite = sprite;
                    return image;
                }
            }
            else
            {
                Debug.LogError("LoadImage is null: " + path);
            }

            return null;
        }
#endif

        // Asynchronous load image
        public async UniTask<Image> LoadImageAsync(string path, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            var sprite = await LoadGameAssetAsync<Sprite>(path);
            if (sprite != null)
            {
                var obj = new GameObject(path.getFileName());
                var go = InstantGameObject(obj, parent, position, rotation, false);
                if (go != null)
                {
                    var image = go.AddComponent<Image>();
                    image.sprite = sprite;
                    return image;
                }
            }
            else
            {
                Debug.LogError("LoadImageAsync is null: " + path);
            }

            return null;
        }

        // Callback load image
        public void LoadImageCallback(string path, Action<Image> cb, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            LoadGameAssetCallback<Sprite>(path, (sprite) =>
            {
                if (sprite != null)
                {
                    var obj = new GameObject(path.getFileName());
                    var go = InstantGameObject(obj, parent, position, rotation, false);
                    if (go != null)
                    {
                        var image = go.AddComponent<Image>();
                        image.sprite = sprite;
                        cb?.Invoke(image);
                    }
                }
                else
                {
                    Debug.LogError("LoadImageCallback is null: " + path);
                }
            });
        }


#if !UNITY_WEBGL
        // Load Texture
        public SpriteRenderer LoadTexture(string path, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            var sprite = LoadGameAsset<Sprite>(path);
            if (sprite != null)
            {
                var obj = new GameObject(path.getFileName());
                var go = InstantGameObject(obj, parent, position, rotation, false);
                if (go != null)
                {
                    var render = go.AddComponent<SpriteRenderer>();
                    render.sprite = sprite;
                    return render;
                }
            }
            else
            {
                Debug.LogError("LoadTexture is null: " + path);
            }

            return null;
        }
#endif

        // Asynchronous load Texture
        public async UniTask<SpriteRenderer> LoadTextureAsync(string path, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            var sprite = await LoadGameAssetAsync<Sprite>(path);
            if (sprite != null)
            {
                var obj = new GameObject(path.getFileName());
                var go = InstantGameObject(obj, parent, position, rotation, false);
                if (go != null)
                {
                    var render = go.AddComponent<SpriteRenderer>();
                    render.sprite = sprite;
                    return render;
                }
            }
            else
            {
                Debug.LogError("LoadTextureAsync is null: " + path);
            }

            return null;
        }

        // Callback load Texture
        public void LoadTextureCallback(string path, Action<SpriteRenderer> cb, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            LoadGameAssetCallback<Sprite>(path, (sprite) =>
            {
                if (sprite != null)
                {
                    var obj = new GameObject(path.getFileName());
                    var go = InstantGameObject(obj, parent, position, rotation, false);
                    if (go != null)
                    {
                        var render = go.AddComponent<SpriteRenderer>();
                        render.sprite = sprite;
                        cb?.Invoke(render);
                    }
                }
                else
                {
                    Debug.LogError("LoadTextureCallback is null: " + path);
                }
            });
        }




        //---------------------------------------------------------------------

        // Asynchronous load Lua
        public async UniTask<TextAsset> LoadLuaAsset(string path)
        {
            path = GetAssetsPath(path); //string.Format(GetAssetsPath(), path);
            var asset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();
            return asset;
        }

        // Asynchronous load a group of Lua with specified tag
        public async UniTask<IList<TextAsset>> LoadLuaAssetsByTag(string tag)
        {
            List<string> tags = new List<string>() { tag };
            var assets = await Addressables.LoadAssetsAsync<TextAsset>(tags, null, Addressables.MergeMode.Intersection).ToUniTask();
            return assets;
        }

        // Synchronous load SkeletonDataAsset
        public Spine.Unity.SkeletonDataAsset LoadSkeletonDataAsset(string path)
        {
            path = GetAssetsPath(path); //string.Format(GetAssetsPath(), path);
            var op = Addressables.LoadAssetAsync<Spine.Unity.SkeletonDataAsset>(path);
            Spine.Unity.SkeletonDataAsset spineDataAsset = op.WaitForCompletion();
            if (spineDataAsset == null)
            {
                Debug.LogError("path is error:" + path);
            }
            // Spine.Unity.SkeletonDataAsset spDataAsset = Instantiate(spineDataAsset);
            Addressables.Release(op);
            return spineDataAsset;
        }

        #endregion
    }
}
