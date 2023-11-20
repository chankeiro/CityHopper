using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Bercetech.Games.Fleepas.CityBunny
{
    public class BadgeBar : MonoBehaviour
    {
        [SerializeField]
        private Image[] _badgesImages;
        private int _badgesCollected = 0;

        private static Signal _allBadgesCollected = new Signal();
        public static Signal AllBadgesCollected => _allBadgesCollected;
        // Defining a static shared instance variable so other scripts can access to the object
        private static BadgeBar _sharedInstance;
        public static BadgeBar SharedInstance => _sharedInstance;

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

        private void OnEnable()
        {
            QuestionMark.NewBadgeCollected.TakeUntilDisable(gameObject).Subscribe(sprite =>
            {
                // Set new badge when receiving the message
                _badgesImages[_badgesCollected].sprite = sprite;
                _badgesImages[_badgesCollected].gameObject.SetActive(true);
                _badgesCollected += 1;
                // Send message when all badges have been collected
                if (_badgesCollected == _badgesImages.Length)
                    _allBadgesCollected.Fire();
            }).AddTo(gameObject);
        }

        private void OnDisable()
        {
            foreach (Image img in _badgesImages)
            {
                // Reset badges
                img.sprite = null;
            }
        }
    }
}
