using UnityEngine;

public class ScoreControl : MonoBehaviour
{
    private float timer;
    private int targetNum;
    private int currentNum;

    public GameObject uiScore;
    public UnityEngine.UI.Image[] uiImageScoreDigits;
    public UnityEngine.Sprite[] numSprites;

    void Start()
    {
        this.timer = 0.0f;
    }

    void Update()
    {
        if (this.targetNum > this.currentNum)
        {
            this.timer += Time.deltaTime;
            // 最快每 0.1 秒增加一次分数
            if (timer > 0.1f)
            {
                this.timer = 0.0f;

                // 差距比较大时每次增加5个
                if (this.targetNum - this.currentNum > 10)
                {
                    this.currentNum += 5;
                }
                else
                {
                    this.currentNum++;
                }
            }
        }

        // 数字在增加的时候，稍微放大的效果
        float scale = 1.0f;
        if (this.targetNum != this.currentNum)
        {
            scale = 2.5f - 1.5f * (this.timer * 10.0f);
        }

        int disp_number = Mathf.Max(0, this.currentNum);

        // 按位变化分数的显示
        for (int i = 0; i < this.uiImageScoreDigits.Length; i++)
        {
            int number_at_digit = disp_number % 10;

            this.uiImageScoreDigits[i].sprite = this.numSprites[number_at_digit];
            this.uiImageScoreDigits[i].GetComponent<RectTransform>().localScale = Vector3.one * scale;

            disp_number /= 10;
        }
    }

    public void setNum(int num)
    {
        if (this.targetNum == this.currentNum)
        {
            this.timer = 0.0f;
        }
        this.targetNum = num;
    }

    public void setNumForce(int num)
    {
        this.targetNum = num;
        this.currentNum = num;
    }

    public bool isActive()
    {
        return (this.targetNum != this.currentNum) ? true : false;
    }
}
