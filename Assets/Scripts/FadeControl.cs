using UnityEngine;

public class FadeControl : MonoBehaviour
{
    private float timer;
    private float fadeTime;
    private Color colorStart;
    private Color colorTarget;

    public UnityEngine.UI.Image ui_image;
    private void Awake()
    {
        this.timer = 0.0f;
        this.fadeTime = 0.0f;
        this.colorStart = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        this.colorTarget = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }

    private void Update()
    {
        if (this.timer < this.fadeTime)
        {
            float rate;
            rate = this.timer / this.fadeTime;
            if (float.IsNaN(rate))
            {
                rate = 1.0f;
            }
            rate = Mathf.Sin(rate * Mathf.PI / 2.0f);
            Color color = Color.Lerp(this.colorStart, this.colorTarget, rate);

            this.ui_image.color = color;
        }
        this.timer += Time.deltaTime;
    }

    public void fade(float time, Color start, Color target)
    {
        this.ui_image.gameObject.SetActive(true);

        this.fadeTime = time;
        this.timer = 0.0f;
        this.colorStart = start;
        this.colorTarget = target;
    }

    public bool isActive()
    {
        return (this.timer > this.fadeTime) ? false : true;
    }

    // 单例模式
    private static FadeControl instance = null;
    public static FadeControl get()
    {
        if (FadeControl.instance == null)
        {
            GameObject go = GameObject.Find("FadeControl");

            if (go != null)
            {
                FadeControl.instance = go.GetComponent<FadeControl>();
            }
            else
            {
                Debug.LogError("Can't find game object\"FadeControl\".");
            }
        }

        return FadeControl.instance;
    }
}