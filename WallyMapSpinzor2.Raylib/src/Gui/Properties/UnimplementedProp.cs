using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowUnimplementedProp()
    {
        ImGui.PushTextWrapPos();
        ImGui.Text("Properties gui not implemented for this object");
        ImGui.PopTextWrapPos();
        return false;
    }
}