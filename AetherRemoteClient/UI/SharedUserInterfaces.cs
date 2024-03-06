using AetherRemoteClient.Domain;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System.Numerics;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI;

public class SharedUserInterfaces
{
    // Constants
    public static readonly Vector4 Grey = new(0.5f, 0.5f, 0.5f, 1);
    public static readonly Vector4 Gold = new(1, 0.66f, 0, 1);
    public static readonly Vector4 Red = new(1, 0.33f, 0.33f, 1);
    public static readonly Vector4 Green = new(0.33f, 1, 0.33f, 1);
    public static readonly Vector4 Blue = new(0.33f, 0.33f, 1, 1);
    public static readonly Vector4 White = Vector4.One;

    public static readonly ImGuiWindowFlags PopupWindowFlags =
        ImGuiWindowFlags.NoTitleBar |
        ImGuiWindowFlags.NoMove |
        ImGuiWindowFlags.NoResize;

    private readonly DalamudPluginInterface pluginInterface;
    private readonly IPluginLog logger;

    private static IFontHandle? BigFont;
    private static bool BigFontBuilt = false;
    private const int BigFontSize = 40;
    private const int BigFontDefaultOffset = 8;

    private static IFontHandle? MediumFont;
    private static bool MediumFontBuilt = false;
    private const int MediumFontSize = 24;
    private const int MediumFontDefaultOffset = -4;

    public SharedUserInterfaces(IPluginLog logger, DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        this.logger = logger;

        Task.Run(BuildDefaultFontExtraSizes);
    }

    /// <summary>
    /// Draws an icon with optional color.
    /// </summary>
    public static void Icon(FontAwesomeIcon icon, Vector4? color = null)
    {
        if (color.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, color.Value);
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();
        if (color.HasValue) ImGui.PopStyleColor();
    }

    /// <summary>
    /// Calculates the size a button would be. Useful for UI alignment.
    /// </summary>
    public static Vector2 CalculateIconButtonScaledSize(FontAwesomeIcon icon, float scale = 1.0f)
    {
        ImGui.PushFont(UiBuilder.IconFont);

        var size = ImGui.CalcTextSize(icon.ToIconString()) + (ImGui.GetStyle().FramePadding * 2);
        size.X = size.Y;
        size *= scale;

        ImGui.PopFont();

        return size;
    }

    /// <summary>
    /// Draws a button with an icon.
    /// </summary>
    public static bool IconButtonScaled(FontAwesomeIcon icon, float scale = 1.0f, string? id = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);

        if (id != null)
            ImGui.PushID(id);

        var result = ImGui.Button(icon.ToIconString(), CalculateIconButtonScaledSize(icon, scale));

        if (id != null)
            ImGui.PopID();

        ImGui.PopFont();
        return result;
    }

    /// <summary>
    /// Draws a button with an icon. <br/>
    /// Use this when <see cref="CalculateIconButtonScaledSize"/> is already calculated.
    /// </summary>
    public static bool IconButtonScaled(FontAwesomeIcon icon, Vector2 size, string? id = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);

        if (id != null)
            ImGui.PushID(id);

        var result = ImGui.Button(icon.ToIconString(), size);

        if (id != null)
            ImGui.PopID();

        ImGui.PopFont();
        return result;
    }

    /// <summary>
    /// Draws text using the medium font with optional color.
    /// </summary>
    public static void MediumText(string text, Vector4? color = null)
    {
        FontText(text, MediumFont, MediumFontBuilt, color);
    }

    /// <summary>
    /// Draws text using the big font with optional color.
    /// </summary>
    public static void BigText(string text, Vector4? color = null)
    {
        FontText(text, BigFont, BigFontBuilt, color);
    }

    /// <summary>
    /// Draws text using the default font, centered, with optional color.
    /// </summary>
    public static void TextCentered(string text, int yOffset = 0, Vector4? color = null)
    {
        FontTextCentered(text, null, false, yOffset, color);
    }

    /// <summary>
    /// Draws text using the medium font, centered, with optional color.
    /// </summary>
    public static void MediumTextCentered(string text, int yOffset = 0, Vector4? color = null)
    {
        FontTextCentered(text, MediumFont, MediumFontBuilt, yOffset, color);
    }

    /// <summary>
    /// Draws text using the big font, centered, with optional color.
    /// </summary>
    public static void BigTextCentered(string text, int yOffset = 0, Vector4? color = null)
    {
        FontTextCentered(text, BigFont, BigFontBuilt, yOffset, color);
    }

    /// <summary>
    /// Displays centered text with best fitting font size.
    /// </summary>
    public static void DynamicTextCentered(string text, float workingSpace, Vector4? color = null)
    {
        if (BigFontBuilt)
        {
            BigFont?.Push();
            var bigTextWidth = ImGui.CalcTextSize(text).X;
            BigFont?.Pop();

            if (bigTextWidth <= workingSpace)
            {
                BigTextCentered(text, BigFontDefaultOffset, color);
                return;
            }
        }

        if (MediumFontBuilt)
        {
            MediumFont?.Push();
            var mediumTextWidth = ImGui.CalcTextSize(text).X;
            MediumFont?.Pop();

            if (mediumTextWidth <= workingSpace)
            {
                MediumTextCentered(text, MediumFontDefaultOffset, color);
                return;
            }
        }

        TextCentered(text, 0, color);
    }

    /// <summary>
    /// Draws text with a given color.
    /// </summary>
    public static void ColorText(string text, Vector4 color)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
    }

    /// <summary>
    /// Text wrap supporting huge words, and colors using & followed by another letter or number.
    /// Refer to _colorMap dictionary in SharedUserIntefaces for color information.
    /// </summary>
    public static void WrappedTextColor(string text)
    {
        var pushColorCount = 0;
        var writeableScreenSpace = ImGui.GetWindowWidth() - 10;
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(3, 0));

        var words = text.Split(' ');
        for (var i = 0; i < words.Length; i++)
        {
            var word = words[i];
            if (word[0] == '&' && word.Length == 2)
            {
                var color = ConvertToColor(word[1]);
                ImGui.PushStyleColor(ImGuiCol.Text, color);
                pushColorCount++;
                continue;
            }

            var wordWidth = ImGui.CalcTextSize(word).X;
            if (ImGui.GetCursorPosX() + wordWidth > writeableScreenSpace)
            {
                if (word.Length < 16)
                {
                    ImGui.NewLine();
                    ImGui.TextUnformatted(word);
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, Grey);

                    var condensedWord = $"{word[..4]}...{word[(word.Length - 4)..]}";
                    var condensedWordWidth = ImGui.CalcTextSize(condensedWord).X;

                    if (ImGui.GetCursorPosX() + condensedWordWidth > writeableScreenSpace)
                    {
                        ImGui.NewLine();
                    }

                    ImGui.TextUnformatted(condensedWord);

                    ImGui.PopStyleColor();

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.PopStyleVar();
                        ImGui.BeginTooltip();
                        ImGui.PushStyleColor(ImGuiCol.Text, Grey);
                        ImGui.TextUnformatted("Full word will shown in game, this is just a preview");
                        ImGui.PopStyleColor();
                        ImGui.Separator();
                        ImGui.TextUnformatted(word);
                        ImGui.EndTooltip();
                        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(3, 0));
                    }
                }
            }
            else
            {
                ImGui.TextUnformatted(word);
            }

            if (i < words.Length - 1) ImGui.SameLine();
        }

        for (var i = 0; i < pushColorCount; i++)
        {
            ImGui.PopStyleColor();
        }

        ImGui.PopStyleVar();
    }

    /// <summary>
    /// Input text with searchable list.
    /// </summary>
    public static void ComboFilter(
        string id,
        ref string selectedString,
        ThreadedFilter<string> filterHelper,
        ImGuiWindowFlags? imGuiWindowFlags = null)
    {
        var _sizeX = 170;
        var _sizeY = filterHelper.List.Count < 10 ? (filterHelper.List.Count * 25) + 10 : 260;
        // TODO: Fix sizing bug when searching through options.
        // For example, when scanning through emotes, if you search "ma" you can see it clearly

        ImGui.SetNextItemWidth(_sizeX);
        if (ImGui.InputText($"##{id}-ComboFilter", ref selectedString, 32))
        {
            filterHelper.Restart(selectedString);
        }
        var isInputTextActive = ImGui.IsItemActive();
        var isInputTextActivated = ImGui.IsItemActivated();

        var popupName = $"##{id}-Popup";

        if (isInputTextActivated && !ImGui.IsPopupOpen(popupName))
        {
            ImGui.OpenPopup(popupName);
        }

        var _x = ImGui.GetItemRectMin().X;
        var _y = ImGui.GetCursorPosY() + ImGui.GetWindowPos().Y;
        ImGui.SetNextWindowPos(new Vector2(_x, _y));

        ImGui.SetNextWindowSize(new Vector2(_sizeX, _sizeY));

        if (ImGui.BeginPopup(popupName, imGuiWindowFlags ?? (PopupWindowFlags | ImGuiWindowFlags.ChildWindow)))
        {
            for (var i = 0; i < filterHelper.List.Count; i++)
            {
                var option = filterHelper.List[i];
                if (ImGui.Selectable(option))
                {
                    selectedString = option;
                }
            }

            if (!isInputTextActive && !ImGui.IsWindowFocused())
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Draws text using a specific font with optional color.
    /// </summary>
    private static void FontText(string text, IFontHandle? font, bool fontBuilt, Vector4? color = null)
    {
        if (color.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, color.Value);
        if (fontBuilt) font?.Push();
        ImGui.TextUnformatted(text);
        if (fontBuilt) font?.Pop();
        if (color.HasValue) ImGui.PopStyleColor();
    }

    /// <summary>
    /// Draws text using a specific font, centered, with optional color.
    /// </summary>
    private static void FontTextCentered(string text, IFontHandle? font, bool fontBuilt, int yOffset = 0, Vector4? color = null)
    {
        if (fontBuilt) font?.Push();

        var userIdColor = color ?? White;
        var windowWidth = ImGui.GetWindowSize().X;
        var userIdWidth = ImGui.CalcTextSize(text).X;

        ImGui.SetCursorPosX((windowWidth - userIdWidth) * 0.5f);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - yOffset);

        ColorText(text, userIdColor);

        if (fontBuilt) font?.Pop();
    }

    /// <summary>
    /// Converts a char into a Vector4 color.
    /// </summary>
    private static Vector4 ConvertToColor(char colorCode)
    {
        return colorCode switch
        {
            'f' => White,
            '6' => Gold,
            'c' => Red,
            'a' => Green,
            '9' => Blue,
            '7' => Grey,
            _ => White
        };
    }

    private async Task BuildDefaultFontExtraSizes()
    {
        BigFont = pluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit => {
            toolkit.OnPreBuild(preBuild =>
            {
                preBuild.AddDalamudDefaultFont(BigFontSize);
            });
        });

        MediumFont = pluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit => {
            toolkit.OnPreBuild(preBuild =>
            {
                preBuild.AddDalamudDefaultFont(MediumFontSize);
            });
        });

        await BigFont.WaitAsync();
        await MediumFont.WaitAsync();
        await pluginInterface.UiBuilder.FontAtlas.BuildFontsAsync();

        BigFontBuilt = true;
        MediumFontBuilt = true;
    }
}
