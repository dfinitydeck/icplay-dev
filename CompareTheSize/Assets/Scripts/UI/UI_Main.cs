using DG.Tweening;
using Framework;
using Framework.UIFrame;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UI_Main : UIWindow
    {
        private readonly Vector2 LEFT_POS_DEFAULT = new Vector2(-111.6f, -71f);
        private readonly Vector2 RIGHT_POS_DEFAULT = new Vector2(78f, 173f);
        private readonly Vector2 LEFT_POS_OPEN = new Vector2(-260f, -71f);
        private readonly Vector2 RIGHT_POS_OPEN = new Vector2(220f, 173f);

        private readonly Vector2 LEFT_POS_FLY = new Vector2(-695f, -305f);
        private readonly Vector2 RIGHT_POS_FLY = new Vector2(695f, 435f);
        
        private readonly Vector2 RIGHT_FRONT_IMG_FLY = new Vector2(577f, 415f);
        private readonly Vector2 LEFT_FRONT_IMG_FLY = new Vector2(-564f, -290f);
        private readonly Vector2 RIGHT_FRONT_IMG_DEFAULT = new Vector2(182f, 294.5f);
        private readonly Vector2 LEFT_FRONT_IMG_DEFAULT = new Vector2(-193f, -173.5f);

        [SerializeField] private AudioClips m_AudioClips;
        [SerializeField] private Image m_LeftFrontImg;
        [SerializeField] private Image m_RightFrontImg;
        [SerializeField] private Image m_CardBack01Img;
        [SerializeField] private Image m_CardFront01Img;
        [SerializeField] private Image m_CardFront02Img;
        [SerializeField] private Image m_CardBack02Img;
        [SerializeField] private Image m_CardFlyImg;
        [SerializeField] private RectTransform m_NextRootBack;
        [SerializeField] private RectTransform m_NextRootFront;
        [SerializeField] private RectTransform m_CurrRoot;
        [SerializeField] private GameObject m_ShuffleRootGo;
        [SerializeField] private TMP_Text m_ResidueTxt;
        [SerializeField] private TMP_Text m_ScoreTxt;
        [SerializeField] private TMP_Text m_SwapCountTxt;

        [SerializeField] private Button m_ReturnBtn;
        [SerializeField] private Button m_HigherBtn;
        [SerializeField] private Button m_LowerBtn;
        [SerializeField] private Button m_SwapBtn;

        [SerializeField] private GameObject m_CommonRoot;
        [SerializeField] private GameObject m_EndRoot;
        [SerializeField] private NumberComp m_NumberComp;

        [SerializeField] private GameObject m_HigherImgGo;
        [SerializeField] private GameObject m_LowerImgGo;

        // Score prompt
        [SerializeField] private GameObject m_CorrectGo;
        [SerializeField] private GameObject m_JokerGo;
        [SerializeField] private GameObject m_DrawGo;

        [SerializeField] private Button m_MainBtn;
        [SerializeField] private Button m_AgainBtn;
        [SerializeField] private TMP_Text m_AgainTxt;
       
        [SerializeField] private Button m_TipsMaskBtn;
        [SerializeField] private GameObject m_TipsGo;
        [SerializeField] private Button m_GoPay;
        
        private ShuffleEffect m_ShuffleEffect;
        private PokerInfo m_CurPoker;
        private PokerInfo m_NextPoker;
        private bool m_IsFlipRight = false;
        private bool m_IsPlayEffect = false;

        public override void OnShow(object param)
        {
            m_TipsGo.SetActive(false);
            AddEvent();
            ResetGame(true);
            OnPlayerDataUpdate(null);
        }

        /// <summary>
        /// Initialize card display
        /// </summary>
        private void InitPoker()
        {
            m_CurPoker = GameMgr.Instance.GetPoker();
            var pokerSpritePath = GetAssetPath(m_CurPoker);
            // Switch front and back display
            ResourceManager.Instance.LoadGameAssetCallback<Sprite>(pokerSpritePath, (sp) =>
            {
                m_CardFront02Img.sprite = sp;
            });
            m_ResidueTxt.text = GameMgr.Instance.GetResiduePokers().ToString();
            m_ScoreTxt.text = GameMgr.Instance.GetCurScore().ToString();
        }
        private void AddEvent()
        {
            m_ReturnBtn.onClick.AddListener(OnReturnBtnClick);
            m_HigherBtn.onClick.AddListener(OnHigherBtnClick);
            m_LowerBtn.onClick.AddListener(OnLowerBtnClick);
            m_SwapBtn.onClick.AddListener(OnSwapBtnClick);
            m_MainBtn.onClick.AddListener(OnMainBtnClick);
            m_AgainBtn.onClick.AddListener(OnAgainBtnClick);
            m_TipsMaskBtn.onClick.AddListener(OnTipMaskBtnClick);
            m_GoPay.onClick.AddListener(OnPayBtnClick);
            EventMgr.Instance.AddListener(EventKey.PlayerDataUpdate,OnPlayerDataUpdate);
        }

        private void OnPlayerDataUpdate(object obj)
        {
            var playerData = DataModule.Instance.GetPlayerData();
            m_AgainTxt.text = $"[{playerData.count}]";
        }

        private void OnPayBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            UIManager.Instance.Show(UIList.UI_Pay);
        }

        private void OnTipMaskBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            m_TipsGo.SetActive(false);
        }

        private void OnAgainBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            var playerData = DataModule.Instance.GetPlayerData();
            var leftCount = playerData.count;
            // Current game count insufficient
            if (leftCount <= 0)
            {
                m_TipsGo.SetActive(true);
                return;
            }
           m_AudioClips.PlayClip(ClipsType.Again);
           ResetGame();
           DataModule.Instance.MinusGameCount();
        }

        private void ResetGame(bool isStart = false)
        {
            if (isStart)
            {
                m_CardBack01Img.gameObject.SetActive(false);
                m_CardFront02Img.gameObject.SetActive(false);
                GameMgr.Instance.Reset();
                m_ShuffleEffect = m_ShuffleRootGo.GetComponent<ShuffleEffect>();
                m_ShuffleEffect.SetEffectTime(3.0f);
                PlayShuffleEffect();
                InitPoker();
                m_LeftFrontImg.rectTransform.anchoredPosition = LEFT_FRONT_IMG_FLY;
                m_RightFrontImg.rectTransform.anchoredPosition = RIGHT_FRONT_IMG_FLY;
                m_IsPlayEffect = true;
                m_IsFlipRight = true;
                m_EndRoot.SetActive(false);
                m_CommonRoot.SetActive(true);
                m_CorrectGo.SetActive(false);
                m_JokerGo.SetActive(false);
                m_DrawGo.SetActive(false);
                TimeMgr.SetTimeout(PokerFly2Inside,4.0f);
                m_SwapCountTxt.text = $"BONUS {GameMgr.Instance.GetSwapCount().ToString()}";
            }
            else
            {
                if (m_IsPlayEffect)
                    return;
                Log.Info($"Clicked shuffle button");
                GameMgr.Instance.Swap();
                GameMgr.Instance.Reset();
                InitPoker();
                var swapCount = GameMgr.Instance.GetSwapCount();
                if (swapCount == 0)
                    return;
                
                // m_AudioClips.PlayClip(ClipsType.Swap);
               
                m_SwapCountTxt.text = $"BONUS {GameMgr.Instance.GetSwapCount().ToString()}";
                m_ShuffleEffect.SetEffectTime(5.0f);
                // Poker flies out of screen
                PokerFly2Outside();
                m_CardFront01Img.transform.SetParent(m_NextRootBack, false);
                m_CardBack01Img.transform.SetParent(m_NextRootBack, false);
                // Switch front and back display
                m_CardFront01Img.gameObject.SetActive(true);
                m_CardBack01Img.gameObject.SetActive(false);
                // Shuffle effect
                m_IsPlayEffect = true;
                TimeMgr.SetTimeout(PlayShuffleEffect,0.5f);
                m_IsFlipRight = false;
                TimeMgr.SetTimeout(InitPoker, 2.0f);
                TimeMgr.SetTimeout(PokerFly2Inside,7.0f);
                
                m_EndRoot.SetActive(false);
                m_CommonRoot.SetActive(true);
            }
        }

        private void OnMainBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            UIManager.Instance.Hide(UIList.UI_Main);
            UIManager.Instance.Hide(UIList.UI_Setting);
            GameMgr.Instance.Return();
        }

        /// <summary>
        /// Shuffle button click event
        /// </summary>
        private void OnSwapBtnClick()
        {
            if (m_IsPlayEffect)
                return;
            m_AudioClips.PlayClip(ClipsType.Swap);
            Log.Info($"Clicked shuffle button");
            GameMgr.Instance.Swap();
            m_SwapCountTxt.text = $"BONUS {GameMgr.Instance.GetSwapCount().ToString()}";
            m_ShuffleEffect.SetEffectTime(5.0f);
            // Poker flies out of screen
            PokerFly2Outside();
            m_CardFront01Img.transform.SetParent(m_NextRootBack, false);
            m_CardBack01Img.transform.SetParent(m_NextRootBack, false);
            // Switch front and back display
            m_CardFront01Img.gameObject.SetActive(true);
            m_CardBack01Img.gameObject.SetActive(false);
            // Shuffle effect
            m_IsPlayEffect = true;
            TimeMgr.SetTimeout(PlayShuffleEffect,0.5f);
            m_IsFlipRight = false;
            TimeMgr.SetTimeout(InitPoker, 2.0f);
            TimeMgr.SetTimeout(PokerFly2Inside,7.0f);
            
            m_EndRoot.SetActive(false);
            m_CommonRoot.SetActive(true);
        }

        /// <summary>
        /// Choose smaller than current
        /// </summary>
        private void OnLowerBtnClick()
        {
            m_AudioClips.PlayClip(ClipsType.Guess);
            if (m_IsPlayEffect)
                return;
            m_IsPlayEffect = true;
            m_NextPoker = GameMgr.Instance.GetPoker();

            // Current card is joker
            if (m_CurPoker.Size >= 14)
            {
                GuessJoker(true);
                return;
            }
            // Two cards are equal
            if (GameMgr.Instance.CompareSize(m_NextPoker, m_CurPoker) == 0)
            {
                GuessEqual();
                return;
            }
            // Guessed correctly
            if (GameMgr.Instance.CompareSize(m_NextPoker, m_CurPoker) == -1)
            {
                GuessWin();
                return;
            }
            // Guessed wrong
            GuessLose();
        }

        /// <summary>
        /// Choose larger than current
        /// </summary>
        private void OnHigherBtnClick()
        {
            m_AudioClips.PlayClip(ClipsType.Guess);
            if (m_IsPlayEffect)
                return;
            m_IsPlayEffect = true;
            m_NextPoker = GameMgr.Instance.GetPoker();
            // Current card is joker
            if (m_CurPoker.Size >= 14)
            {
                GuessJoker(true);
                return;
            }
            // Two cards are equal
            if (GameMgr.Instance.CompareSize(m_NextPoker, m_CurPoker) == 0)
            {
                GuessEqual();
                return;
            }
            // Guessed correctly
            if (GameMgr.Instance.CompareSize(m_NextPoker, m_CurPoker) == 1)
            {
                GuessWin();
                return;
            }
            // Guessed wrong
            GuessLose();
        }
        

        /// <summary>
        /// Return button click event
        /// </summary>
        private void OnReturnBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            UIManager.Instance.Show(UIList.UI_Setting,1);
        }

        /// <summary>
        /// Guessed correctly
        /// </summary>
        private void GuessWin()
        {
            Log.Info($"Guessed correctly, score +1");
            AddScore(1);
            NextStep();
        }

        /// <summary>
        /// Two cards are equal
        /// </summary>
        private void GuessEqual()
        {
            Log.Info($"Two cards are equal, score +0");
            AddScore(0);
            NextStep();
        }

        /// <summary>
        /// Guessed joker
        /// </summary>
        private void GuessJoker(bool isSelf = false)
        {
            Log.Info($"Drew joker, score +0");
            AddScore(0, true);
            NextStep();
        }
        
        /// <summary>
        /// Guessed wrong
        /// </summary>
        private void GuessLose()
        {
            Log.Info($"Guessed wrong------------");
            m_HigherImgGo.SetActive(GameMgr.Instance.CompareSize(m_NextPoker, m_CurPoker) == 1);
            m_LowerImgGo.SetActive(GameMgr.Instance.CompareSize(m_NextPoker, m_CurPoker) == -1);
            var pokerSpritePath = GetAssetPath(m_NextPoker);
            // Switch front and back display
            ResourceManager.Instance.LoadGameAssetCallback<Sprite>(pokerSpritePath, (sp) =>
            {
                m_CardBack01Img.sprite = sp;
                FlipLeftNext();
            });

            TimeMgr.SetTimeout(() =>
            {
                m_AudioClips.PlayClip(ClipsType.Wrong);
            }, 1.0f);
            TimeMgr.SetTimeout(GameOver ,1f);
        }

        private void NextStep()
        {
            // Guessed correctly, flip card first
            if (m_CurPoker.Size < m_NextPoker.Size)
            {
                // Switch front and back display
                m_CardFront01Img.gameObject.SetActive(true);
                m_CardBack01Img.gameObject.SetActive(false);
                TimeMgr.SetTimeout(FlipLeftNext, 0.5f);
            }
            else
            {
                // Guess correctly, score +1
                GuessWin();
            }
        }

        private void AddScore(int score,bool isJoker = false)
        {
            var scoreTipsShowTime = 1.0f;
            switch (score)
            {
                case 0:
                    TimeMgr.SetTimeout(() =>
                    {
                        m_DrawGo.SetActive(true);
                        RefreshNumber();
                        m_AudioClips.PlayClip(ClipsType.Draw);
                    }, scoreTipsShowTime);
                    break;
                case 1:
                    TimeMgr.SetTimeout(() =>
                    {
                        GameMgr.Instance.AddScore();
                        if (isJoker)
                        {
                            m_JokerGo.SetActive(true);
                            m_AudioClips.PlayClip(ClipsType.Joker);
                            RefreshNumber();
                        }
                        else
                        {
                            m_CorrectGo.SetActive(true);
                            m_AudioClips.PlayClip(ClipsType.Correct);
                            RefreshNumber();
                        }
                    }, scoreTipsShowTime);
                    break;
            }

            TimeMgr.SetTimeout(() =>
            {
                m_CorrectGo.SetActive(false);
                m_DrawGo.SetActive(false);
                m_JokerGo.SetActive(false);
            }, scoreTipsShowTime + 1.0f);
        }
        /// <summary>
        /// Left card flies to right
        /// </summary>
        private void PokerFly2Right()
        {
            Log.Info($"Left card flies to right");
            m_CardFlyImg.sprite = m_CardFront01Img.sprite;
            m_CardFlyImg.gameObject.SetActive(true);
            m_CardFlyImg.rectTransform.anchoredPosition = m_CardFront01Img.rectTransform.anchoredPosition;
            m_CardFlyImg.rectTransform.DOAnchorPos(RIGHT_POS_FLY, 1.0f).OnComplete(() =>
            {
                m_CardFlyImg.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// Add new poker
        /// </summary>
        private void PokerFly2Left()
        {
            m_CardFlyImg.sprite = m_CardFront02Img.sprite;
            m_CardFlyImg.gameObject.SetActive(true);
            m_CardFlyImg.rectTransform.anchoredPosition = m_CardFront02Img.rectTransform.anchoredPosition;
            m_CardFlyImg.rectTransform.DOAnchorPos(LEFT_POS_FLY, 1.0f).OnComplete(() =>
            {
                m_CardFlyImg.gameObject.SetActive(false);
            });
        }

        private void RefreshNumber()
        {
            m_ScoreTxt.text = GameMgr.Instance.GetCurScore().ToString();
            m_ResidueTxt.text = GameMgr.Instance.GetResiduePokers().ToString();
        }

        /// <summary>
        /// Game over
        /// </summary>
        private void GameOver()
        {
            m_EndRoot.SetActive(true);
            m_CommonRoot.SetActive(false);
            m_NumberComp.SetText(GameMgr.Instance.GetCurScore());
            DataModule.Instance.SaveData();
        }

        /// <summary>
        /// Poker flies into screen
        /// </summary>
        private void PokerFly2Inside()
        {
            m_LeftFrontImg.rectTransform.DOAnchorPos(LEFT_FRONT_IMG_DEFAULT, 1.0f);
            m_RightFrontImg.rectTransform.DOAnchorPos(RIGHT_FRONT_IMG_DEFAULT, 1.0f);
            m_IsPlayEffect = false;
        }

        /// <summary>
        /// Poker flies out of screen
        /// </summary>
        private void PokerFly2Outside()
        {
            m_LeftFrontImg.rectTransform.DOAnchorPos(LEFT_FRONT_IMG_FLY, 1.0f);
            m_RightFrontImg.rectTransform.DOAnchorPos(RIGHT_FRONT_IMG_FLY, 1.0f);
        }

        // Move card to expanded state
        private void MovePoker2Open()
        {
            m_NextRootBack.DOAnchorPosX(LEFT_POS_OPEN.x, 0.5f);
            m_NextRootFront.DOAnchorPosX(LEFT_POS_OPEN.x, 0.5f);
            m_CurrRoot.DOAnchorPosX(RIGHT_POS_OPEN.x, 0.5f);
        }

        // Move card to merged state
        private void MovePoker2Close()
        {
            m_NextRootBack.DOAnchorPosX(LEFT_POS_DEFAULT.x, 0.5f).OnComplete(() =>
            {
                m_IsPlayEffect = false;
            });
            m_NextRootFront.DOAnchorPosX(LEFT_POS_DEFAULT.x, 0.5f);
            m_CurrRoot.DOAnchorPosX(RIGHT_POS_DEFAULT.x, 0.5f);
        }

        // Flip left card
        public void FlipLeftNext()
        {
            m_CardFront01Img.gameObject.SetActive(true);
            m_CardBack01Img.gameObject.SetActive(false);
            Log.Info($"Flip left card-----------");
            m_CardFront01Img.rectTransform.DOScale(Vector3.zero, 0.25f).OnComplete(() =>
            {
                // First stage flip
                var pokerSpritePath = GetAssetPath(m_CurPoker);
                ResourceManager.Instance.LoadGameAssetCallback<Sprite>(pokerSpritePath, (sp) =>
                {
                    m_CardFront01Img.sprite = sp;
                    // Switch front and back display
                    m_CardFront01Img.rectTransform.DOScale(Vector3.one, 0.25f);
                    // Second stage flip
                });
            });
        }
        
        // Flip right card
        public void FlipRightNext()
        {
            m_CardFront02Img.gameObject.SetActive(true);
            m_CardBack02Img.gameObject.SetActive(false);
            Log.Info($"Flip right card-----------");
            m_CardFront02Img.rectTransform.DOScale(Vector3.zero, 0.25f).OnComplete(() =>
            {
                var pokerSpritePath = GetAssetPath(m_NextPoker);
                ResourceManager.Instance.LoadGameAssetCallback<Sprite>(pokerSpritePath, (sp) =>
                {
                    m_CardFront02Img.sprite = sp;
                    // Switch front and back display
                    m_CardFront02Img.rectTransform.DOScale(Vector3.one, 0.25f);
                    // Second stage flip
                });
            });
        }

        /// <summary>
        /// Play shuffle effect
        /// </summary>
        private void PlayShuffleEffect()
        {
            m_ShuffleEffect.gameObject.SetActive(true);
        }

        private string GetAssetPath(PokerInfo pokerInfo)
        {
            var size = pokerInfo.Size;
            var pType = pokerInfo.pType;
            var sizeStr = pokerInfo.Size.ToString();
            switch (size)
            {
                case 1:
                    sizeStr = "A";
                    break;
                case 11:
                    sizeStr = "J";
                    break;
                case 12:
                    sizeStr = "Q";
                    break;
                case 13:
                    sizeStr = "K";
                    break;
            }

            var pokerName = $"{sizeStr}_{pType.ToString().ToLower()}";
            if (size == 14)
                pokerName = $"Joker_B";
            if (size == 15)
                pokerName = $"Joker_S";
            return $"Pokers/{pokerName}.png";
        }

        public override void OnHide()
        {
            RemoveEvent();
        }

        private void RemoveEvent()
        {
            m_ReturnBtn.onClick.RemoveAllListeners();
            m_HigherBtn.onClick.RemoveAllListeners();
            m_LowerBtn.onClick.RemoveAllListeners();
            m_SwapBtn.onClick.RemoveAllListeners();
            m_MainBtn.onClick.RemoveAllListeners();
            m_AgainBtn.onClick.RemoveAllListeners();
            m_TipsMaskBtn.onClick.RemoveAllListeners();
            m_GoPay.onClick.RemoveAllListeners();
            EventMgr.Instance.RemoveListener(EventKey.PlayerDataUpdate,OnPlayerDataUpdate);
        }
    }
}