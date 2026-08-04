using System;
using System.Collections.Generic;
using UnityEngine;
using Zzinmak.Data;

namespace Zzinmak.UI
{
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private CardInfo cardPrefab;
        [SerializeField] private Transform cardContainer;

        private readonly List<CardInfo> spawnedCards = new List<CardInfo>();
        private Action<UpgradeData> onSelected;

        public void Show(List<UpgradeData> options, Action<UpgradeData> callback)
        {
            onSelected = callback;
            ClearCards();

            foreach (var option in options)
            {
                var card = Instantiate(cardPrefab, cardContainer);
                card.Setup(option, HandleCardClicked);
                spawnedCards.Add(card);
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            ClearCards();
        }

        private void HandleCardClicked(UpgradeData upgrade)
        {
            onSelected?.Invoke(upgrade);
        }

        private void ClearCards()
        {
            foreach (var card in spawnedCards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            spawnedCards.Clear();
        }
    }
}
