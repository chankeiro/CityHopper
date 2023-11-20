using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bercetech.Games.Fleepas
{

    public class MainMaterialPicker : MonoBehaviour
    {
        [SerializeField]
        private Material _mainTargetMaterial;
        [SerializeField]
        private Material _newTargetMaterial;
        private Material[] _targetMaterials;
        private bool _targetWithMainMaterial;


        public void ChangeTargetMaterial(bool changeToNewMaterial)
        {
            // Set target Materials on first call
            if (_targetMaterials == null)
            {
                _targetMaterials = gameObject.GetComponentInChildren<Renderer>().materials;
                _targetWithMainMaterial = true;
            }

            _targetMaterials = gameObject.GetComponentInChildren<Renderer>().materials;

            // Only change if the target doesn't already have the desired material
            if (changeToNewMaterial && _targetWithMainMaterial)
            {
                int index = 0;
                _targetWithMainMaterial = false;
                foreach (Material mat in _targetMaterials)
                {
                    if (mat.color == _mainTargetMaterial.color)
                    {
                        _targetMaterials[index] = _newTargetMaterial;
                        gameObject.GetComponentInChildren<Renderer>().materials = _targetMaterials;
                        break;
                    }
                    index++;
                }
            }
            if (!changeToNewMaterial && !_targetWithMainMaterial)
            {
                int index = 0;
                _targetWithMainMaterial = true;
                foreach (Material mat in _targetMaterials)
                {
                    if (mat.color == _newTargetMaterial.color)
                    {
                        _targetMaterials[index] = _mainTargetMaterial;
                        gameObject.GetComponentInChildren<Renderer>().materials = _targetMaterials;
                        break;
                    }
                    index++;
                }
            }
        }

        public Color GetCurrentColor()
        {
            if (_targetWithMainMaterial)
                return _mainTargetMaterial.color;
            else
            {
                return _newTargetMaterial.color;
            }
        }

        public bool HasNewTargetMaterial()
        {
            return !_targetWithMainMaterial;
        }

        public void  SetMaterials(Material mainTargetMaterial, Material newTargetMaterial)
        {
            _mainTargetMaterial = mainTargetMaterial;
            _newTargetMaterial = newTargetMaterial;
        }
    }

}