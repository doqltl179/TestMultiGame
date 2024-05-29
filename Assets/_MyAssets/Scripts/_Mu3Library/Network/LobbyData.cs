using Steamworks.Data;

public class LobbyData {
    public const string key_ip = "ip";
    public const string key_port = "port";
    public const string key_ownerId = "ownerId";
    public const string key_ownerName = "ownerName";

    public const string key_roomTitle = "roomTitle";
    public const string key_roomPassword = "roomPassword";



    #region Utility
    public static string GetIP(Lobby lobby) => GetValue(lobby, key_ip);
    public static string GetPort(Lobby lobby) => GetValue(lobby, key_port);
    public static string GetOwnerId(Lobby lobby) => GetValue(lobby, key_ownerId);
    public static string GetOwnerName(Lobby lobby) => GetValue(lobby, key_ownerName);

    public static string GetRoomTitle(Lobby lobby) => GetValue(lobby, key_roomTitle);
    public static string GetRoomPassword(Lobby lobby) => GetValue(lobby, key_roomPassword);

    private static string GetValue(Lobby lobby, string key) {
        return lobby.GetData(key);
    }
    #endregion
}
