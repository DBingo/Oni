using UnityEngine;
using System.Collections;

public class ScoreControl : MonoBehaviour
{
    private float timer;

    private int targetNum;
    private int currentNum;

    public UnityEngine.UI.Image[] uiImageScoreDigits;

    // Use this for initialization
    void Start()
    {
        this.timer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(this.targetNum > this.currentNum)
        {
            this.timer += Time.deltaTime;
            // 最快每 0.1 秒增加一次分数
            if(timer > 0.1f)
            {
                // int idx = Random.Range(0, this.CountUpSounds.Length);
                // this.count_up_audio.PlayOneShot(this.CountUpSounds[idx]);

                this.timer = 0.0f;

                if(this.targetNum - this.currentNum > 10)
                {
                    this.currentNum += 5;
                }
                else
                {
                    this.currentNum++;
                }
            }
        }

        float scale = 1.0f;
        if(this.targetNum != this.currentNum)
        {
            scale = 2.5f - 1.5f * (this.timer * 10.0f);
        }

        int disp_number = Mathf.Max(0, this.currentNum);

        for(int i = 0; i< this.uiImageScoreDigits.Length; i++)
        {
            // 按位变化分数的显示
        }

        Debug.Log(disp_number);

    }

    public void setNum(int num)
    {
        if(this.targetNum == this.currentNum){
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
