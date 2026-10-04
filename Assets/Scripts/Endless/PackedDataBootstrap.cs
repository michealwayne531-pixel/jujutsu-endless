using System.Collections;
using System.IO;
using UnityEngine;

namespace Reconstructed.Endless
{
    /// <summary>Loads the recovered packed data when it is a valid AssetBundle and safely reports native player-data containers.</summary>
    public sealed class PackedDataBootstrap : MonoBehaviour
    {
        [SerializeField] private string packedDataFile = "data.unity3d";
        public bool LoadAttempted { get; private set; }
        public AssetBundle LoadedBundle { get; private set; }

        private IEnumerator Start()
        {
            string path = Path.Combine(Application.streamingAssetsPath, packedDataFile);
            if (File.Exists(path))
            {
                LoadAttempted = true;
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
                yield return request;
                LoadedBundle = request.assetBundle;
                if (LoadedBundle != null) Debug.Log("Recovered packed data AssetBundle loaded: " + path);
                else Debug.Log("Recovered data.unity3d is a Unity player-data container; runtime scene assets remain reconstructed.");
            }
            else Debug.LogWarning("Recovered packed data not found at " + path);
        }
    }
}
