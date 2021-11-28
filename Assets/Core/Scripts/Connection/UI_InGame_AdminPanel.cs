using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    }

    public void OnClick_OpenAdminPanel()
    {
        if (ConnectionManager.instance.currentUserData.IsAdmin())
        {
            adminPanel.SetActive(true);
            UI_InGame.instance.AdminPanelOpened(true);
            RuneSpawnPanel_SetDefaultValues();
        }
    }

    public void OnClick_CloseAdminPanel()
    {
        adminPanel.SetActive(false);
        UI_InGame.instance.AdminPanelOpened(false);
    }

    #region SpawnRuneSubPanel

    [Header("Spawn Rune Subpanel")]
    public TMP_Dropdown runeSpawnDropdown_runeType;
    public TMP_Dropdown runeSpawnDropdown_runeAmount;
    public TMP_Dropdown runeSpawnDropdown_runePosition;
    public Toggle runeSpawnToggle_notifyOthers;

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

    public void OnSpawnRuneTypeChanged(int val)
    {
        runeSpawn_RuneType = (Rune)val;
    }
    public void OnSpawnRuneAmountChanged(int val)
    {
        runeSpawn_amount = (CustomRuneSpawn_Amount) val;
    }
    public void OnSpawnRunePositionsChanged(int val)
    {
        runeSpawn_position = (CustomRuneSpawn_Position) val;
    }
    public void OnSpawnRuneNotifyOthersChanged(bool val)
    {
        notifyOthersOnRuneSpawn = val;
    }

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
    public Toggle showNetworkDelay_toggle;
    public Toggle showPlayerSpawns_toggle;
    public Toggle showRuneSpawns_toggle;

    void OnPanelInit_SetDefaultStateToSimpleToggles()
    {
        //showNetworkDelay_toggle.SetIsOnWithoutNotify(false);
        //showPlayerSpawns_toggle.SetIsOnWithoutNotify(false);
        //showRuneSpawns_toggle.SetIsOnWithoutNotify(false);
    }

    //bool showNetworkDelay
    //public void OnSpawnRuneNotifyOthersChanged(bool val)
    //{
    //    notifyOthersOnRuneSpawn = val;
    //}

    #endregion
}
