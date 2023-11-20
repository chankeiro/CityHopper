using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;

namespace Bercetech.Games.Fleepas.CityBunny
{
    public class Bunny : MonoBehaviour
    {
        private Rigidbody _bunnyRB;
        private Collider _bunnyCollider;
        private Animation _bunnyAnimations;
        [SerializeField]
        private SkinnedMeshRenderer _bunnyEyesMesh;
        [SerializeField]
        private float _gravityMultiplier;
        [SerializeField]
        private float _jumpVerticalSpeed;
        [SerializeField]
        private float _walkVerticalSpeed;
        [SerializeField]
        private float _maxHorizontalSpeed;
        [SerializeField]
        private float _minHorizontalSpeed;
        [SerializeField]
        private string[] _walkCollidersNames;
        [SerializeField]
        private string[] _damageCollidersNames;
        [SerializeField]
        private string[] _outOfBoundsCollidersNames;
        [SerializeField]
        private string[] _prizeCollidersNames;
        [SerializeField]
        private float _outOfBoundsWaitingTime;
        private AudioSource _audioSource;
        [SerializeField]
        private AudioClip _shortJumpSound;
        [SerializeField]
        private AudioClip _longJumpSound;
        [SerializeField]
        private AudioClip _outOfBoundsSound;
        [SerializeField]
        private AudioClip _yuhuuSound;
        private bool _jumpNext;
        private int _lookDirection; // Positive looks right and negative looks left
        private Vector3 _horizontalDirection;
        private bool _hasCollidedFloor;
        private bool _hasCollidedDamage;
        private bool _bunnyOutOfBounds;
        private bool _prizeReached;
        private bool _finalAnimationStarted;
        private bool _canUpdateHorizontalSpeed;
        private List<Renderer> _brightRenderers = new List<Renderer>();
        private bool _isSparking;
        private Collider _colliderWithBunny;
        private Vector3 _lastValidPosition;
        private static Signal _damageTaken = new Signal();
        public static Signal DamageTaken => _damageTaken;

        private static Signal _endReached = new Signal();
        public static Signal EndReached => _endReached;
        private int _tweenAnimation1;
        private int _tweenAnimation2;
        private int _tweenAnimation3;

        // Bunny relative position to the Camera
        private Vector3 _reslPostToCamera;
        private float _horizontalSpeed;

        private void Awake()
        {
            _bunnyRB = GetComponent<Rigidbody>();
            _bunnyCollider = GetComponent<Collider>();
            _bunnyAnimations = GetComponent<Animation>();
            _audioSource = GetComponent<AudioSource>();
            // Getting bright renderers
            var allRenderers = GetComponentsInChildren<Renderer>();
            // Keeping only renderers with main material with Brightness parameter
            foreach (var rend in allRenderers)
            {
                if (rend.material.HasProperty("_Brightness"))
                {
                    _brightRenderers.Add(rend);
                }
            }
            // Increase gravity to make the bunny fall quicker
            Physics.gravity = _gravityMultiplier * Physics.gravity;
            _bunnyRB.useGravity = true;
            // Update Horizontal Speed 8 times per second
            Observable.Interval(TimeSpan.FromSeconds(0.125f)).Subscribe(_ =>
            {
                _reslPostToCamera = transform.position - Camera.main.transform.position;
                // The horizontal speed depends on the position of the camera relative to the bunny
                // It will be negative when the camera points to the left, and positive otherwise
                _horizontalSpeed = _maxHorizontalSpeed * Vector3.Cross(
                    Vector3.Cross(Vector3.up, _reslPostToCamera).normalized,
                    Vector3.Cross(Vector3.up, Camera.main.transform.forward).normalized
                    ).y; // y is in a range of -1 to 1
                if (Math.Abs(_horizontalSpeed) < _minHorizontalSpeed) _horizontalSpeed = 0;
            }).AddTo(gameObject);
            // Start looking Left
            _lookDirection = -1;
        }

        private void OnEnable()
        {
            LeanTween.cancel(_tweenAnimation1);
            LeanTween.cancel(_tweenAnimation2);
            LeanTween.cancel(_tweenAnimation3);
        }


        private void Update()
        {

#if UNITY_EDITOR
            if (Input.GetKeyDown("space"))
            {
                Jump();
            }
#endif
            Walk();
        }


        public void Jump()
        {
            // Bunny will jump next time it wals
            _jumpNext = true;
        }

        private void Walk()
        {

            // Look at the horizontal Direction
            if (!_finalAnimationStarted)
            {
                if (_horizontalSpeed != 0)
                    _lookDirection = _horizontalSpeed < 0 ? -1 : 1;
                transform.LookAt(transform.position + _lookDirection * _horizontalDirection);
            }
            if (_hasCollidedDamage)
            {
                SparkBunny(2, 0.6f);
                // Pushing the bunny up and back
                _bunnyRB.velocity = 1.5f * _walkVerticalSpeed * Vector3.up - 0.3f * _lookDirection * _maxHorizontalSpeed * _horizontalDirection;
                // Animate Bunny
                _bunnyAnimations["jump"].speed = 5 * _gravityMultiplier / _walkVerticalSpeed;
                _bunnyAnimations.Play("jump");
                _hasCollidedDamage = false;

            }
            else if (_bunnyOutOfBounds)
            {
                // Moving the bunny back to the last valida position and freezing it there a for few
                // seconds so the player realizes
                if (!_isSparking)
                {
                    SparkBunny(2, _outOfBoundsWaitingTime);
                    _audioSource.volume = 1;
                    _audioSource.PlayOneShot(_outOfBoundsSound);
                    _bunnyRB.velocity = Vector3.zero;
                    _bunnyRB.useGravity = false;
                    transform.position = _lastValidPosition + 2 * Vector3.up;
                    Observable.Timer(TimeSpan.FromSeconds(_outOfBoundsWaitingTime)).TakeUntilDisable(gameObject).Subscribe(_ =>
                    {
                        // Free bunny
                        _bunnyRB.useGravity = true;
                        _bunnyOutOfBounds = false;
                    }).AddTo(gameObject);
                }
            }
            else if (_prizeReached)
            {
                FinalAnimation();
            }
            // Only jumping from the floor or if the vertical speed is 0 (bunny get's stucked)
            else if (_hasCollidedFloor || _bunnyRB.velocity.y == 0)
            {
                // Storing this position as last valid
                _lastValidPosition = transform.position;
                var verticalSpeed = _jumpNext ? _jumpVerticalSpeed : _walkVerticalSpeed;
                _audioSource.volume = _jumpNext ? 0.7f: 0.16f;
                _audioSource.PlayOneShot(_jumpNext ? _longJumpSound : _shortJumpSound);
                // Moving the bunny towards the center of the floor, to avoid it falling for being to close
                // to the edge. Although that's a good reason to fall, we want to make it a little bit easier to players
                // We will move it progresively (0.2 factor) to avoid big jumps.
                transform.position = transform.position // current position
                    - 0.2f * Vector3.Cross(_horizontalDirection, Vector3.up) // move it in the perpendicular direction to the longitudinal direction of the plane
                                                                             // Calculating the distance between the meridional floor plane and the current position of the bunny. This is the gap to close. But we
                                                                             // are multiplying by 0.2 at the beginning, so we are not doing it at once
                    * new Plane(Vector3.Cross(_horizontalDirection, Vector3.up), _colliderWithBunny.transform.position).GetDistanceToPoint(transform.position);
                // Now applying velocity
                _bunnyRB.velocity = verticalSpeed * Vector3.up
                    + (1 + 0.5f * (_jumpNext ? 1 : 0)) * _horizontalSpeed * _horizontalDirection;
                // Animate Bunny
                _bunnyAnimations["jump"].speed = 5 * _gravityMultiplier / verticalSpeed;
                _bunnyAnimations.Play("jump");
                // Reset jump
                _jumpNext = false;
                _hasCollidedFloor = false;
                _canUpdateHorizontalSpeed = true;
            }
            else
            {
                // Otherwise just update the horizontal Speed 
                if (_bunnyRB.velocity.y > 1f && _canUpdateHorizontalSpeed)
                    _bunnyRB.velocity = _bunnyRB.velocity.y * Vector3.up + _horizontalSpeed * _horizontalDirection;
            }

        }

        private void OnCollisionEnter(Collision collision)
        {
            // If the collision is from a floor, update the direction
            _colliderWithBunny = collision.collider;
            if (_walkCollidersNames.Contains(_colliderWithBunny.name))
            {
                // The direction is set by the floor direction. It sets the direction but not the orientation of the bunny
                // which will depend on the camera pointing to its left or right.
                // In this first step we get the direction. The orientation doesn't matter here
                var fDirection = _colliderWithBunny.transform.TransformDirection(_colliderWithBunny.GetComponent<FloorDirection>().floorDirection);
                // Now we project the righ vector of the camera transform onto the floor direction. This will always
                // provide a vector that will go from the bunny to the right (from the camera perspective) but in the
                // floor direction. The _horizontalSpeed value, which will be negative if  the bunny goes to the 
                // left, will finally decide the bunny orientation
                _horizontalDirection = (Vector3.Dot(Camera.main.transform.right, fDirection) * fDirection).normalized;
                _horizontalDirection.y = 0; // It would be possitive in floors with slope, but we want to controll this speed with the jump
                _horizontalDirection = _horizontalDirection.normalized;
                _hasCollidedFloor = true;
            }
            else if (_damageCollidersNames.Contains(_colliderWithBunny.name) ||
                _damageCollidersNames.Contains(_colliderWithBunny.name.Remove(Math.Max(_colliderWithBunny.name.IndexOf("(Clone)"), 0))))
            {
                _damageTaken.Fire();
                _hasCollidedDamage = true;
                // Stop jumping and horizontal speed update
                _canUpdateHorizontalSpeed = false;
                _jumpNext = false;
            }
            else if (_outOfBoundsCollidersNames.Contains(_colliderWithBunny.name) ||
              _outOfBoundsCollidersNames.Contains(_colliderWithBunny.name.Remove(Math.Max(_colliderWithBunny.name.IndexOf("(Clone)"), 0))))
            {
                _damageTaken.Fire();
                _bunnyOutOfBounds = true;
                // Stop jumping and horizontal speed update
                _canUpdateHorizontalSpeed = false;
                _jumpNext = false;
            }
            else if (_prizeCollidersNames.Contains(_colliderWithBunny.name) ||
            _prizeCollidersNames.Contains(_colliderWithBunny.name.Remove(Math.Max(_colliderWithBunny.name.IndexOf("(Clone)"), 0))))
            {
                _prizeReached = true;
                // Stop Bunny
                _canUpdateHorizontalSpeed = false;
                _jumpNext = false;
            }
            else
            {
                // Don't update horizontal speed if hitting against a wall
                _canUpdateHorizontalSpeed = false;
            }
        }

        private void FinalAnimation()
        {
            if (!_finalAnimationStarted)
            {
                _finalAnimationStarted = true;
                // Stop clock
                Clock.SharedInstance.StopClock();
                // Stop and animate bunny
                _bunnyRB.useGravity = false;
                _bunnyRB.velocity = Vector3.zero;
                SparkBunny(0, 1.5f);
                // Rotate towards user camera (plus 3 turns)
                var finalRotation = Quaternion.LookRotation(
                    (Camera.main.transform.position - transform.position).normalized,
                    Camera.main.transform.up).eulerAngles;
                finalRotation.y += 1080;
                _tweenAnimation1 = LeanTween.rotate(gameObject, finalRotation, 1.5f).setEaseInOutSine().setOnComplete(_ =>
                {
                    _bunnyAnimations["jump"].speed = 1.2f * _gravityMultiplier / _walkVerticalSpeed;
                    _bunnyAnimations["jump"].wrapMode = WrapMode.Loop;
                    _bunnyAnimations.Play("jump");
                    _audioSource.volume = 1;
                    _audioSource.PlayOneShot(_yuhuuSound);
                }).id;
                _tweenAnimation2 = LeanTween.scale(gameObject, 10 * Vector3.one, 2.5f).setEaseOutCubic().setOnComplete(_ =>
                {
                    // Send end of stage reached event
                    _endReached.Fire();
                }).id;
                // Show eyes with hearts
                _tweenAnimation3 = LeanTween.value(0, 100, 0.5f).setEaseInOutSine().setLoopPingPong().setOnUpdate((val) =>
                {
                    _bunnyEyesMesh.SetBlendShapeWeight(2, val);
                }).id;
            }
        }


        private void SparkBunny(int level, float time)
        {
            if (!_isSparking)
            {
                _isSparking = true;
                foreach (Renderer rend in _brightRenderers)
                {
                    var currentBrightness = rend.material.GetFloat("_Brightness");
                    var currentColor = new Color();
                    if (level > 0)
                    {
                        currentColor = rend.material.color;
                        switch (level)
                        {
                            case 1:
                                rend.material.color = new Color(1f, 0.5f, 0f, 1f); // orange
                                break;
                            case 2:
                                float h;
                                float v;
                                Color.RGBToHSV(currentColor, out h, out _, out v);
                                rend.material.color = Color.HSVToRGB(h, 0.9f, v); // saturated color
                                break;
                            default:
                                rend.material.color = Color.red;
                                break;
                        }

                    }
                    // Sparking a number of times depending on the 
                    var repeats = Mathf.RoundToInt(time / 0.3f);
                    LeanTween.value(currentBrightness + 2f, currentBrightness, 0.3f)
                      .setEaseInOutSine().setRepeat(repeats)
                      .setOnUpdate((float val) =>
                      {
                          rend.material.SetFloat("_Brightness", val);
                      })
                      .setOnCompleteOnRepeat(false) // Only running on complete after all repeats
                      .setOnComplete((_) =>
                      {
                          if (level > 0)
                              rend.material.color = currentColor;
                          _isSparking = false;
                      });
                }
            }
        }
    }
}

