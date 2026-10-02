using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panahon.Networking;

namespace Panahon.UI.MainMenu
{
    public class LobbyRoomUI : MonoBehaviour
    {
        [Header("Room Info")]
        [SerializeField] private Text roomCodeText;
        [SerializeField] private Button copyCodeButton;
        [SerializeField] private Text connectionStatusText;

        [Header("Player Slots (4)")]
        [SerializeField] private List<Text> slotNameTexts = new List<Text>();
        [SerializeField] private List<Text> slotCharacterTexts = new List<Text>();
        [SerializeField] private List<Image> slotReadyBanners = new List<Image>();

        [Header("Character Selection Grid")]
        [SerializeField] private Button selectMateoButton;
        [SerializeField] private Button selectTalaButton;
        [SerializeField] private Button selectAndresButton;
        [SerializeField] private Button selectMayaButton;

        [Header("Actions")]
        [SerializeField] private Button readyButton;
        [SerializeField] private Text readyButtonText;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button leaveRoomButton;

        private void Start()
        {
            BindActions();

            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnRoomJoined += HandleRoomJoined;
                NetworkManager.Instance.OnSlotsUpdated += UpdateSlotDisplays;
                NetworkManager.Instance.OnStatusMessage += HandleStatusMessage;

                if (!string.IsNullOrEmpty(NetworkManager.Instance.CurrentRoomCode))
                {
                    HandleRoomJoined(NetworkManager.Instance.CurrentRoomCode);
                }
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnRoomJoined -= HandleRoomJoined;
                NetworkManager.Instance.OnSlotsUpdated -= UpdateSlotDisplays;
                NetworkManager.Instance.OnStatusMessage -= HandleStatusMessage;
            }
        }

        private void BindActions()
        {
            if (selectMateoButton != null) selectMateoButton.onClick.AddListener(() => OnCharacterClicked(0));
            if (selectTalaButton != null) selectTalaButton.onClick.AddListener(() => OnCharacterClicked(1));
            if (selectAndresButton != null) selectAndresButton.onClick.AddListener(() => OnCharacterClicked(2));
            if (selectMayaButton != null) selectMayaButton.onClick.AddListener(() => OnCharacterClicked(3));

            if (readyButton != null) readyButton.onClick.AddListener(OnReadyClicked);
            if (startGameButton != null) startGameButton.onClick.AddListener(OnStartGameClicked);
            if (leaveRoomButton != null) leaveRoomButton.onClick.AddListener(OnLeaveRoomClicked);
            if (copyCodeButton != null) copyCodeButton.onClick.AddListener(OnCopyCodeClicked);
        }

        private void HandleRoomJoined(string roomCode)
        {
            if (roomCodeText != null)
            {
                roomCodeText.text = $"ROOM CODE: <color=#50fa7b><b>{roomCode}</b></color>";
            }

            if (startGameButton != null)
            {
                startGameButton.gameObject.SetActive(NetworkManager.Instance != null && NetworkManager.Instance.IsHost);
            }
        }

        private void UpdateSlotDisplays(List<NetworkPlayerSlot> slots)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i < slots.Count)
                {
                    var slot = slots[i];
                    if (slotNameTexts != null && i < slotNameTexts.Count && slotNameTexts[i] != null)
                    {
                        string hostTag = slot.IsHost ? " [HOST]" : "";
                        string localTag = slot.IsLocal ? " (YOU)" : "";
                        slotNameTexts[i].text = slot.IsOccupied ? $"{slot.PlayerName}{hostTag}{localTag}" : $"<i>Slot {i + 1}: Waiting...</i>";
                    }

                    if (slotCharacterTexts != null && i < slotCharacterTexts.Count && slotCharacterTexts[i] != null)
                    {
                        slotCharacterTexts[i].text = slot.SelectedCharacterIndex >= 0 
                            ? NetworkManager.GetCharacterName(slot.SelectedCharacterIndex)
                            : "No Character Selected";
                    }

                    if (slotReadyBanners != null && i < slotReadyBanners.Count && slotReadyBanners[i] != null)
                    {
                        slotReadyBanners[i].color = slot.IsReady 
                            ? new Color(0.2f, 0.8f, 0.4f, 0.9f) 
                            : (slot.IsOccupied ? new Color(0.85f, 0.6f, 0.1f, 0.6f) : new Color(0.25f, 0.25f, 0.3f, 0.4f));
                    }
                }
            }

            // Update ready button label
            if (NetworkManager.Instance != null && NetworkManager.Instance.LocalPlayer != null && readyButtonText != null)
            {
                readyButtonText.text = NetworkManager.Instance.LocalPlayer.IsReady ? "<b>UNREADY</b>" : "<b>READY UP</b>";
            }
        }

        private void OnCharacterClicked(int characterIndex)
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.SelectCharacter(characterIndex);
            }
        }

        private void OnReadyClicked()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.ToggleReady();
            }
        }

        private void OnStartGameClicked()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.StartGame();
            }
        }

        private void OnLeaveRoomClicked()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.LeaveRoom();
            }
            gameObject.SetActive(false);
        }

        private void OnCopyCodeClicked()
        {
            if (NetworkManager.Instance != null && !string.IsNullOrEmpty(NetworkManager.Instance.CurrentRoomCode))
            {
                GUIUtility.systemCopyBuffer = NetworkManager.Instance.CurrentRoomCode;
                HandleStatusMessage("Room code copied to clipboard!");
            }
        }

        private void HandleStatusMessage(string msg)
        {
            if (connectionStatusText != null)
            {
                connectionStatusText.text = msg;
            }
        }
    }
}
