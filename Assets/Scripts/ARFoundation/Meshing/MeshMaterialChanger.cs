using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bercetech.Games.Fleepas
{
    public class MeshMaterialChanger : MonoBehaviour
    {
        [SerializeField]
        private GameObject _arMesh;
        [SerializeField]
        private Material _meshingMaterial;
        [SerializeField]
        private Material _paintableMaterial;
        private Dictionary<int, Material> _materialDictionary;

        public enum ARMeshMaterialType
        {
            Invisible = 0,
            Paintable = 1,
            Meshing = 2,
        }

        private void Start()
        {
            _materialDictionary = new Dictionary<int, Material>
            {
                {0, null},
                {1, _paintableMaterial},
                {2, _meshingMaterial},
            };
        }


        public void SetMateriaOnARMesh(ARMeshMaterialType materialType)
        {
            if (_arMesh.transform.childCount > 0)
            {
                foreach (Transform child in _arMesh.transform)
                {
                    var renderer = child.GetComponent<MeshRenderer>();
                    _materialDictionary.TryGetValue((int)materialType, out Material meshMaterial);
                    if (meshMaterial != null)
                    {
                        if (!renderer.enabled)
                            renderer.enabled = true;
                        renderer.material = meshMaterial;
                    } else
                    {
                        // Disabling renderer if there is not material, or for invisible option
                        renderer.enabled = false;
                    }
                }
            }
        }


    }
}
