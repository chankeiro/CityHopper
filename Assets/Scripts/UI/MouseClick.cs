using UnityEngine;
using UnityEngine.UI;

namespace Bercetech.Games.Fleepas {
    public class MouseClick : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _canvasAudioSource;
        private Button _button;
        // Start is called before the first frame update
        void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(PlayClick);
        }

        private void PlayClick()
        {
            _canvasAudioSource.Play();
        }


    }
}
