using UnityEngine;
using TMPro;
using UniRx;

public class CountDownStartEffect : MonoBehaviour
{
   
    // Update is called once per frame
    void OnEnable()
    {
        Color fontColor = GetComponent<TextMeshProUGUI>().color;
        float startTime = Time.time;
        float transparencyOut;
        float transparencyInOut;
        var fadingEvent = Observable.EveryUpdate().TakeUntilDisable(gameObject).Subscribe(_ =>
        {
            transparencyOut = Mathf.Max(0f, Mathf.Min(1f, (1.5f - (Time.time - startTime)) / 1f));
            transparencyInOut = (Time.time - startTime) < 0.5f
            ? Mathf.Min(1f, Mathf.Pow(Time.time - startTime, 2) / Mathf.Pow(0.5f, 2))
            : transparencyOut;
            // Setting color transparency
            fontColor.a = transparencyInOut;
            GetComponent<TextMeshProUGUI>().color = fontColor;
            // Deactivating object when totally transparent
            if (transparencyOut == 0)
                gameObject.SetActive(false);
        }).AddTo(gameObject);
        
    }
}
