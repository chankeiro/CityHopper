// Explanations of the goal of this script in PaintManager.cs
using UnityEngine;

public class MousePainter : MonoBehaviour
{
    public Camera cam; //AR Camera for raycasting
    [Space]
    public bool mouseSingleClick;
    [Space]
    public Color paintColor;

    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;

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
                Paintable p = hit.collider.GetComponent<Paintable>();
                if (p != null)
                {
                    PaintManager.SharedInstance.paint(p, hit.point, radius, hardness, strength, paintColor);
                }
            }
        }

    }

}

