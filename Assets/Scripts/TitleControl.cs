using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleControl : MonoBehaviour
{
    public enum STATE
    {
        NONE = -1,
        TITLE = 0,
        WAIT_SE_END,
        FADE_WAIT,

        NUM,
    }

    private STATE state = STATE.NONE;
    private STATE next_state = STATE.NONE;
    private float step_timer = 0.0f;

    private FadeControl fader = null;

    public UnityEngine.UI.Image uiImageStart;

    private const float TITLE_ANIME_TIME = 0.1f;
    private const float FADE_TIME = 1.0f;

    void Start()
    {
        PlayerControl player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        player.SetPlayable(false);

        this.fader = FadeControl.get();
        this.fader.fade(1.0f, new Color(0.0f, 0.0f, 0.0f, 1.0f), new Color(0.0f, 0.0f, 0.0f, 0.0f));

        this.next_state = STATE.TITLE;
    }
    
    void Update()
    {
        this.step_timer += Time.deltaTime;

        switch (this.state)
        {
            case STATE.TITLE:
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        this.next_state = STATE.WAIT_SE_END;
                    }
                }
                break;
            case STATE.WAIT_SE_END:
                {
                    bool to_finish = true;

                    // TODO 入场音效
                    // 这里是模拟了 1.5s 的音效时间
                    if(this.step_timer < 1.5)
                    {
                        to_finish = false;
                    }

                    if (to_finish)
                    {
                        this.fader.fade(FADE_TIME, new Color(0.0f, 0.0f, 0.0f, 0.0f), new Color(0.0f, 0.0f, 0.0f, 1.0f));
                        this.next_state = STATE.FADE_WAIT;
                    }
                }
                break;

            case STATE.FADE_WAIT:
                {
                    if (!this.fader.isActive())
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
                    }
                }
                break;
        }


        if(next_state != STATE.NONE)
        {
            switch (this.next_state)
            {
                case STATE.WAIT_SE_END:
                    {
                        // TODO 播放开始音效
                    }
                    break;
            }

            this.state = this.next_state;
            this.next_state = STATE.NONE;

            this.step_timer = 0.0f;
        }

        switch (this.state)
        {
            case STATE.WAIT_SE_END:
                {
                    float scale = 1.0f;
                    float rate = this.step_timer / TITLE_ANIME_TIME;
                    scale = Mathf.Lerp(2.0f, 1.0f, rate);

                    this.uiImageStart.GetComponent<RectTransform>().localScale = Vector3.one * scale;
                }
                break;
        }
    }
}
