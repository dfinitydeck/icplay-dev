using System;
using System.Collections;
using DG.Tweening;
using Framework;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Shuffle effect component
/// </summary>
public class ShuffleEffect : MonoBehaviour
{
    private readonly Vector2 LEFT_POS_FLY = new Vector2(-695f, -305f);
    private readonly Vector2 RIGHT_POS_FLY = new Vector2(695f, 435f);


    [SerializeField] private Image[] m_PokerImgs;
    [SerializeField] private Image m_FlyImg01;
    [SerializeField] private Image m_FlyImg02;
    [SerializeField] private float m_EffectTime = 5.0f;
    [SerializeField] private float m_MoveIntervalTime = 0.1f;

    private float[] m_PokerPosXs = new float[8] { -500, -500, -500, -250, 0, 250, 500, 750 };
    private float m_DefaultPokerPosLeftX = -500;
    private int[] m_PokerImgIdxs = new int[8] { -1, -2, -3, -4, -5, -6, -7, -8 };
    private Coroutine m_ShuffleCoroutine;
    private int m_CurStep = 0;
    private int m_MaxStep = 0;

    private int m_CurFlyIdx01 = 0;
    private int m_CurFlyIdx02 = 0;
    private bool m_IsPauseMove = false;
    
    private Timer m_Timer;

    private void OnEnable()
    {
        Reset();
        m_ShuffleCoroutine = StartCoroutine(PlayEffect());
        var go = gameObject;
        m_Timer = TimeMgr.SetTimeout(() =>
            {
                go.SetActive(false);
            },
            m_EffectTime + m_MoveIntervalTime * m_PokerImgs.Length);
    }

    private IEnumerator PlayEffect()
    {
        Log.Info($"Start playing shuffle effect");
        while (true)
        {
            MoveOneStep();
            yield return new WaitForSeconds(m_MoveIntervalTime);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    // Poker moves one step
    private void MoveOneStep()
    {
        Log.Info($"Poker moves one step {m_CurStep}  maxStep: {m_MaxStep}");
        // if (m_IsPauseMove)
        //     return;
        m_CurStep++;
        for (int i = 0; i < m_PokerImgs.Length; i++)
        {
            var idx = i;
            var img = m_PokerImgs[idx];
        //    img.enabled = true;
            var rectTrans = img.transform as RectTransform;
            var pokerPosIdx = m_PokerImgIdxs[idx];
            if (pokerPosIdx >= 0 && m_PokerPosXs.Length > pokerPosIdx)
            {
                if (m_CurStep >= m_MaxStep && pokerPosIdx == 1)
                    continue;
                
                // Play fly out effect
                if (m_CurStep == m_CurFlyIdx01)
                {
                    m_FlyImg01.rectTransform.anchoredPosition = Vector2.zero;
                    m_FlyImg01.rectTransform.DOAnchorPos(LEFT_POS_FLY, 1.0f);
                    HideImg();
                }

                if (m_CurStep == m_CurFlyIdx02)
                {
                    m_FlyImg02.rectTransform.anchoredPosition = Vector2.zero;
                    m_FlyImg02.rectTransform.DOAnchorPos(RIGHT_POS_FLY, 1.0f);
                    HideImg();
                }
                
                var posX = m_PokerPosXs[pokerPosIdx];
                rectTrans.DOAnchorPosX(posX, m_MoveIntervalTime);
                m_PokerImgIdxs[idx] = pokerPosIdx + 1;
                continue;
            }

            if (pokerPosIdx >= 0 && pokerPosIdx >= m_PokerPosXs.Length)
            {
                m_PokerImgIdxs[idx] = 1;
                img.enabled = true;
                rectTrans.anchoredPosition = new Vector2(m_DefaultPokerPosLeftX, 0);
                continue;
            }

            m_PokerImgIdxs[idx] = pokerPosIdx + 1;
        }
    }

    private void HideImg()
    {
        for (int i = 0; i < m_PokerImgs.Length; i++)
        {
            var img = m_PokerImgs[i];
            var rectTrans = img.transform as RectTransform;
            if (rectTrans.anchoredPosition == Vector2.zero)
            {
                img.enabled = false;
                TimeMgr.SetTimeout(() =>
                {
                 //   rectTrans.anchoredPosition = new Vector2(m_DefaultPokerPosLeftX, 0);
                    img.enabled = true;
                }, 0.5f);
                break;
            }
        }
    }

    /// <summary>
    /// Reset state
    /// </summary>
    private void Reset()
    {
        m_CurStep = 0;
        for (int i = 0; i < m_PokerImgs.Length; i++)
        {
            var rectTrans = m_PokerImgs[i].transform as RectTransform;
            m_PokerImgs[i].enabled = true;
            if (rectTrans != null) rectTrans.anchoredPosition = new Vector2(m_DefaultPokerPosLeftX, 0);
        }

        m_PokerImgIdxs = new int[8] { -1, -2, -3, -4, -5, -6, -7, -8 };
        m_MaxStep = (int)Math.Floor(m_EffectTime / m_MoveIntervalTime);

        m_CurFlyIdx01 = Random.Range(7, m_MaxStep - 3);
        m_CurFlyIdx02 = Random.Range(7, m_MaxStep - 3);
        while (Math.Abs(m_CurFlyIdx01 - m_CurFlyIdx02) <= 3)
        {
            m_CurFlyIdx02 = Random.Range(7, m_MaxStep - 3);
        }

        Log.Info($"Current random card: {m_CurFlyIdx01}  {m_CurFlyIdx02}");
    }

    public void SetEffectTime(float time)
    {
        m_EffectTime = time;
    }

    private void OnDisable()
    {
        TimeMgr.ClearTimeout(m_Timer);
        StopCoroutine(m_ShuffleCoroutine);
    }
}