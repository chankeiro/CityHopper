using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bercetech.Games.Fleepas.CityBunny
{
    public class QuestionMark : MonoBehaviour
    {

        [SerializeField]
        private GameObject _badge;
        [SerializeField]
        private GameObject _badgeScreen;
        [SerializeField]
        private Image _badgeScreenLogo;
        [SerializeField]
        private Sprite _logoSprite;
        [SerializeField]
        private TextMeshProUGUI _badgeScreenName;
        [SerializeField]
        private string _badgeScreenNameContent;
        [SerializeField]
        private TextMeshProUGUI _badgeScreenDescription;
        [SerializeField]
        private string _badgeScreenDescriptionContent;
        [SerializeField]
        private TextMeshProUGUI _badgeScreenActionButton;
        [SerializeField]
        private string _badgeScreenActionButtonContent;
        private AudioSource _audioSource;

        private static Signal<Sprite> _newBadgeCollected = new Signal<Sprite>();
        public static Signal<Sprite> NewBadgeCollected => _newBadgeCollected;

        private Material[] _materials;
        private float[] _materialsBrightness = new float[3];
        private bool _isON = true;
        private Rotator _rotator;
        private Vector3 _initialPosition;
        

        private void Awake()
        {
            _materials = GetComponent<Renderer>().materials;
            _rotator = GetComponent<Rotator>();
            _audioSource = GetComponent<AudioSource>();
            // Store all values in case we have to restore the initial state
            var matIdx = 0;
            foreach (Material mat in _materials)
            {
                _materialsBrightness[matIdx] = mat.GetFloat("_Brightness");
                matIdx += 1;
            }
            _initialPosition = transform.position;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.name == "Bunny")
            {
                // Vertical quick movement effect
                gameObject.LeanMove(transform.position + 1f * Vector3.up, 0.1f)
                    .setOnComplete((_) =>
                    {
                        gameObject.LeanMove(transform.position - 1f * Vector3.up, 0.1f);
                    });
                // Proceed if is still ON
                if (_isON)
                {   
                    // Stop Block
                    _rotator.enabled = false;
                    foreach (Material mat in _materials)
                    {
                        mat.SetFloat("_Brightness", 0.3f);
                    }
                    _isON = false;
                    // Show Badge
                    _badge.transform.position = transform.position;
                    _badge.LeanMove(transform.position + 7f * Vector3.up, 1f).setEaseOutCubic();
                    _badge.LeanScale(3f * Vector3.one, 1f).setEaseOutCubic()
                        .setOnComplete((_) =>
                        {
                            // Configure Screen and enable it
                            _badgeScreenLogo.sprite = _logoSprite;
                            _badgeScreenName.text = _badgeScreenNameContent;
                            _badgeScreenDescription.text = _badgeScreenDescriptionContent;
                            _badgeScreenActionButton.text = _badgeScreenActionButtonContent;
                            _newBadgeCollected.Fire(_logoSprite);
                            _badgeScreen.SetActive(true);
                        });
                    _badge.SetActive(true);
                    // Stopping clock
                    Clock.SharedInstance.PauseClock();
                }
                // Play Sound
                _audioSource.Play();
            }

        }

        private void Reset()
        {
            _isON = true;
            _rotator.enabled = true;
            var matIdx = 0;
            foreach (Material mat in _materials)
            {
                mat.SetFloat("_Brightness", _materialsBrightness[matIdx]);
                matIdx += 1;
            }
            transform.position = _initialPosition;
            _badge.SetActive(false);
        }

    }
}
