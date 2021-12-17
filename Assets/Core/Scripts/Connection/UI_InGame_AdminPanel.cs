using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static EnumsAndData;
using static NetworkingMessageAttributes;

public class UI_InGame_AdminPanel : MonoBehaviour
{
    public static UI_InGame_AdminPanel instance;

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        instance = this;
    }

    [Header("AdminPanelRelated")]
    public GameObject adminPanelButton;
    public GameObject adminPanel;

    [Header("Tween Settings")]
    public UnityEngine.UI.Image adminPanelBackground;
    public Color adminPanelBakground_normalColor;
    public RectTransform adminPanelRect;
    public float adminPanelOpenCloseTime = 0.35f;


    void Start()
    {
        ManageAdminAccessBtn();
        OnPanelInit_SetDefaultStateToSimpleToggles();
    }

    void ManageAdminAccessBtn()
    {
        if (ConnectionManager.instance.currentUserData.IsAdmin())
            adminPanelButton.SetActive(true); 
        else adminPanelButton.SetActive(false);

        adminPanel.gameObject.SetActive(false);
        OnClick_CloseHintPanel();
    }

    public void OnClick_OpenAdminPanel()
    {
        if (ConnectionManager.instance.currentUserData.IsAdmin())
        {
            AdminPanelOpenCloseAnimation(true);
            
            InstantCloseHintPanel();
            UI_InGame.instance.AdminPanelOpened(true);
            RuneSpawnPanel_SetDefaultValues();
        }
    }

    void AdminPanelOpenCloseAnimation(bool state)
    {
        if (state)
        {
            adminPanel.SetActive(true);
            adminPanelBackground.color = Color.clear;
            adminPanelBackground.DOColor(adminPanelBakground_normalColor, adminPanelOpenCloseTime);

            adminPanelRect.anchoredPosition = new Vector2(-adminPanelRect.rect.width, 0);
            DOTween.To(() => adminPanelRect.anchoredPosition, x => adminPanelRect.anchoredPosition = x, new Vector2(0, 0f), adminPanelOpenCloseTime);
        }
        else
        {
            adminPanelBackground.color = adminPanelBakground_normalColor;
            adminPanelBackground.DOColor(Color.clear, adminPanelOpenCloseTime);

            adminPanelRect.anchoredPosition = new Vector2(0, 0);
            DOTween.To(() => adminPanelRect.anchoredPosition, x => adminPanelRect.anchoredPosition = x, new Vector2(-adminPanelRect.rect.width, 0f), adminPanelOpenCloseTime).OnComplete(() => 
            {
                adminPanel.SetActive(false);
            });
        }
    }

    public void OnClick_CloseAdminPanel()
    {
        AdminPanelOpenCloseAnimation(false);
        //OnClick_CloseHintPanel();
        InstantCloseHintPanel();
        UI_InGame.instance.AdminPanelOpened(false);
    }

    #region SpawnRuneSubPanel

    [Header("Spawn Rune Subpanel")]
    public TMP_Dropdown runeSpawnDropdown_runeType;
    public TMP_Dropdown runeSpawnDropdown_runeAmount;
    public TMP_Dropdown runeSpawnDropdown_runePosition;
    public UnityEngine.UI.Toggle runeSpawnToggle_notifyOthers;

    Rune runeSpawn_RuneType;
    CustomRuneSpawn_Amount runeSpawn_amount;
    CustomRuneSpawn_Position runeSpawn_position;
    bool notifyOthersOnRuneSpawn;

    void RuneSpawnPanel_SetDefaultValues()
    {
        runeSpawn_RuneType = Rune.None;
        runeSpawn_amount = CustomRuneSpawn_Amount.One;
        runeSpawn_position = CustomRuneSpawn_Position.ClosestSpawn;
        notifyOthersOnRuneSpawn = true;

        runeSpawnDropdown_runeType.SetValueWithoutNotify((int)runeSpawn_RuneType);
        runeSpawnDropdown_runeAmount.SetValueWithoutNotify((int)runeSpawn_amount);
        runeSpawnDropdown_runePosition.SetValueWithoutNotify((int)runeSpawn_position);
        runeSpawnToggle_notifyOthers.SetIsOnWithoutNotify(notifyOthersOnRuneSpawn);
    }

    public void OnSpawnRuneTypeChanged(int val) => runeSpawn_RuneType = (Rune)val;
    public void OnSpawnRuneAmountChanged(int val) => runeSpawn_amount = (CustomRuneSpawn_Amount) val;
    public void OnSpawnRunePositionsChanged(int val) => runeSpawn_position = (CustomRuneSpawn_Position) val;
    public void OnSpawnRuneNotifyOthersChanged(bool val) => notifyOthersOnRuneSpawn = val;
    

    // code|RuneType|AmountEnum|SpawnPosEnum|notifyOthers
    public void OnClick_SpawnRune()
    {
        Debug.Log("Spawn Rune!");

        string message = $"{ADMIN_COMMAND_SPAWN_RUNE}|{runeSpawn_RuneType}|{runeSpawn_amount}|{runeSpawn_position}|{notifyOthersOnRuneSpawn}";
        ConnectionManager.instance.SendMessageToServer(message);
    }

    #endregion

    #region Other Simple Toggles

    [Header("Other Simple Toggles")]
    public UnityEngine.UI.Toggle showNetworkDelay_toggle;
    public UnityEngine.UI.Toggle showPlayerSpawns_toggle;
    public UnityEngine.UI.Toggle showRuneSpawns_toggle;

    void OnPanelInit_SetDefaultStateToSimpleToggles()
    {
        showNetworkDelay_toggle.SetIsOnWithoutNotify(false);
        showPlayerSpawns_toggle.SetIsOnWithoutNotify(false);
        showRuneSpawns_toggle.SetIsOnWithoutNotify(false);

        OnlineGameManager.instance.showGhostSelf.Value = false;
        OnlineGameManager.instance.showPlayerSpawns.Value = false;
        OnlineGameManager.instance.showRuneSpawns.Value = false;
    }

    public void OnShowNetworkDelayToggleChanged(bool val) => OnlineGameManager.instance.showGhostSelf.Value = val;
    public void OnShowPlayerSpawnsToggleChanged(bool val) => OnlineGameManager.instance.showPlayerSpawns.Value = val;
    public void OnShowRuneSpawnsToggleChanged(bool val) => OnlineGameManager.instance.showRuneSpawns.Value = val;

    #endregion

    #region Admin Panel Hints
    public GameObject adminPanelHintParent;
    public TMP_Text networkHintHeader;
    public TMP_Text networkHintBody;


    [Space(4f)]
    public string spawnRuneHeader;
    [TextArea(3, 10)] public string spawnRuneHint;

    [Space(4f)]
    public string newtworkDelayHeader;
    [TextArea(3, 10)] public string networkDelayHint;

    [Space(4f)]
    public string playerSpawnsHeader;
    [TextArea(3, 10)] public string playerSpawnsHint;

    [Space(4f)]
    public string runeSpawnsHeader;
    [TextArea(3, 10)] public string runeSpawnsHint;

    string GetAdminPanelHeaderByHintType(AdminPanelHint hint)
    {
        switch (hint)
        {
            case AdminPanelHint.SpawnRune: return spawnRuneHeader;
            case AdminPanelHint.NetworkDelay: return newtworkDelayHeader;
            case AdminPanelHint.PlayerSpawns: return playerSpawnsHeader;
            case AdminPanelHint.RuneSpawns: return runeSpawnsHeader;
            default: return "Hint";
        }
    }
    string GetAdminPanelBodyByHintType(AdminPanelHint hint)
    {
        switch (hint)
        {
            case AdminPanelHint.SpawnRune: return spawnRuneHint;
            case AdminPanelHint.NetworkDelay: return networkDelayHint;
            case AdminPanelHint.PlayerSpawns: return playerSpawnsHint;
            case AdminPanelHint.RuneSpawns: return runeSpawnsHint;
            default: return "Well, come on, it's cool anyway ;)";
        }
    }

    public void OnClick_OpenHintPanel(int id) => OpenHintPanel((AdminPanelHint)id);
    public void OpenHintPanel(AdminPanelHint adminPanelHint)
    {
        if (!adminPanelHintParent.activeSelf)
        {
            adminPanelHintParent.SetActive(true);

            adminPanelHintParent.transform.localScale = Vector3.zero;
            adminPanelHintParent.transform.DOScale(Vector3.one, adminPanelOpenCloseTime);
        }

        networkHintHeader.text = GetAdminPanelHeaderByHintType(adminPanelHint);
        networkHintBody.text = GetAdminPanelBodyByHintType(adminPanelHint);
    }

    public void OnClick_CloseHintPanel()
    {
        adminPanelHintParent.transform.localScale = Vector3.one;
        adminPanelHintParent.transform.DOScale(Vector3.zero, adminPanelOpenCloseTime).OnComplete(() => 
        {
            adminPanelHintParent.SetActive(false);
        });
    }

    public void InstantCloseHintPanel() => adminPanelHintParent.SetActive(false);

    #endregion
}
