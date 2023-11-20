using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRendererScaler : MonoBehaviour
{
    
    void Start()
    {
        // When the renderer Rect Transform needs to have a defined
        // width and height instead of being relative to the screen size,
        // or we need to change from portrait to landscape during the game,
        // we need to calculate the width of the renderer at the beginning
        // depending on the aspect ratio, so its shape doesn't get deformed
        RectTransform rectTransform = this.GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, rectTransform.rect.height * Screen.width/Screen.height
            );
    }

}
