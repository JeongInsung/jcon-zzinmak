using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zzinmak.Data;

public class CardInfo : MonoBehaviour
{
    public Image icon;
    public TMP_Text NameTitle;
    public TMP_Text Description;

    private UpgradeData upgrade;
    private Action<UpgradeData> onClicked;

    public void Setup(UpgradeData data, Action<UpgradeData> callback)
    {
        upgrade = data;
        onClicked = callback;

        if (icon != null) icon.sprite = data.icon;
        if (NameTitle != null) NameTitle.text = data.upgradeName;
        if (Description != null) Description.text = data.description;

        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClicked?.Invoke(upgrade));
        }
    }
}
