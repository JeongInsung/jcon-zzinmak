using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zzinmak.Data;

namespace Zzinmak.UI
{
    public class LevelUpUI : MonoBehaviour
    {
        [Serializable]
        public class Card
        {
            public Button button;
            public Image icon;
            public TMP_Text nameText;
            public TMP_Text descText;
        }

        [SerializeField] private Card[] cards;

        private Action<UpgradeData> onSelected;

        public void Show(List<UpgradeData> options, Action<UpgradeData> callback)
        {
            onSelected = callback;

            for (int i = 0; i < cards.Length; i++)
            {
                if (i < options.Count)
                {
                    var option = options[i];
                    cards[i].button.gameObject.SetActive(true);
                    if (cards[i].icon != null) cards[i].icon.sprite = option.icon;
                    if (cards[i].nameText != null) cards[i].nameText.text = option.upgradeName;
                    if (cards[i].descText != null) cards[i].descText.text = option.description;

                    cards[i].button.onClick.RemoveAllListeners();
                    var captured = option;
                    cards[i].button.onClick.AddListener(() => onSelected?.Invoke(captured));
                }
                else
                {
                    cards[i].button.gameObject.SetActive(false);
                }
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
