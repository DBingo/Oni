using UnityEngine;

public class SceneControl : MonoBehaviour
{
    // 生成的怪物组
    public GameObject OniGroupPrefab = null;
    // 结束画面的怪物小山
    public GameObject[] OniYamaPrefab;
    public GameObject OniEmitterPrefab = null;
    // -----------------
    public PlayerControl player = null;
    public GameObject main_camera = null;
    public LevelControl level_control = null;
    public ResultControl result_control = null;
    public ScoreControl score_control = null;
    public OniEmitterControl oni_emitter = null;

    // 计算分数使用 ------------------------------------------------------------------- //
    public int oni_group_num = 0;
    public int oni_group_appear_max = 10;
    public static int oni_group_penalty = 1;
    public int oni_group_complite = 0;
    public int oni_group_defeat_num = 0;
    public int oni_group_miss_num = 0;
    // 
    public float eval_rate_okay = 1.0f;
    public float eval_rate_good = 2.0f;
    public float eval_rate_great = 4.0f;
    // 击飞时机评价
    public float attack_time = 0.0f;
    public enum EVALUATION
    {
        NONE = -1,
        OKAY = 0,
        GOOD,
        GREAT,
        MISS,
        NUM
    }
    public EVALUATION evaluation = EVALUATION.NONE;
    public const float ATTACK_TIME_GREAT = 0.05f;
    public const float ATTACK_TIME_GOOD = 0.10f;
    // 状态管理 ------------------------------------------------------------------- //
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
    public const int ONI_APPEAR_NUM_MAX = 10; // 生成的每组最大怪物数量
    private const float START_TIME = 2.0f;    // 开始文字显示时间
    private const float GOAL_STOP_DISTANCE = 3.0f;  // 结束时，怪物小山与玩家停下位置的距离

    // 游戏整体的结果 ------------------------------------------------------------------ //
    public struct Result
    {
        public int oni_defeat_num;          // 斩杀的怪物数量（总和）
        public int[] eval_count;            // 各个评价的次数
        public int rank;                    // 游戏整体的结果
        public float score;                 // 当前得分
        public float score_max;             // 理论可以获得的最大分数（all great)
    };

    public Result result;

    // -----------------------------------------------------------------------------------

    void Start()
    {
        // 获取角色相机对象
        this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        this.player.scene_control = this;
        this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");
        // 初始化生成器
        this.level_control = new LevelControl();
        this.level_control.scene_control = this;
        this.level_control.player = this.player;
        this.level_control.OniGroupPrefab = this.OniGroupPrefab;
        this.level_control.create();
        // 初始化结果计算器
        this.result_control = new ResultControl();
        // 清空游戏结果
        this.result.oni_defeat_num = 0;
        this.result.eval_count = new int[(int)EVALUATION.NUM];
        this.result.rank = 0;
        this.result.score = 0;
        this.result.score_max = 0;
        for (int i = 0; i < this.result.eval_count.Length; i++)
        {
            this.result.eval_count[i] = 0;
        }

        // 开始游戏
        this.next_state = STATE.START;
    }

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
                    if (this.state_timer > SceneControl.START_TIME)
                    {
                        this.next_state = STATE.GAME;
                    }
                }
                break;

            case STATE.GAME:
                {
                    if (this.oni_group_complite >= this.oni_group_appear_max)
                    {
                        next_state = STATE.ONI_VANISH_WAIT;
                    }
                }
                break;

            case STATE.ONI_VANISH_WAIT:
                {
                    do
                    {
                        if (GameObject.FindGameObjectsWithTag("OniGroup").Length > 0)
                        {
                            break;
                        }
                        if (this.player.GetSpeedRate() < 0.5f)
                        {
                            break;
                        }

                        next_state = STATE.LAST_RUN;
                    } while (false);
                }
                break;

            case STATE.LAST_RUN:
                {
                    if (this.state_timer > 2.0f)
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

            case STATE.GOAL:
                {
                    if(this.oni_emitter.oni_num == 0)
                    {
                        this.next_state = STATE.ONI_FALL_WAIT;
                    }
                }
                break;

            case STATE.ONI_FALL_WAIT:
                {
                    //if (!this.score_control.isActive() && this.state_timer > 1.5f)
                    //{
                    //    this.next_state = STATE.RESULT_DEFEAT;
                    //}
                }
                break;



        }

        // 状态变化时，下一个状态开始前的初始化处理，然后改变状态
        if (this.next_state != STATE.NONE)
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

                        // 根据结果的 rank 生成怪物山
                        //if(this.result_control.getTotalRank() > 0)
                        //{
                            GameObject oni_yama = Instantiate(this.OniYamaPrefab[2]) as GameObject;

                            Vector3 oni_yama_position = this.player.transform.position;

                            oni_yama_position.x += this.player.CalcDistanceToStop();
                            oni_yama_position.x += SceneControl.GOAL_STOP_DISTANCE;

                            oni_yama_position.y = 0.5f;

                            oni_yama.transform.position = oni_yama_position;
                        //}
                    }
                    break;

                case STATE.GOAL:
                    {
                        GameObject go = Instantiate(this.OniEmitterPrefab) as GameObject;

                        this.oni_emitter = go.GetComponent<OniEmitterControl>();

                        Vector3 emitter_position = oni_emitter.transform.position;

                        emitter_position.x = this.player.transform.position.x;
                        emitter_position.x += this.player.CalcDistanceToStop();
                        emitter_position.x += SceneControl.GOAL_STOP_DISTANCE;

                        emitter_position.y = 10;
                        this.oni_emitter.transform.position = emitter_position;

                        int oni_num = 0;
                        switch (this.result_control.getTotalRank())
                        {
                            case 0: oni_num = Mathf.Min(this.result.oni_defeat_num, 10); break;
                            case 1: oni_num = 6; break;
                            case 2: oni_num = 10; break;
                            case 3: oni_num = 20; break;
                        }

                        oni_num = 10;

                        this.oni_emitter.oni_num = oni_num;
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

    public void OnPlayerMissed()
    {
        this.oni_group_miss_num++;
        this.oni_group_complite++;
        this.oni_group_appear_max -= oni_group_penalty;
        this.evaluation = EVALUATION.MISS;

        this.level_control.OnPlayerMissed();

        this.result.eval_count[(int)this.evaluation]++;

        GameObject[] oni_groups = GameObject.FindGameObjectsWithTag("OniGroup");

        foreach (var oni_group in oni_groups)
        {
            this.oni_group_num--;
            oni_group.GetComponent<OniGroupControl>().beginLeave();
        }
    }

    public void AddDefeatNum(int num)
    {
        this.oni_group_defeat_num++;
        this.oni_group_complite++;
        this.result.oni_defeat_num += num;

        this.attack_time = this.player.GetComponent<PlayerControl>().GetAttackTimer();

        if (this.evaluation == EVALUATION.MISS)
        {
            this.evaluation = EVALUATION.OKAY;
        }
        else
        {
            if (this.attack_time < ATTACK_TIME_GREAT)
            {
                this.evaluation = EVALUATION.GREAT;
            }
            else if (this.attack_time < ATTACK_TIME_GOOD)
            {
                this.evaluation = EVALUATION.GOOD;
            }
            else
            {
                this.evaluation = EVALUATION.OKAY;
            }
        }

        this.result.eval_count[(int)this.evaluation] += num;

        float[] score_list = { this.eval_rate_okay, this.eval_rate_good, this.eval_rate_great, 0 };
        this.result.score_max += num * this.eval_rate_great; // 理论可以获得的最大分数
        this.result.score += num * score_list[(int)this.evaluation]; // 实际获得的分数

        // Rank 的计算
        this.result_control.addOniDefeatScore(num);
        this.result_control.addEvaluationScore(this.evaluation);
    }

    // ================================================================ //
    // 单例模式
    protected static SceneControl instance = null;

    public static SceneControl get()
    {
        if (SceneControl.instance == null)
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
