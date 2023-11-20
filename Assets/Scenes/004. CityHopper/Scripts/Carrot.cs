using UnityEngine;
using System;
using System.Linq;

namespace Bercetech.Games.Fleepas.CityBunny
{
    public class Carrot : MonoBehaviour
    {
        [SerializeField]
        private string[] _explodeCollidersNames;
        private ExplosionManager _explosionManager;
        // Start is called before the first frame update
        void Awake()
        {
            _explosionManager = GetComponent<ExplosionManager>();
        }


        // Explode on collision with collider
        private void OnCollisionEnter(Collision collision)
        {
            if (_explodeCollidersNames.Contains(collision.collider.name) ||
                _explodeCollidersNames.Contains(collision.collider.name.Remove(Math.Max(collision.collider.name.IndexOf("(Clone)"), 0)))) { 
                _explosionManager.Explode(0);
                GetComponent<AudioSource>().Play();
            }
        }
    }
}
