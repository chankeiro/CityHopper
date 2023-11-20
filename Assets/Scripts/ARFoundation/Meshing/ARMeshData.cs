using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
using UnityEngine.XR.ARFoundation;


namespace Bercetech.Games.Fleepas
{

    public class ARMeshData : MonoBehaviour
    {

        [SerializeField]
        private float _minAreaSize = 15f; // This is just what seems a reasonable value to me
        public float MinAreaSize => _minAreaSize;
        [SerializeField]
        private GameObject _meshPrefab;
        private ARMeshManager _arMeshManager;
        [SerializeField]
        private GameObject _arMesh;
        public GameObject ARMesh => _arMesh;


        // Armesh parameters
        private float _area;
        public float Area => _area;
        private int _meshChunksCount;
        public int MeshChunksCount => _meshChunksCount;
        private List<MeshFilter> _chunkFilters;
        public List<MeshFilter> ChunkFilters => _chunkFilters;

        private Vector3[] _mVertices;
        private Vector3 _chunkArea;

        // Defining a static shared instance variable so other scripts can access to the object
        private static ARMeshData _sharedInstance;
        public static ARMeshData SharedInstance => _sharedInstance;
        void Awake()
        {
            // If there is an instance, and it's not me, delete myself.
            if (_sharedInstance != null && _sharedInstance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _sharedInstance = this;
            }
            
        }

        private void Start()
        {
            // Set arMeshManager on start and any time the meshing process is restarted
            _arMeshManager = GetComponent<ARMeshManager>();
            ARFManager.SharedInstance.MeshingRestarted.Subscribe(_ =>
            {
                _arMeshManager = GetComponent<ARMeshManager>();
            }).AddTo(gameObject);
        }


        public void PreCalculateARMeshData()
        {
            // To calculate the area in that moment and save some time when
            // that value is needed (mainly onEnable Target Generator)
            GetArea();
            Logging.Omigari("Scanned Area: " + _area);
            // Getting Mesh Data that will be queried very often while generation new random locations
            GetMeshChunkData();
        }

        public float GetArea()
        {
            _area = 0;
            if (_arMesh.transform.childCount > 0)
            {
                foreach (Transform child in _arMesh.transform)
                {
                    _mVertices = child.GetComponent<MeshFilter>().sharedMesh.vertices;
                    _chunkArea = Vector3.zero;
                    for (int p = _mVertices.Length - 1, q = 0; q < _mVertices.Length; p = q++)
                    {
                        _chunkArea += Vector3.Cross(_mVertices[q], _mVertices[p]);
                    }
                    _chunkArea *= 0.5f;
                    _area += _chunkArea.magnitude;
                }
            }

            return _area;
        }

        public void GetMeshChunkData()
        {
            _meshChunksCount = _arMesh.transform.childCount;
            if (_meshChunksCount > 0)
            {
                _chunkFilters = _arMesh.transform.GetComponentsInChildren<MeshFilter>().ToList();
            }
        }


        // This function is to get the area directly from the mesh manager, when an ARMesh object
        // has not been created yet
        public float GetAreaFromMeshManager()
        {
            _area = 0;
            if (_arMeshManager.meshes.Count > 0)
            {
                foreach (MeshFilter child in _arMeshManager.meshes)
                {
                    _mVertices = child.sharedMesh.vertices;
                    _chunkArea = Vector3.zero;
                    for (int p = _mVertices.Length - 1, q = 0; q < _mVertices.Length; p = q++)
                    {
                        _chunkArea += Vector3.Cross(_mVertices[q], _mVertices[p]);
                    }
                    _chunkArea *= 0.5f;
                    _area += _chunkArea.magnitude;
                }
            }
            return _area;
        }

        // Copy the mesh in the ARMeshManager to an independent object
        public void CopyMeshesFromManager()
        {
            if (_arMeshManager.meshes.Count > 0)
            {
                foreach (MeshFilter child in _arMeshManager.meshes)
                {
                    Mesh meshChunk = new Mesh();
                    var meshPrefabCopy = Instantiate(_meshPrefab);
                    // Need to copy characteristics in meshChunk instead to asign the 
                    // mesh directly to the prefab like this
                    //  meshPrefabCopy.GetComponent<MeshFilter>().mesh = child.mesh;
                    // Otherwise, sometimes the meshPrefab gets 0 
                    // vertices and triangles, no idea why (Unity bug?)
                    meshPrefabCopy.GetComponent<MeshFilter>().mesh = meshChunk;
                    meshChunk.vertices = child.sharedMesh.vertices;
                    meshChunk.normals = child.sharedMesh.normals;
                    meshChunk.uv = child.sharedMesh.uv;
                    meshChunk.uv2 = child.sharedMesh.uv2;
                    meshChunk.triangles = child.sharedMesh.triangles;
                    meshPrefabCopy.GetComponent<MeshCollider>().sharedMesh = meshChunk;

                    // Instantiating the Chunks in ARmesh
                    Instantiate(meshPrefabCopy, _arMesh.transform);
                    Destroy(meshPrefabCopy);
                }

                _meshPrefab.GetComponent<MeshFilter>().sharedMesh = null;
                _meshPrefab.GetComponent<MeshCollider>().sharedMesh = null;
                // Remove the meshes from the manager
                _arMeshManager.DestroyAllMeshes();

                // Update area and chunkFilters data, which will be used later by other scripts (mainly target generator)
                PreCalculateARMeshData();
            }
        }

        public void DestroyMeshesFromManager()
        {
            foreach (Transform child in _arMesh.transform)
            {
                Destroy(child.gameObject);
            }
        }


    }
}
