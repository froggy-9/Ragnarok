using System.Collections.Generic;
using DeadLetterOffice.Core;
using UnityEngine;

namespace DeadLetterOffice.Board
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private BoardCardSO[] _allCards;
        [SerializeField] private BoardConnectionSO[] _connections;

        private readonly HashSet<BoardCardSO> _unlockedCards = new();
        private readonly HashSet<BoardConnectionSO> _completedConnections = new();

        public IReadOnlyCollection<BoardCardSO> UnlockedCards => _unlockedCards;
        public IReadOnlyCollection<BoardConnectionSO> CompletedConnections => _completedConnections;

        private void OnEnable()
        {
            GameEventBus.Subscribe<FlagChangedEvent>(OnFlagChanged);
            RefreshUnlockedCards();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<FlagChangedEvent>(OnFlagChanged);
        }

        public bool TryConnect(BoardCardSO fromCard, BoardCardSO toCard)
        {
            BoardConnectionSO connection = FindConnection(fromCard, toCard);
            if (connection == null)
            {
                return false;
            }

            if (_completedConnections.Add(connection))
            {
                if (connection.CompletionFlag != null)
                {
                    connection.CompletionFlag.Value = true;
                }

                GameEventBus.Publish(new ConnectionMadeEvent(connection));
            }

            return true;
        }

        private void RefreshUnlockedCards()
        {
            if (_allCards == null)
            {
                return;
            }

            foreach (BoardCardSO card in _allCards)
            {
                if (card != null && card.IsUnlocked() && _unlockedCards.Add(card))
                {
                    GameEventBus.Publish(new BoardCardUnlockedEvent(card));
                }
            }
        }

        private BoardConnectionSO FindConnection(BoardCardSO fromCard, BoardCardSO toCard)
        {
            if (fromCard == null || toCard == null || _connections == null)
            {
                return null;
            }

            foreach (BoardConnectionSO connection in _connections)
            {
                if (connection == null)
                {
                    continue;
                }

                bool forward = connection.FromCard == fromCard && connection.ToCard == toCard;
                bool backward = connection.FromCard == toCard && connection.ToCard == fromCard;
                if (forward || backward)
                {
                    return connection;
                }
            }

            return null;
        }

        private void OnFlagChanged(FlagChangedEvent evt)
        {
            RefreshUnlockedCards();
        }
    }
}
