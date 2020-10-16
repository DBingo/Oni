using UnityEngine;
using System.Collections;

public class SceneControl : MonoBehaviour
{

    public GameObject OniGroupPrefab = null;

    // 结束画面的怪物小山
    //public GameObject OniPrefab = null;
    //public GameObject OniEmitterPrefab = null;
    //public GameObject[] OniYamaPrefab;

    public PlayerControl player = null;
    public GameObject main_camera = null;

    public LevelControl level_control = null;

    // 分数计算变量       ------------------------------------------------------------- //
    public int oni_group_num = 0;
    public int oni_group_appear_max = 50;
    public static int oni_group_penalty = 1;

    public int oni_group_complite = 0;


    public enum EVALUATION
    {
        NONE = -1,
        OKAY=0,
        GOOD,
        GREAT,
        MISS,
        NUM
    }

    public EVALUATION evaluation = EVALUATION.NONE;

    public ResultControl result_control = null;
    public ScoreControl score_control = null;

    // 状态管理用到的变量 ------------------------------------------------------------- //
    public enum STATE
    {
        NONE = -1,
        START,
        GAME,
        ONI_VANISH_WAIT,
        LAST_RUN,
        PLAYER_STOP_WAIT,

        GOAL,
        ONI_FALL_WAIT,
        RESULT_DEFEAT,
        RESULT_EVALUATION,
        RESULT_TOTAL,

        GAME_OVER,
        GOTO_TITLE,

        NUM,
    }

    public STATE state = STATE.NONE;
    public STATE next_state = STATE.NONE;
    public float state_timer = 0.0f;
    public float state_timer_prev = 0.0f;

    // 一些设定常量 ------------------------------------------------------------------- //

    // 生成的每组最大怪物数量
    public const int ONI_APPEAR_NUM_MAX = 10;

    // 开始文字显示时间
    private const float START_TIME = 2.0f;

    // -------------------------------------------------------------------------------- //

    // 游戏整体的结果
    public struct Result
    {
        public int oni_defeat_num;          // 斩杀的怪物数量（总和）
        public int[] eval_count;            // 各个评价的次数
        public int rank;                    // 游戏整体的结果
        public float score;                 // 当前得分
        public float score_max;             // 游戏中的最高记录
    };

    public Result result;

    // -------------------------------------------------------------------------------- //

    // Use this for initialization
    void Start()
    {
        this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        this.player.scene_control = this;

        this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");
        this.level_control = new LevelControl();
        this.level_control.scene_control = this;
        this.level_control.player = this.player;
        this.level_control.OniGroupPrefab = this.OniGroupPrefab;
        this.level_control.create();

        this.result_control = new ResultControl();

        this.next_state = STATE.START;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Break();
        }

        this.state_timer_prev = this.state_timer;
        this.state_timer += Time.deltaTime;

        // 根据当前状态和条件，判断是否迁移到下一个状态
        switch (this.state)
        {
            case STATE.START:
            {
                if(this.state_timer > SceneControl.START_TIME)
                {
                    this.next_state = STATE.GAME;
                }
            }
            break;

            case STATE.GAME:
            {
                if(this.oni_group_complite >= this.oni_group_appear_max)
                {
                    next_state = STATE.ONI_VANISH_WAIT;
                }
            }
            break;

            case STATE.ONI_VANISH_WAIT:
            {
                do
                {
                    if(GameObject.FindGameObjectsWithTag("OniGroup").Length > 0)
                    {
                        break;
                    }
                    if(this.player.GetSpeedRate() < 0.5f)
                    {
                        break;
                    }

                    next_state = STATE.LAST_RUN;
                } while (false);
            }
            break;

            case STATE.LAST_RUN:
            {
                if(this.state_timer > 2.0f)
                {
                    next_state = STATE.PLAYER_STOP_WAIT;
                }
            }
            break;

            case STATE.PLAYER_STOP_WAIT:
            {
                if (this.player.IsStopped())
                {
                    next_state = STATE.GOAL;
                }
            }
            break;

        }

        // 状态变化时，下一个状态开始前的初始化处理，然后改变状态
        if(this.next_state != STATE.NONE)
        {
            switch (this.next_state)
            {
                case STATE.START:
                {
                    // 显示开始文字
                }
                break;
                case STATE.PLAYER_STOP_WAIT:
                {
                    this.player.StopRequest();

                    // 生成怪物山
                }
                break;
            }

            this.state = this.next_state;
            this.next_state = STATE.NONE;
            this.state_timer = 0.0f;
            this.state_timer_prev = -1.0f;
        }

        // 各个状态的逻辑
        switch (this.state)
        {
            case STATE.GAME:
            {
                this.level_control.oniAppearControl();
            }
            break;
        }
    }

    // ================================================================ //
    // 单例模式
    protected static SceneControl instance = null;

    public static SceneControl get()
    {
        if(SceneControl.instance == null)
        {
            GameObject go = GameObject.Find("SceneControl");

            if (go != null)
            {
                SceneControl.instance = go.GetComponent<SceneControl>();
            }
            else
            {
                Debug.LogError("Can't find game object \"SceneControl\".");
            }
        }

        return (SceneControl.instance);
    }
}
