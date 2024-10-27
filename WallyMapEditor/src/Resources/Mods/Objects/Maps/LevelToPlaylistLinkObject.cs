using System.IO;

namespace WallyMapEditor.Mod;

public sealed class LevelToPlaylistLinkObject
{
    internal enum VersionEnum : byte
    {
        Base = 1,
        LATEST = Base,
    }

    public required string LevelName { get; set; }
    public required string[] Playlists { get; set; }

    internal static LevelToPlaylistLinkObject Get(Stream stream)
    {
        _ = (VersionEnum)stream.GetU8();
        string levelname = stream.GetStr();
        string[] playlists = stream.GetStr().Split(',');
        return new()
        {
            LevelName = levelname,
            Playlists = playlists,
        };
    }

    internal void Put(Stream stream)
    {
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutStr(LevelName);
        stream.PutStr(string.Join(',', Playlists));
    }
}