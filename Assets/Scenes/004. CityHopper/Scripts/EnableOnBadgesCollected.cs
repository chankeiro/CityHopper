using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Bercetech.Games.Fleepas.CityBunny
{
    public class EnableOnBadgesCollected : MonoBehaviour
    {
        [SerializeField]
        private PositionOscilator _positionOscilator;

    void Start()
        {
            _positionOscilator.enabled = false;
            BadgeBar.AllBadgesCollected.Subscribe(_ =>
            {
                _positionOscilator.enabled = true;
            }).AddTo(gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
