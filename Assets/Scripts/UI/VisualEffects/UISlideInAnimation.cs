using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Bercetech.Games.Fleepas
{
    public class UISlideInAnimation : MonoBehaviour
    {
        // Easing curves demo:
        // https://codepen.io/jhnsnc/pen/LpVXGM
        [SerializeField]
        private _slideDirection _slideDirectionSelected = _slideDirection.fromBotton;
        [SerializeField]
        private float _time = 0.5f;
        [SerializeField]
        private float _delay = 0f;
        [SerializeField]
        private float _extraRelativeSlideFromBorder = 0f;
        [SerializeField]
        private CanvasScaler _canvasScaler;

        public enum _slideDirection : int
        {
            NotDefined = 0,
            fromBotton = 1,
            fromTop = 2,
            fromLeft = 3,
            fromRight = 4,
            toBotton = 5,
            toTop = 6,
            toLeft = 7,
            toRight = 8,
        }

        private void OnEnable()
        {
            Slide((int)_slideDirectionSelected);
        }

        public void Slide(int slideDirection)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            // The variable slideDirection can be overwritten by _slideDirectionSelected
            if (_slideDirectionSelected != _slideDirection.NotDefined)
                slideDirection = (int)_slideDirectionSelected;

            if (slideDirection == (int)_slideDirection.fromBotton)
            {
                var finalPosition = transform.localPosition.y;
                transform.localPosition = new Vector2(0, -_canvasScaler.referenceResolution.y * (1 + _extraRelativeSlideFromBorder));
                transform.LeanMoveLocalY(finalPosition, _time).setEaseOutCubic().delay = _delay;
                return;
            }
            if (slideDirection == (int)_slideDirection.fromTop)
            {
                var finalPosition = transform.localPosition.y;
                transform.localPosition = new Vector2(0, _canvasScaler.referenceResolution.y * (1 + _extraRelativeSlideFromBorder));
                transform.LeanMoveLocalY(finalPosition, _time).setEaseOutCubic().delay = _delay;
                return;
            }
            if (slideDirection == (int)_slideDirection.fromLeft)
            {
                var finalPosition = transform.localPosition.x;
                transform.localPosition = new Vector2(-_canvasScaler.referenceResolution.x * (1 + _extraRelativeSlideFromBorder), 0);
                transform.LeanMoveLocalX(finalPosition, _time).setEaseOutCubic().delay = _delay;
                return;
            }
            if (slideDirection == (int)_slideDirection.fromRight)
            {
                var finalPosition = transform.localPosition.x;
                transform.localPosition = new Vector2(_canvasScaler.referenceResolution.x * (1 + _extraRelativeSlideFromBorder), 0);
                transform.LeanMoveLocalX(finalPosition, _time).setEaseOutCubic().delay = _delay;
                return;
            }
            if (slideDirection == (int)_slideDirection.toBotton)
            {
                var finalPosition = transform.localPosition.y;
                transform.LeanMoveLocalY(-_canvasScaler.referenceResolution.y * (1 + _extraRelativeSlideFromBorder), _time).setEaseOutCubic()
                    .setOnComplete(_ => {
                        gameObject.SetActive(false);
                        transform.localPosition = new Vector2(0, finalPosition);
                    }).delay = _delay;
                return;
            }
            if (slideDirection == (int)_slideDirection.toTop)
            {
                var finalPosition = transform.localPosition.y;
                transform.LeanMoveLocalY(_canvasScaler.referenceResolution.y * (1 + _extraRelativeSlideFromBorder), _time).setEaseOutCubic()
                    .setOnComplete(_ => {
                        gameObject.SetActive(false);
                        transform.localPosition = new Vector2(0, finalPosition);
                    }).delay = _delay;
                return;
            }
            if (slideDirection == (int)_slideDirection.toLeft)
            {
                var finalPosition = transform.localPosition.x;
                transform.LeanMoveLocalX(-_canvasScaler.referenceResolution.x * (1 + _extraRelativeSlideFromBorder), _time).setEaseOutCubic()
                    .setOnComplete(_ => {
                        gameObject.SetActive(false);
                        transform.localPosition = new Vector2(finalPosition, 0);
                    }).delay = _delay;
                return;
            }
            if (slideDirection == (int)_slideDirection.toRight)
            {
                var finalPosition = transform.localPosition.x;
                transform.LeanMoveLocalX(_canvasScaler.referenceResolution.x * (1 + _extraRelativeSlideFromBorder), _time).setEaseOutCubic()
                    .setOnComplete(_ => {
                        gameObject.SetActive(false);
                        transform.localPosition = new Vector2(finalPosition, 0);
                    }).delay = _delay;
                return;
            }
        }

    }
}
