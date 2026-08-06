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
        [SerializeField] private float cardStaggerDelay = 0.08f;

        private readonly List<CardInfo> spawnedCards = new List<CardInfo>();
        private Action<ICardOption> onSelected;

        public void Show(List<ICardOption> options, Action<ICardOption> callback)
        {
            onSelected = callback;
            ClearCards();
            gameObject.SetActive(true);

            for (int i = 0; i < options.Count; i++)
            {
                var card = Instantiate(cardPrefab, cardContainer);
                card.Setup(options[i], HandleCardClicked, i * cardStaggerDelay);
                spawnedCards.Add(card);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            ClearCards();
        }

        private void HandleCardClicked(ICardOption option)
        {
            onSelected?.Invoke(option);
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
