// UI layer — decoupled from game logic via UnityEventsusing System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UIController — Owns all HUD elements: credits display, spin button state,
/// result popups, and the paytable panel.
///
/// Listens to SlotMachineManager events (UnityEvents wired in the Inspector)
/// rather than polling — this keeps UI fully decoupled from game logic.
/// </summary>
public class UIController : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────
    // Inspector References
    // ──────────────────────────────────────────────────────────────

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI betText;
    [SerializeField] private Button          spinButton;
    [SerializeField] private TextMeshProUGUI spinButtonLabel;

    [Header("Result Popup")]
    [SerializeField] private GameObject      popupPanel;
    [SerializeField] private TextMeshProUGUI popupTitleText;
    [SerializeField] private TextMeshProUGUI popupBodyText;
    [SerializeField] private Button          popupCloseButton;
    [SerializeField] private Animator        popupAnimator;   // plays "PopupIn" & "PopupOut"

    [Header("Paytable Panel")]
    [SerializeField] private GameObject      paytablePanel;
    [SerializeField] private TextMeshProUGUI paytableText;

    [Header("Jackpot FX")]
    [SerializeField] private GameObject jackpotFXObject;      // particle system / animation
    [SerializeField] private AudioSource jackpotAudioSource;

    [Header("Win Line")]
    [SerializeField] private GameObject winLineIndicator;     // horizontal highlight across reels

    // ──────────────────────────────────────────────────────────────
    // Private State
    // ──────────────────────────────────────────────────────────────

    private static readonly int ANIM_OPEN  = Animator.StringToHash("Open");
    private static readonly int ANIM_CLOSE = Animator.StringToHash("Close");

    // ──────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────────

    private void Start()
    {
        // Populate paytable text from the single source of truth
        if (paytableText != null)
            paytableText.text = PayoutTable.GetPaytableDescription();

        HidePopup();
        SetJackpotFX(false);
        SetWinLine(false);
    }

    // ──────────────────────────────────────────────────────────────
    // Event Handlers (hooked via Inspector UnityEvents on SlotMachineManager)
    // ──────────────────────────────────────────────────────────────

    /// <summary>Called when credits change — refreshes the HUD counter.</summary>
    public void OnCreditsChanged(int newCredits)
    {
        if (creditsText != null)
            creditsText.text = $"CREDITS: {newCredits}";
    }

    /// <summary>Called when a spin begins — disables spin button to prevent double-tap.</summary>
    public void OnSpinStarted()
    {
        SetSpinButtonInteractable(false);
        SetWinLine(false);
        SetJackpotFX(false);
    }

    /// <summary>Called when all reels have stopped — re-enables the spin button.</summary>
    public void OnAllReelsStopped()
    {
        SetSpinButtonInteractable(true);
    }

    /// <summary>
    /// Called with the spin result string. Shows appropriate popup / FX.
    /// Possible values: "WIN", "LOSE", "JACKPOT", "BONUS", "NO_CREDITS"
    /// </summary>
    public void OnSpinResult(string result)
    {
        switch (result)
        {
            case "JACKPOT":
                SetJackpotFX(true);
                SetWinLine(true);
                ShowPopup("🎰 JACKPOT!", "You hit the big one!\nMaximum payout awarded!", Color.yellow);
                break;

            case "WIN":
                SetWinLine(true);
                ShowPopup("🎉 YOU WIN!", "Matching symbols!\nYour payout has been added.", Color.green);
                break;

            case "BONUS":
                SetWinLine(true);
                ShowPopup("🃏 WILD WIN!", "Wild symbols completed the line!", new Color(1f, 0.5f, 0f));
                break;

            case "LOSE":
                // No popup for a loss — keeps the pace fast and avoids rubbing it in
                break;

            case "NO_CREDITS":
                ShowPopup("💸 OUT OF CREDITS", "Add more credits to keep playing.", Color.red);
                break;
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Popup Helpers
    // ──────────────────────────────────────────────────────────────

    public void ShowPopup(string title, string body, Color titleColor)
    {
        if (popupPanel == null) return;
        popupPanel.SetActive(true);

        if (popupTitleText != null)
        {
            popupTitleText.text  = title;
            popupTitleText.color = titleColor;
        }

        if (popupBodyText != null)
            popupBodyText.text = body;

        if (popupAnimator != null)
            popupAnimator.SetTrigger(ANIM_OPEN);
    }

    /// <summary>Called by the popup close button.</summary>
    public void HidePopup()
    {
        if (popupPanel == null) return;

        if (popupAnimator != null)
            popupAnimator.SetTrigger(ANIM_CLOSE);
        else
            popupPanel.SetActive(false);
    }

    // ──────────────────────────────────────────────────────────────
    // Paytable Toggle
    // ──────────────────────────────────────────────────────────────

    public void TogglePaytable()
    {
        if (paytablePanel != null)
            paytablePanel.SetActive(!paytablePanel.activeSelf);
    }

    // ──────────────────────────────────────────────────────────────
    // Private Helpers
    // ──────────────────────────────────────────────────────────────

    private void SetSpinButtonInteractable(bool interactable)
    {
        if (spinButton != null)
            spinButton.interactable = interactable;

        if (spinButtonLabel != null)
            spinButtonLabel.text = interactable ? "SPIN" : "...";
    }

    private void SetJackpotFX(bool active)
    {
        if (jackpotFXObject != null)
            jackpotFXObject.SetActive(active);
    }

    private void SetWinLine(bool active)
    {
        if (winLineIndicator != null)
            winLineIndicator.SetActive(active);
    }
}
