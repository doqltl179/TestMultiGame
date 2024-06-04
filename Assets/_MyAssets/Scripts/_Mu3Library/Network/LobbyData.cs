using Steamworks.Data;

public class LobbyData {
    public const string key_ip = "ip";
    public const string key_port = "port";
    public const string key_ownerId = "ownerId";
    public const string key_ownerName = "ownerName";

    public const string key_roomTitle = "roomTitle";
    public const string key_roomPassword = "roomPassword";

    public const string key_isPrivate = "isPrivate";



    private static string GetValue(Lobby lobby, string key) {
        return lobby.GetData(key);
    }

    private static void SetValue(Lobby lobby, string key, string value) {
        lobby.SetData(key, value);
    }

    #region Utility
    public static string GetIP(Lobby lobby) => GetValue(lobby, key_ip);
    public static void SetIP(Lobby lobby, string value) => SetValue(lobby, key_ip, value);

    public static string GetPort(Lobby lobby) => GetValue(lobby, key_port);
    public static void SetPort(Lobby lobby, string value) => SetValue(lobby, key_port, value);

    public static string GetOwnerId(Lobby lobby) => GetValue(lobby, key_ownerId);
    public static void SetOwnerId(Lobby lobby, string value) => SetValue(lobby, key_ownerId, value);

    public static string GetOwnerName(Lobby lobby) => GetValue(lobby, key_ownerName);
    public static void SetOwnerName(Lobby lobby, string value) => SetValue(lobby, key_ownerName, value);

    public static string GetLobbyTitle(Lobby lobby) => GetValue(lobby, key_roomTitle);
    public static void SetLobbyTitle(Lobby lobby, string value) => SetValue(lobby, key_roomTitle, value);

    public static string GetLobbyPassword(Lobby lobby) => GetValue(lobby, key_roomPassword);
    public static void SetLobbyPassword(Lobby lobby, string value) => SetValue(lobby, key_roomPassword, value);

    public static bool GetIsPrivate(Lobby lobby) {
        bool result = false;
        if(bool.TryParse(GetValue(lobby, key_isPrivate), out result)) {

        }

        return result;
    }
    public static void SetIsPrivate(Lobby lobby, bool value) => SetValue(lobby, key_isPrivate, value.ToString());
    #endregion
}
