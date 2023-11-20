using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Bercetech.Games.Fleepas.CityBunny
{
    public class WindyGrass : MonoBehaviour
    {
        private int _leanGrassShape;
        private SkinnedMeshRenderer _grassMesh;

        private void Awake()
        {
            _grassMesh = GetComponent<SkinnedMeshRenderer>();
        }
        void OnEnable()
        {
            // Move the grass leaf from side to side
            _leanGrassShape = LeanTween.value(0, 100, 1 + 0.2f*Random.value).setEaseInOutSine().setLoopPingPong().setOnUpdate((val) =>
            {
                _grassMesh.SetBlendShapeWeight(0, val);
            }).id;
        }

        private void OnDisable()
        {
            LeanTween.cancel(_leanGrassShape);
        }

    }
}
