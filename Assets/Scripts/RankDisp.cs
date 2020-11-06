using UnityEngine;
using System.Collections;

public class RankDisp : MonoBehaviour
{
    protected const float ZOOM_TIME = 0.4f;

    public float timer = 0.0f;
    public float scale = 1.0f;
    public float alpha = 0.0f;

    public UnityEngine.UI.Image uiImageGrade;

    public UnityEngine.Sprite[] uiSpriteRank;

    void Start()
    {

    }
    
    void Update()
    {
        float delta_time = Time.deltaTime;

        this.update_sub();

        this.timer += delta_time;
    }

    protected void update_sub()
    {
        float zoon_in_time = ZOOM_TIME;
        float rate;

        if(this.timer < zoon_in_time)
        {
            rate = this.timer / zoon_in_time;
            rate = Mathf.Pow(rate, 2.5f);
            this.scale = Mathf.Lerp(1.5f, 1.0f, rate);
            this.alpha = Mathf.Lerp(0.0f, 1.0f, rate);
        }
        else
        {
            this.scale = 1.0f;
            this.alpha = 1.0f;
        }

        UnityEngine.UI.Image[] images = this.GetComponentsInChildren<UnityEngine.UI.Image>();

        foreach(var image in images)
        {
            Color color = image.color;
            color.a = this.alpha;
            image.color = color;
        }

        this.GetComponent<RectTransform>().localScale = Vector3.one * this.scale;
        
    }

    public void startDisp(int rank)
    {
        this.uiImageGrade.sprite = this.uiSpriteRank[rank];
        this.gameObject.SetActive(true);
        this.timer = 0.0f;
        //this.update_sub();
    }

    public void hide()
    {
        this.gameObject.SetActive(false);
    }
}
