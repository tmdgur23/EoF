using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Cards.Effects;
using Units.General;

public class PendingRoll
{
    public DiceRollEffect Effect;
    public Unit Target;
    public Unit From;
}

[RequireComponent(typeof(CanvasGroup))]
public class DiceSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI diceText;
    public float rollDuration = 1.0f; // Time the dice rolls
    public float rollSpeed = 0.05f;   // Time between number changes

    private Coroutine currentRollCoroutine;
    private Queue<PendingRoll> pendingRolls = new Queue<PendingRoll>();
    private PendingRoll currentRoll;
    private int currentResult;
    private bool isRolling;
    private bool isWaitingForInput;

    public bool IsBusy => currentRoll != null || pendingRolls.Count > 0 || isRolling || isWaitingForInput;

    private GameObject rerollBtnObj;
    private GameObject stopBtnObj;
    private UnityEngine.UI.Button rerollBtnComp;
    private UnityEngine.UI.Button stopBtnComp;
    
    private CanvasGroup canvasGroup;

    public static DiceSystem Instance { get; private set; }

    private static GameObject leftDiceObj;
    private static GameObject rightDiceObj;
    private static GameObject centerDiceObj;
    private static TextMeshProUGUI leftDiceText;
    private static TextMeshProUGUI rightDiceText;
    private static TextMeshProUGUI centerDiceText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        SetupDiceObjects();
    }

    private void SetupDiceObjects()
    {
        var parent = transform.parent;
        if (parent != null)
        {
            var leftTransform = parent.Find("DiceImage");
            if (leftTransform != null)
            {
                leftDiceObj = leftTransform.gameObject;
                leftDiceText = leftDiceObj.GetComponentInChildren<TextMeshProUGUI>();
            }

            var rightTransform = parent.Find("DiceImage2");
            if (rightTransform != null)
            {
                rightDiceObj = rightTransform.gameObject;
                rightDiceText = rightDiceObj.GetComponentInChildren<TextMeshProUGUI>();
            }

            var centerTransform = parent.Find("DiceImageCenter");
            if (centerTransform == null && leftDiceObj != null)
            {
                centerDiceObj = Instantiate(leftDiceObj, parent);
                centerDiceObj.name = "DiceImageCenter";
                var rect = centerDiceObj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(0, 0);
                }
                centerDiceText = centerDiceObj.GetComponentInChildren<TextMeshProUGUI>();
                if (centerDiceText != null)
                {
                    centerDiceText.text = "";
                }

                var ds = centerDiceObj.GetComponent<DiceSystem>();
                if (ds != null) Destroy(ds);
            }
            else if (centerTransform != null)
            {
                centerDiceObj = centerTransform.gameObject;
                centerDiceText = centerDiceObj.GetComponentInChildren<TextMeshProUGUI>();
            }
        }
    }

    private void Start()
    {
        ToggleUI(false);
    }

    private void SetDiceVisibility(GameObject obj, bool visible)
    {
        if (obj == null) return;

        var imgs = obj.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        foreach (var img in imgs)
        {
            img.enabled = visible;
        }

        var txts = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in txts)
        {
            txt.enabled = visible;
        }
    }

    private void ToggleUI(bool active)
    {
        if (leftDiceObj == null) SetupDiceObjects();

        bool isMadness3 = false;
        if (currentRoll != null && currentRoll.Effect != null)
        {
            isMadness3 = currentRoll.Effect is MadnessType3Effect || currentRoll.Effect.GetType().Name == "MadnessType3SecondRollEffect";
        }

        if (!active)
        {
            SetDiceVisibility(leftDiceObj, false);
            SetDiceVisibility(rightDiceObj, false);
            SetDiceVisibility(centerDiceObj, false);
        }
        else
        {
            if (isMadness3)
            {
                SetDiceVisibility(leftDiceObj, true);
                SetDiceVisibility(rightDiceObj, true);
                SetDiceVisibility(centerDiceObj, false);
            }
            else
            {
                SetDiceVisibility(leftDiceObj, false);
                SetDiceVisibility(rightDiceObj, false);
                SetDiceVisibility(centerDiceObj, true);
            }
        }

        if (rerollBtnObj == null)
        {
            var r = Object.FindObjectOfType<Battle.RerollButton>(true);
            if (r != null)
            {
                rerollBtnObj = r.gameObject;
                rerollBtnComp = r.GetComponent<UnityEngine.UI.Button>();
            }
        }

        if (stopBtnObj == null)
        {
            var s = Object.FindObjectOfType<Battle.StopButton>(true);
            if (s != null)
            {
                stopBtnObj = s.gameObject;
                stopBtnComp = s.GetComponent<UnityEngine.UI.Button>();
            }
        }

        if (rerollBtnObj != null) rerollBtnObj.SetActive(active);
        if (stopBtnObj != null) stopBtnObj.SetActive(active);
        
        UpdateButtonsInteractable();
    }

    private void UpdateButtonsInteractable()
    {
        bool interactable = isWaitingForInput && !isRolling;
        if (rerollBtnComp != null) rerollBtnComp.interactable = interactable;
        if (stopBtnComp != null) stopBtnComp.interactable = interactable;
    }

    public void EnqueueRoll(DiceRollEffect effect, Unit target, Unit from)
    {
        pendingRolls.Enqueue(new PendingRoll { Effect = effect, Target = target, From = from });
        if (!isRolling && !isWaitingForInput)
        {
            ProcessNextRoll();
        }
    }

    private void ProcessNextRoll()
    {
        if (pendingRolls.Count > 0)
        {
            currentRoll = pendingRolls.Dequeue();
            StartRoll();
        }
        else
        {
            ToggleUI(false);
        }
    }

    public void StartRoll()
    {
        if (currentRoll == null) return;

        if (leftDiceObj == null) SetupDiceObjects();

        bool isMadness3Effect = currentRoll.Effect is MadnessType3Effect;
        bool isMadness3Second = currentRoll.Effect != null && currentRoll.Effect.GetType().Name == "MadnessType3SecondRollEffect";

        if (isMadness3Effect)
        {
            diceText = leftDiceText;
        }
        else if (isMadness3Second)
        {
            diceText = rightDiceText;
        }
        else
        {
            diceText = centerDiceText;
        }

        isRolling = true;
        isWaitingForInput = false;
        
        ToggleUI(true);

        if (currentRollCoroutine != null)
        {
            StopCoroutine(currentRollCoroutine);
        }
        currentRollCoroutine = StartCoroutine(RollRoutine());
    }

    private IEnumerator RollRoutine()
    {
        float timer = 0f;
        int lastResult = 1;

        int actualMin = currentRoll.Effect.MinRoll;
        int actualMax = currentRoll.Effect.MaxRoll;

        // Check for Boost Status
        var boostStatus = currentRoll.From.StatusContainer.Get<Status.Types.FateBoostRollStatus>();
        if (boostStatus != null && boostStatus.Stacks > 0)
        {
            actualMin += 5;
            actualMax += 5;
        }

        while (timer < rollDuration)
        {
            lastResult = Random.Range(actualMin, actualMax + 1);
            if (diceText != null)
            {
                diceText.text = lastResult.ToString();
            }
            yield return new WaitForSeconds(rollSpeed);
            timer += rollSpeed;
        }

        // Final result
        currentResult = Random.Range(actualMin, actualMax + 1);

        // Check for Max Roll Status
        var maxStatus = currentRoll.From.StatusContainer.Get<Status.Types.FateMaxRollStatus>();
        if (maxStatus != null && maxStatus.Stacks > 0)
        {
            currentResult = actualMax;
        }

        if (diceText != null)
        {
            diceText.text = currentResult.ToString();
        }
        Debug.Log("Dice Turn Roll Final Result: " + currentResult);

        isRolling = false;
        isWaitingForInput = true;
        
        UpdateButtonsInteractable();
    }

    public void OnRerollClicked()
    {
        if (isWaitingForInput && currentRoll != null)
        {
            StartRoll();
        }
    }

    public void OnStopClicked()
    {
        if (isWaitingForInput && currentRoll != null)
        {
            isWaitingForInput = false;
            UpdateButtonsInteractable();
            
            var rollToApply = currentRoll;
            currentRoll = null;

            // Consume Buffs
            var player = rollToApply.From;
            var maxStatus = player.StatusContainer.Get<Status.Types.FateMaxRollStatus>();
            if (maxStatus != null && maxStatus.Stacks > 0) maxStatus.AddStacks(-1);

            var boostStatus = player.StatusContainer.Get<Status.Types.FateBoostRollStatus>();
            if (boostStatus != null && boostStatus.Stacks > 0) boostStatus.AddStacks(-1);

            int runs = 1;
            var doubleStatus = player.StatusContainer.Get<Status.Types.FateDoubleRollStatus>();
            if (doubleStatus != null && doubleStatus.Stacks > 0)
            {
                runs = 2;
                doubleStatus.AddStacks(-1);
            }

            // apply result mechanically
            for (int i = 0; i < runs; i++)
            {
                rollToApply.Effect.ApplyResult(rollToApply.Target, rollToApply.From, currentResult);
            }
            
            if (!isRolling && !isWaitingForInput)
            {
                ProcessNextRoll();
            }
        }
    }
}
