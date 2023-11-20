// Explanations of the goal of this script in PaintManagerARDKMesh.cs
using UnityEngine;
using System.Collections.Generic;

namespace Bercetech.Games.Fleepas
{
    public class MousePainterARDKMesh : MonoBehaviour
    {
        public Camera cam;
        [Space]
        public bool mouseSingleClick;
        [Space]
        public float radius = 1;
        public float strength = 1;
        public float hardness = 1;

        // Paint positions
        private static int _maxPaintHitsCount = 5; // Keeping track of a maximum of 5 points. Try to keep it as lower as possible, since it will reduce
        // the work of the shader
        private PaintHit[] _paintHits = new PaintHit[_maxPaintHitsCount];
        private int _paintHitIndex = 0;


        void Update()
        {

            bool click;
            // Option to paint just with the first click, or all the time while the button is pressed
            click = mouseSingleClick ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0);

            if (click)
            {
                Vector3 position = Input.mousePosition;
                Ray ray = cam.ScreenPointToRay(position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    Debug.DrawRay(ray.origin, hit.point - ray.origin, Color.red);
                    Renderer rend = hit.collider.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        var painHit = new PaintHit(
                            new Vector4(hit.point.x, hit.point.y, hit.point.z, 0),
                            new Vector4(hit.normal.x, hit.normal.y, hit.normal.z, 0),
                            Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f),
                            radius,
                            Shader.GetGlobalVector("_Time").y // Using shader time reference
                            );
                        _paintHits[_paintHitIndex] = painHit;
                        // Coming back to write in the first element of the arrays when they are completely filled
                        if (_paintHitIndex < (_maxPaintHitsCount - 1)) _paintHitIndex++; else _paintHitIndex = 0;
                        PaintManagerARDKMesh.SharedInstance.paint(rend, _paintHits, hardness, strength);
                    }
                }
            }

        }

    }
}

