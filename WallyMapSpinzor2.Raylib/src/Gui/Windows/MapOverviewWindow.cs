using System.Collections;
using ImGuiNET;
using Rl = Raylib_cs.Raylib;
using Raylib_cs;

namespace WallyMapSpinzor2.Raylib;

public class MapOverviewWindow
{
    private bool _open = true;
    public bool Open { get => _open; set => _open = value; }

    private bool _propChanged = false;

    public void Show(Level l, CommandHistory cmd, ref object? selected)
    {
        ImGui.Begin("Map Overview", ref _open);

        if (_propChanged && Rl.IsMouseButtonReleased(MouseButton.Left))
        {
            _propChanged = false;
            cmd.SetAllowMerge(false);
        }

        if (l.Type is not null)
        {
            ImGui.Text($"LevelName: {l.Type.LevelName}");
            ImGui.Text($"AssetName: {l.Type.AssetName}");
            ImGui.Text($"FileName: {l.Type.FileName}");

            l.Type.DisplayName = ImGuiExt.InputText("DisplayName", l.Type.DisplayName);
        }

        _propChanged |= ImGuiExt.DragFloatHistory("default SlowMult##overview", l.Desc.SlowMult, val => l.Desc.SlowMult = val, cmd, speed: 0.05);
        _propChanged |= ImGuiExt.DragIntHistory("default NumFrames##overview", l.Desc.NumFrames, val => l.Desc.NumFrames = val, cmd, minValue: 0);

        if (l.Type is not null && l.Type.LevelName != "Random")
        {
            if (ImGui.CollapsingHeader("Kill Bounds##overview"))
            {
                _propChanged |= ImGuiExt.DragIntHistory("Top##killbounds", l.Type.TopKill!.Value, val => l.Type.TopKill = val, cmd, minValue: 1);
                _propChanged |= ImGuiExt.DragIntHistory("Bottom##killbounds", l.Type.BottomKill!.Value, val => l.Type.BottomKill = val, cmd, minValue: 1);
                _propChanged |= ImGuiExt.DragIntHistory("Left##killbounds", l.Type.LeftKill!.Value, val => l.Type.LeftKill = val, cmd, minValue: 1);
                _propChanged |= ImGuiExt.DragIntHistory("Right##killbounds", l.Type.RightKill!.Value, val => l.Type.RightKill = val, cmd, minValue: 1);
            }
        }

        if (ImGui.CollapsingHeader("Camera Bounds##overview"))
        {
            _propChanged |= PropertiesWindow.ShowCameraBoundsProps(l.Desc.CameraBounds, cmd);
        }

        if (ImGui.CollapsingHeader("Spawn Bot Bounds##overview"))
        {
            _propChanged |= PropertiesWindow.ShowSpawnBotBoundsProps(l.Desc.SpawnBotBounds, cmd);
        }

        if (l.Type is not null && ImGui.CollapsingHeader("Weapon Spawn Color##overview") && l.Type.CrateColorA is not null && l.Type.CrateColorB is not null)
        {
            Color colA = ImGuiExt.ColorPicker3("Outer##crateColorA", new(l.Type.CrateColorA.Value.R, l.Type.CrateColorA.Value.G, l.Type.CrateColorA.Value.B, 255));
            Color colB = ImGuiExt.ColorPicker3("Inner##crateColorB", new(l.Type.CrateColorB.Value.R, l.Type.CrateColorB.Value.G, l.Type.CrateColorB.Value.B, 255));
            CrateColor crateColA = new(colA.R, colA.G, colA.B);
            CrateColor crateColB = new(colB.R, colB.G, colB.B);

            if (crateColA != l.Type.CrateColorA)
            {
                _propChanged = true;
                cmd.Add(new CrateColorChange(l.Type, crateColA, false));
            }

            if (crateColB != l.Type.CrateColorB)
            {
                _propChanged = true;
                cmd.Add(new CrateColorChange(l.Type, crateColB, true));
            }
        }

        if (ImGui.CollapsingHeader("Images##overview"))
        {
            ShowSelectableList(l.Desc.Backgrounds, ref selected);
        }

        ImGui.Separator();

        if (ImGui.CollapsingHeader("Assets##overview"))
        {
            TeamScoreboard? ts = l.Desc.TeamScoreboard;
            if (ts is not null && ImGui.Selectable($"{ts.GetType().Name} {GetExtraObjectInfo(ts)}##selectable{ts.GetHashCode()}", selected == ts))
            {
                selected = ts;
            }
            ShowSelectableList(l.Desc.Assets, ref selected);
            ShowSelectableList(l.Desc.LevelAnims, ref selected);
            ShowSelectableList(l.Desc.AnimatedBackgrounds, ref selected);
            ShowSelectableList(l.Desc.LevelAnimations, ref selected);
        }

        if (ImGui.CollapsingHeader("Collisions##overview"))
        {
            ShowSelectableList(l.Desc.Collisions, ref selected);
            ShowSelectableList(l.Desc.DynamicCollisions, ref selected);
        }

        if (ImGui.CollapsingHeader("Respawns##overview"))
        {
            ShowSelectableList(l.Desc.Respawns, ref selected);
            ShowSelectableList(l.Desc.DynamicRespawns, ref selected);
        }

        if (ImGui.CollapsingHeader("Item Spawns##overview"))
        {
            ShowSelectableList(l.Desc.ItemSpawns, ref selected);
            ShowSelectableList(l.Desc.DynamicItemSpawns, ref selected);
        }

        if (ImGui.CollapsingHeader("Volumes##overview"))
        {
            ShowSelectableList(l.Desc.Volumes, ref selected);
        }

        if (ImGui.CollapsingHeader("Nav Nodes##overview"))
        {
            ShowSelectableList(l.Desc.NavNodes, ref selected);
            ShowSelectableList(l.Desc.DynamicNavNodes, ref selected);
        }

        if (ImGui.CollapsingHeader("Sounds##overview"))
        {
            ShowSelectableList(l.Desc.LevelSounds, ref selected);
        }

        if (ImGui.CollapsingHeader("Horde##overview"))
        {
            ShowSelectableList(l.Desc.WaveDatas, ref selected);
        }

        ImGui.End();
    }

    private static void ShowSelectableList(IEnumerable list, ref object? selected)
    {
        foreach (object o in list)
        {
            if (ImGui.Selectable($"{o.GetType().Name} {GetExtraObjectInfo(o)}##selectable{o.GetHashCode()}", selected == o))
                selected = o;
        }
    }

    public static string GetExtraObjectInfo(object o) => o switch
    {
        Background b => $"({b.AssetName ?? b.AnimatedAssetName})",
        Platform p => $"({p.InstanceName})",
        AnimatedBackground ab => $"({ab.Gfx.AnimClass})",
        Gfx g => $"({g.AnimClass})",
        CustomArt ca => $"({ca.Name})",
        ColorSwap cs => $"({cs.OldColor:X8}->{cs.NewColor:X8})",
        LevelAnim la => $"({la.InstanceName})",

        MovingPlatform mp => $"({mp.PlatID})",
        Respawn r => $"({r.X}, {r.Y})",
        AbstractItemSpawn i => $"({i.X}, {i.Y}, {i.W}, {i.H})",
        AbstractCollision c => $"({c.X1}, {c.Y1}, {c.X2}, {c.Y2})",
        AbstractVolume v => $"(team {v.Team} - {v.X}, {v.Y}, {v.W}, {v.H})",
        NavNode n => $"({PropertiesWindow.GetNavTypeString(n.Type)}{n.NavID})",

        LevelSound ls => $"({ls.SoundEventName})",

        WaveData w => $"({w.ID})",
        CustomPath cp => $"({cp.Points.Length} points)",
        Point p => $"({p.X}, {p.Y})",
        Group g => $"({g.GetCount(2)}/{g.GetCount(3)}/{g.GetCount(4)} {PropertiesWindow.GetBehaviorString(g.Behavior)})",

        DynamicCollision dc => $"({dc.PlatID})",
        DynamicItemSpawn di => $"({di.PlatID})",
        DynamicRespawn dr => $"({dr.PlatID})",
        DynamicNavNode dn => $"({dn.PlatID})",

        AbstractKeyFrame kf => $"({PropertiesWindow.FirstKeyFrameNum(kf)})",
        _ => ""
    };
}