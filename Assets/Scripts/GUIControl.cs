using UnityEngine;
using System.Collections;

public class GUIControl : MonoBehaviour
{
    public SceneControl scene_control = null;
    public ScoreControl score_control = null;

    public GameObject uiImageStart;
    public GameObject uiImageReturn;

    public RankDisp rankSmallDefeat;                // 击杀怪物数量的评价
    public RankDisp rankSmallEval;                  // 敏捷评价
    public RankDisp rankTotal;						// 总体评价

    public UnityEngine.Sprite[] uiSprite_GradeSmall;
    public UnityEngine.Sprite[] uiSprite_Grade;

    private void Awake()
    {
        this.scene_control = SceneControl.get();
        this.score_control = this.GetComponent<ScoreControl>();

        this.score_control.setNumForce(this.scene_control.result.oni_defeat_num);

        this.rankSmallDefeat.uiSpriteRank = this.uiSprite_GradeSmall;
        this.rankSmallEval.uiSpriteRank = this.uiSprite_GradeSmall;
        this.rankTotal.uiSpriteRank = this.uiSprite_Grade;
    }

    void Start()
    {

    }
    
    void Update()
    {
        this.score_control.setNum(this.scene_control.result.oni_defeat_num);

        // debug print
    }

    public void setVisibleStart(bool is_visible)
    {
        this.uiImageStart.SetActive(is_visible);
    }

    public void setVisibleReturn(bool is_visible)
    {
        this.uiImageReturn.SetActive(is_visible);
    }

    public void startDispDefeatRank()
    {
        int rank = this.scene_control.result_control.getDefeatRank();
        this.rankSmallDefeat.startDisp(rank);
    }

    public void hideDefeatRank()
    {
        this.rankSmallDefeat.hide();
    }

    public void startDispEvaluationRank()
    {
        int rank = this.scene_control.result_control.getEvaluationRank();
        this.rankSmallEval.startDisp(rank);
    }

    public void hideEvaluationRank()
    {
        this.rankSmallEval.hide();
    }

    public void startDispTotalRank()
    {
        int rank = this.scene_control.result_control.getTotalRank();
        this.rankTotal.startDisp(rank);
    }

    // =================================================

    protected static GUIControl instance = null;

    public static GUIControl get()
    {
        if(GUIControl.instance == null)
        {
            GameObject go = GameObject.Find("GameUI");

            if (go != null)
            {
                GUIControl.instance = go.GetComponent<GUIControl>();
            }
            else
            {
                Debug.LogError("Can't find game object \"GameCanvas\".");
            }
        }

        return GUIControl.instance;
    }
}
