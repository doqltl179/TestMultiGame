using Mu3Library.Utility;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SteamP2P : GenericSingleton<SteamP2P> {
    public List<CSteamID> joinedFriends = new List<CSteamID>();

    private const string MessageHead_Invite = "Invite";
    private const string MessageHead_Join = "Join";
    private const string MessageHead_Chat = "Chat";

    private const string MessageBody_InviteRequest = "Request";
    private const string MessageBody_InviteAccept = "Accept";
    private const string MessageBody_InviteReject = "Reject";

    private const string MessageSplitWord = "|dnt_cange_vlue|";

    public Action<string, string> OnChatReceived;



    private void Awake() {
        if(!SteamManager.Initialized) {
            Debug.LogError("Steam is not initialized.");

            return;
        }

        SteamNetworking.AllowP2PPacketRelay(true);
    }

    private void Update() {
        ReceivePackets();
    }

    #region Utility
    public void Invite(CSteamID id) {
        SendMessage(id, P2PMessageType.Invite, MessageBody_InviteRequest);
    }
    #endregion

    private void SendMessage(CSteamID id, P2PMessageType type, string message) {
        string newMessage = "";
        switch(type) {
            case P2PMessageType.Invite: {
                    newMessage = MessageHead_Invite + MessageSplitWord + message;
                }
                break;
            case P2PMessageType.Join: {
                    newMessage = MessageHead_Join + MessageSplitWord + message;
                }
                break;
            case P2PMessageType.Chat: {
                    newMessage = MessageHead_Chat + MessageSplitWord + message;
                }
                break;
        }

        byte[] data = Encoding.UTF8.GetBytes(newMessage);
        bool success = SteamNetworking.SendP2PPacket(id, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable);

        if(success) {

        }
        else {
            Debug.LogError($"Failed to send message. id: {id}, name: {SteamFriends.GetFriendPersonaName(id)}, messageType: {type}");
        }
    }

    uint receivedMessageSize;
    byte[] receivedData;
    uint receivedBytes;
    CSteamID receivedId;
    private void ReceivePackets() {
        while(SteamNetworking.IsP2PPacketAvailable(out receivedMessageSize)) {
            receivedData = new byte[receivedMessageSize];
            if(SteamNetworking.ReadP2PPacket(receivedData, receivedMessageSize, out receivedBytes, out receivedId)) {
                HandleMessage(receivedId, Encoding.UTF8.GetString(receivedData));
            }
        }
    }

    string[] receivedMessage;
    private void HandleMessage(CSteamID id, string message) {
        receivedMessage = message.Split(MessageSplitWord);
        if(receivedMessage.Length < 2) {
            Debug.LogError($"Wrong message. count: {receivedMessage.Length}");
        }
        else if(receivedMessage[0] == MessageHead_Invite) {
            HandleMessage_Invite(id, receivedMessage[1]);
        }
        else if(receivedMessage[0] == MessageHead_Join) {
            HandleMessage_Join(id, receivedMessage[1]);
        }
        else if(receivedMessage[0] == MessageHead_Chat) {
            HandleMessage_Chat(id, receivedMessage[1]);
        }
        else {
            Debug.LogError($"Not defined head. head: {receivedMessage[0]}");
        }
    }

    private void HandleMessage_Invite(CSteamID id, string message) {
        if(message == MessageBody_InviteRequest) {
            PopupManager.Instance.SetPopup(
                $"Invited by [{SteamFriends.GetFriendPersonaName(id)}].",
                MessageBody_InviteAccept,
                () => {
                    SendMessage(id, P2PMessageType.Invite, MessageBody_InviteAccept);

                    PopupManager.Instance.SetActive(false);
                },
                MessageBody_InviteReject,
                () => {
                    SendMessage(id, P2PMessageType.Invite, MessageBody_InviteReject);

                    PopupManager.Instance.SetActive(false);
                });
            PopupManager.Instance.SetActive(true);
        }
        else if(message == MessageBody_InviteAccept) {
            joinedFriends.Add(id);
        }
        else if(message == MessageBody_InviteReject) {

        }
        else {
            Debug.LogError($"Invite message is wrong. id: {id}, name: {SteamFriends.GetFriendPersonaName(id)}, message: {message}");
        }
    }

    private void HandleMessage_Join(CSteamID id, string message) {

    }

    private void HandleMessage_Chat(CSteamID id, string message) {
        OnChatReceived?.Invoke(SteamFriends.GetFriendPersonaName(id), message);
    }
}

public enum P2PMessageType {
    Invite, 
    Join, 

    Chat, 

    DataSync, 
}