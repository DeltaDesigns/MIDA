using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Tiger;
using Tiger.Schema.Investment;

namespace MIDA;

/// <summary>
/// Interaction logic for Tooltip2.xaml
/// </summary>
public partial class Tooltip2 : UserControl, INotifyPropertyChanged
{
    //public Investment Investment => Investment.Get();
    public dynamic? ActiveItem;

    private DestinyTooltipStyle _tooltipStyle = DestinyTooltipStyle.None;
    public DestinyTooltipStyle TooltipStyle
    {
        get => _tooltipStyle;
        set
        {
            if (_tooltipStyle != value)
            {
                _tooltipStyle = value;
                OnPropertyChanged(nameof(TooltipStyle));
            }
        }
    }

    private Color _bodyColor = System.Windows.Media.Color.FromArgb(255, 52, 51, 56);
    public Color BodyColor
    {
        get => _bodyColor;
        set
        {
            if (_bodyColor != value)
            {
                _bodyColor = value;
                OnPropertyChanged(nameof(BodyColor));
            }
        }
    }

    private HeaderBlock _header;
    public HeaderBlock Header
    {
        get => _header;
        set
        {
            if (_header != value)
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }
    }

    public TranslateTransform ExtraOffset = new(0, 0);

    public ObservableCollection<ToolTipBlock> BodyBlocks { get; } = new();
    public ObservableCollection<ToolTipBlock> InputBlocks { get; } = new();

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    private static readonly HashSet<string> BlacklistedNotifications = new()
    {
        "This weapon can be enhanced further.",
        "This weapon is fully enhanced.",
        "This weapon can be enhanced further and modified at the Relic on Mars.",
        "This weapon is fully enhanced and can be modified at the Relic on Mars.",
        "Deepsight activation is available for this weapon.",
        "Deepsight activation is not available for this weapon instance."
    };

    public Tooltip2()
    {
        InitializeComponent();
        DataContext = this;

        CompositionTarget.Rendering += OnRender;
    }

    public void MakeTooltip(GenericTooltip item)
    {
        ClearTooltip();

        var blocks = new List<ToolTipBlock>();

        //if (item.Style == HeaderBlock.HeaderStyle.Item)
        //{
        //    HeaderColor = MarathonTierType.Legendary.GetColor();
        //    BodyColor = MarathonTierType.Legendary.GetBodyColor();
        //}
        //else
        //{
        //    HeaderColor = Color.FromArgb(255, 0, 0, 0);
        //    BodyColor = Color.FromArgb(255, 0x1C, 0x1C, 0x1C);
        //}


        BodyColor = Color.FromArgb(255, 52, 51, 56);

        Header = new()
        {
            HeaderColor = Color.FromArgb(255, 49, 48, 53),
            Style = item.Style,
            Name = item.Name,
            Type = item.Type,
            Label = item.Label,
            HideTopBar = item.Style == HeaderBlock.HeaderStyle.Item,
            CollapseEmpty = true,
        };

        // Spacer Block
        blocks.Add(new SpacerBlock()
        {
            Order = -1,
            ShowBar = true
        });

        // Description
        blocks.Add(new TextsBlock()
        {
            Order = 0,
            Text = item.Description
        });

        foreach (var block in blocks)
            BodyBlocks.Add(block);

        ExtraOffset = item.ExtraOffset;

        ShowTooltip();
    }

    public async void MakeTooltip(InventoryItem item, DestinySocketCategoryStyle overrideStyle = DestinySocketCategoryStyle.Unknown)
    {
        ClearTooltip();
        var blocks = new List<ToolTipBlock>();
        var inputBlocks = new List<ToolTipBlock>();

        var rarity = item.Rarity;
        var strings = item.GetItemStrings();

        TooltipStyle = strings.TagData.TooltipStyle;
        //BodyColor = rarity.GetBodyColor();
        Header = new()
        {
            HeaderColor = rarity.GetColor(),
            Hash = item.ApiHash,
            Icon = ApiImageUtils.GetPlugWatermark(item),
            Name = item.Name,
            Type = item.Type,
            //DamageType = DestinyDamageType.GetDamageType(item.GetItemDamageTypeIndex()),
            Label = rarity != MarathonTierType.None ? $"{rarity} " : "",
            Glyph = item.TypeGlyph,
            SellValue = item.SellValue != -1 ? item.SellValue.ToString("N0") : string.Empty,

            LabelTextColor = rarity == MarathonTierType.Dynamic ? Color.FromScRgb(1, 1, 1, 1) : Color.FromScRgb(1, 0, 0, 0),

            //TextColor = rarity != MarathonTierType.Common ? Color.FromScRgb(1, 1, 1, 1) : Color.FromScRgb(1, 0, 0, 0),
            //LabelColor = rarity.GetLabelColor()
        };

        if (Keyboard.IsKeyDown(Key.LeftShift))
            Header.Type = $"{Header.Type} : {item.ApiHash}";

        if (TooltipStyle == DestinyTooltipStyle.Build)
        {
            //HeaderColor = MarathonTierType.Unknown.GetColor();
            //BodyColor = MarathonTierType.Unknown.GetBodyColor();

            Header.Label = null;
            Header.TextColor = Color.FromScRgb(1, 1, 1, 1);
            //Header.LabelColor = MarathonTierType.Unknown.GetLabelColor();
        }

        switch (overrideStyle)
        {
            case DestinySocketCategoryStyle.Reusable:
                Header.HeaderColor = Color.FromArgb(255, 0, 0, 0);
                BodyColor = Color.FromArgb(255, 52, 51, 56);

                Header.Label = ""; // TODO
                Header.TextColor = Color.FromScRgb(1, 1, 1, 1);
                //Header.LabelColor = MarathonTierType.Unknown.GetLabelColor();
                Header.HideTopBar = false;
                break;
            case DestinySocketCategoryStyle.Consumable:
                Header.CollapseEmpty = true;
                Header.TextColor = Color.FromScRgb(1, 1, 1, 1);
                //if (rarity <= MarathonTierType.Common)
                //{
                //    HeaderColor = MarathonTierType.Unknown.GetColor();
                //    BodyColor = MarathonTierType.Unknown.GetBodyColor();
                //    Header.LabelColor = MarathonTierType.Unknown.GetLabelColor();
                //}
                break;
        }

        await Task.Run(() =>
        {
            // Spacer Block
            blocks.Add(new SpacerBlock()
            {
                Order = -1,
                ShowBar = true
            });

            // Description
            blocks.Add(new TextsBlock()
            {
                Order = 0,
                Text = item.Description
            });

            // Flavor Text
            //blocks.Add(new TextsBlock()
            //{
            //    Order = 1,
            //    Italic = true,
            //    Text = item.GetItemFlavorText()
            //});


            // Emblem preview
            //if (item.ItemTraits.Contains(DestinyTraitID.item_emblem))
            //{
            //    blocks.Add(new EmblemBlock()
            //    {
            //        Emblem = ApiImageUtils.MakeIcon(strings.TagData.EmblemContainerIndex)
            //    });
            //}

            // Energy Mod Cost
            //if (strings.TagData.DisplayStyle == DestinyUIDisplayStyle.EnergyMod)
            //{
            //    foreach (S86738080 stat in ((S81738080)item.TagData.Unk78.GetValue(item.GetReader())).InvestmentStats)
            //    {
            //        S6F588080 statItem = Investment.Get().StatStrings[stat.StatTypeIndex];
            //        if (statItem.StatHash.Hash32 is 3578062600 or 514071887)
            //        {
            //            blocks.Add(new EnergyModBlock()
            //            {
            //                Order = 2,
            //                Icon = ApiImageUtils.MakeIcon(statItem.StatIconIndex, iconIndex: 3),
            //                Cost = stat.Value
            //            });
            //        }
            //    }
            //}


            if (overrideStyle == DestinySocketCategoryStyle.Unknown)
            {
                // Source
                //if (item.Source != null && item.Source != "")
                //{
                //    blocks.Add(new SpacerBlock()
                //    {
                //        Order = 5,
                //        Height = 15,
                //        ShowBar = true
                //    });

                //    blocks.Add(new TextsBlock()
                //    {
                //        Order = 6,
                //        Text = item.Source
                //    });
                //}
            }


            if (overrideStyle != DestinySocketCategoryStyle.Reusable)
            {
                // Perks
                //if (item.TagData.Unk78.GetValue(item.GetReader()) is S81738080 stats)
                //{
                //    foreach (S87738080 perk in stats.Perks)
                //    {
                //        S33548080 perkStrings = Investment.Get().SandboxPerkStrings[perk.PerkIndex];
                //        if (perkStrings.IconIndex == -1)
                //            continue;

                //        blocks.Add(new PerkBlock()
                //        {
                //            Order = 7,
                //            Description = perkStrings.SandboxPerkDescription,
                //            Icon = ApiImageUtils.MakeIcon(perkStrings.IconIndex, 0, 0, 1)
                //        });
                //    }
                //}

                // Tooltip Notifications
                //if (strings.TagData.TooltipNotifications.Any())
                //{
                //    blocks.Add(new SpacerBlock()
                //    {
                //        Order = 8,
                //        ShowBar = true,
                //        BarOpacity = 0.1f
                //    });

                //    foreach (SB2548080 notif in strings.TagData.TooltipNotifications)
                //    {
                //        string? message = notif.DisplayString.Value;
                //        if (string.IsNullOrWhiteSpace(message))
                //            continue;

                //        if (BlacklistedNotifications.Contains(message))
                //            continue;

                //        blocks.Add(new NotificationBlock()
                //        {
                //            Order = 9,
                //            Text = message,
                //            Style = notif.DisplayStyle
                //        });
                //    }
                //}


                // Input blocks
                //if (strings.TagData.Unk40.GetValue(strings.GetReader()) is SD7548080 preview)
                //{
                //    inputBlocks.Add(new InputBlock()
                //    {
                //        Order = 0,
                //        Key = $"", // Key glyph
                //        KeyPress = $"", // 2nd key glyph (mouse left/right)
                //        Action = $"{(preview.PreviewActionString.Value ?? "Details")}"
                //    });

                //    Dispatcher.Invoke(() =>
                //    {
                //        if (MainWindow.Current.CurrentTab is not null
                //        && MainWindow.Current.CurrentTab.Content is CategoryView
                //        && DareView2.ShouldAddToList(item))
                //            inputBlocks.Add(new InputBlock()
                //            {
                //                Order = 1,
                //                Key = $"",
                //                Action = $"Export"
                //            });
                //    });
                //}
            }
        });


        foreach (var block in blocks)
            BodyBlocks.Add(block);

        foreach (var block in inputBlocks)
            InputBlocks.Add(block);

        ShowTooltip();
    }

    //public void MakeTooltip(Category item)
    //{
    //    ClearTooltip();
    //    var blocks = new List<ToolTipBlock>();

    //    HeaderColor = Color.FromArgb(255, 0, 0, 0);
    //    BodyColor = Color.FromArgb(255, 0x1C, 0x1C, 0x1C);
    //    Header = new()
    //    {
    //        Style = HeaderBlock.HeaderStyle.Category,
    //        Name = item.ItemCategoryName,
    //        Type = item.ItemCategoryType,
    //        HideTopBar = false
    //        //Label = item.ItemCategoryDescription,
    //    };

    //    // Spacer Block
    //    blocks.Add(new SpacerBlock()
    //    {
    //        Order = -1,
    //    });

    //    // Description
    //    blocks.Add(new TextsBlock()
    //    {
    //        Order = 0,
    //        Text = item.ItemCategoryDescription
    //    });

    //    foreach (var block in blocks)
    //        BodyBlocks.Add(block);

    //    ShowTooltip();
    //}

    //public async void MakeTooltip(CategoryEntry item)
    //{
    //    ClearTooltip();
    //    var blocks = new List<ToolTipBlock>();

    //    HeaderColor = Color.FromArgb(255, 0, 0, 0);
    //    BodyColor = Color.FromArgb(255, 0x1C, 0x1C, 0x1C);

    //    Header = new()
    //    {
    //        Name = item.ItemName,
    //        Type = item.ItemType,
    //        HideTopBar = false,
    //    };

    //    await Task.Run(() =>
    //    {
    //        // Spacer Block
    //        blocks.Add(new SpacerBlock()
    //        {
    //            Order = -1,
    //        });

    //        // Description
    //        blocks.Add(new TextsBlock()
    //        {
    //            Order = 0,
    //            Text = item.ItemDescription
    //        });


    //        if (item.EntryType == CategoryEntryType.Record)
    //        {
    //            // Objectives
    //            if (item.Objectives.Count != 0)
    //            {
    //                blocks.Add(new SpacerBlock()
    //                {
    //                    Order = 3,
    //                    Height = 15,
    //                    ShowBar = true
    //                });

    //                foreach (var index in item.Objectives)
    //                {
    //                    var obj = AddObjective(index);
    //                    if (obj is not null)
    //                    {
    //                        obj.Order = 4;
    //                        blocks.Add(obj);
    //                    }
    //                }

    //                blocks.Add(new SpacerBlock()
    //                {
    //                    Order = 5,
    //                    Height = 5,
    //                });
    //            }

    //            // Interval Objectives
    //            if (item.IntervalObjectives.Count != 0)
    //            {
    //                blocks.Add(new SpacerBlock()
    //                {
    //                    Order = 3,
    //                    Height = 15,
    //                    ShowBar = true
    //                });

    //                for (int i = 0; i < item.IntervalObjectives.Count; i++)
    //                {
    //                    var index = item.IntervalObjectives[i];
    //                    var obj = AddObjective(index);
    //                    if (obj is not null)
    //                    {
    //                        obj.Order = 4;
    //                        obj.IsInterval = true;
    //                        obj.IntervalIndex = i + 1;
    //                        blocks.Add(obj);
    //                    }
    //                }

    //                blocks.Add(new SpacerBlock()
    //                {
    //                    Order = 5,
    //                    Height = 5,
    //                });
    //            }

    //            // Rewards
    //            if (item.Rewards.Count != 0)
    //            {
    //                RewardBlock rewards = new();
    //                rewards.RewardOnComplete = item.RewardOnComplete;
    //                if (item.RewardOnComplete)
    //                    rewards.BodyColorOverride = Color.FromArgb(255, 0x32, 0x2c, 0x1e);

    //                foreach (var reward in item.Rewards)
    //                {
    //                    if (reward.Item is null)
    //                        continue;
    //                    rewards.Rewards.Add(new RewardBlock()
    //                    {
    //                        ItemName = reward.ItemName,
    //                        Icon = reward.ItemIcon,
    //                        LargeIcon = reward.ItemIcon2,
    //                        IsEmblem = reward.Item.ItemTraits.Contains(DestinyTraitID.item_emblem)
    //                    });
    //                }
    //                blocks.Add(rewards);
    //            }

    //            // Interval Rewards
    //            if (item.IntervalRewards.Count != 0)
    //            {
    //                RewardBlock rewards = new();
    //                rewards.RewardOnComplete = item.RewardOnComplete;
    //                if (item.RewardOnComplete)
    //                    rewards.BodyColorOverride = Color.FromArgb(255, 0x32, 0x2c, 0x1e);

    //                foreach (var reward in item.IntervalRewards)
    //                {
    //                    if (reward.Item is null)
    //                        continue;

    //                    rewards.Rewards.Add(new RewardBlock()
    //                    {
    //                        IsInterval = true,
    //                        IntervalIndex = reward.IntervalIndex + 1,
    //                        ItemName = reward.ItemName,
    //                        Icon = reward.ItemIcon,
    //                        LargeIcon = reward.ItemIcon2,
    //                        IsEmblem = reward.Item.ItemTraits.Contains(DestinyTraitID.item_emblem)
    //                    });
    //                }
    //                blocks.Add(rewards);
    //            }
    //        }


    //    });

    //    foreach (var block in blocks)
    //        BodyBlocks.Add(block);

    //    ShowTooltip();
    //}

    public void ShowTooltip()
    {
        Dispatcher.Invoke(() =>
        {
            Visibility = Visibility.Visible;
        });
    }

    public void ClearTooltip()
    {
        ExtraOffset = new(0, 0);
        Visibility = Visibility.Collapsed;
        Header = null;
        BodyBlocks.Clear();
        InputBlocks.Clear();
    }

    //public ObjectiveBlock AddObjective(int index)
    //{
    //    S50588080? obj = Investment.Get().GetObjective(index);
    //    if (obj is null || obj.Value.ProgressDescription.Value is null)
    //        return null;

    //    ObjectiveBlock objBlock = new()
    //    {
    //        Order = 4,
    //        Icon = obj.Value.IconIndex != -1 ? ApiImageUtils.MakeIcon(obj.Value.IconIndex) : null,
    //        Description = obj.Value.ProgressDescription.Value,
    //        Value = Investment.Get().GetObjectiveValue(index),
    //        Style = (DestinyUnlockValueUIStyle)obj.Value.InProgressValueStyle
    //    };

    //    if (objBlock.Style == DestinyUnlockValueUIStyle.Automatic)
    //    {
    //        if (objBlock.Value == 1)
    //            objBlock.Style = DestinyUnlockValueUIStyle.Checkbox;
    //    }

    //    return objBlock;
    //}

    private void OnRender(object sender, EventArgs e)
    {
        if (ActiveItem == null && ToolTip.Visibility == Visibility.Visible)
            ClearTooltip();

        if (ActiveItem == null || ToolTip.Visibility != Visibility.Visible || ActualHeight == 0)
            return;

        if (ActiveItem is not FrameworkElement item)
            return;

        if (item.FindCommonVisualAncestor(this) == null)
            return;

        Point activeItemPosition = item.TransformToVisual(this).Transform(new Point(0, 0));

        const float padding = 10f;
        float xOffset = (float)item.ActualWidth + padding;
        float yOffset = 0;

        // if tooltip extends past right side of screen
        if ((activeItemPosition.X + item.ActualWidth) + ToolTip.ActualWidth >= ActualWidth)
            xOffset = (float)(-ToolTip.ActualWidth - padding);

        // if tooltip extends past left side of screen
        if ((activeItemPosition.X - item.ActualWidth) + ToolTip.ActualWidth <= 0)
        {
            xOffset = 0;
            yOffset = (float)(item.ActualHeight + padding);
        }

        // if tooltip height extends past bottom of screen
        if ((activeItemPosition.Y + item.ActualHeight) + ToolTip.ActualHeight >= ActualHeight)
            yOffset = (float)(-ToolTip.ActualHeight + item.ActualHeight);

        // if tooltip height extends past top of screen
        if ((activeItemPosition.Y + item.ActualHeight) - ToolTip.ActualHeight <= 0)
            yOffset = (float)(-activeItemPosition.Y + padding);

        TooltipTranslate.X = activeItemPosition.X + xOffset;
        TooltipTranslate.Y = activeItemPosition.Y + yOffset;
    }
}

public abstract class ToolTipBlock : MIDAUIElement, INotifyPropertyChanged
{
    public int Order { get; set; }
    public Color? BodyColorOverride { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}

public class HeaderBlock : ToolTipBlock
{
    private uint _hash;
    public uint Hash
    {
        get => _hash;
        set
        {
            if (_hash != value)
            {
                _hash = value;
                OnPropertyChanged(nameof(Hash));
            }
        }
    }

    private ImageSource _icon;
    public ImageSource Icon
    {
        get => _icon;
        set
        {
            if (_icon != value)
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }
    }

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value.ToUpper();
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    private string _type;
    public string Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }
    }

    private string _label;
    public string Label
    {
        get => _label;
        set
        {
            if (_label != value)
            {
                _label = value;
                OnPropertyChanged(nameof(Label));
            }
        }
    }

    private Color _labelTextColor = System.Windows.Media.Color.FromScRgb(1f, 0f, 0f, 0f);
    public Color LabelTextColor
    {
        get => _labelTextColor;
        set
        {
            if (_labelTextColor != value)
            {
                _labelTextColor = value;
                OnPropertyChanged(nameof(LabelTextColor));
            }
        }
    }

    private string _glyph;
    public string Glyph
    {
        get => _glyph;
        set
        {
            if (_glyph != value)
            {
                _glyph = value;
                OnPropertyChanged(nameof(Glyph));
            }
        }
    }

    private string _sellValue;
    public string SellValue
    {
        get => _sellValue;
        set
        {
            if (_sellValue != value)
            {
                _sellValue = value;
                OnPropertyChanged(nameof(SellValue));
            }
        }
    }

    private DestinyDamageTypeEnum _damageType;
    public DestinyDamageTypeEnum DamageType
    {
        get => _damageType;
        set
        {
            if (_damageType != value)
            {
                _damageType = value;
                OnPropertyChanged(nameof(DamageType));
            }
        }
    }

    private HeaderStyle _style = HeaderStyle.Item;
    public HeaderStyle Style
    {
        get => _style;
        set
        {
            if (_style != value)
            {
                _style = value;
                OnPropertyChanged(nameof(Style));
            }
        }
    }

    private Color _headerColor = System.Windows.Media.Color.FromScRgb(1, 0, 0, 0);
    public Color HeaderColor
    {
        get => _headerColor;
        set
        {
            if (_headerColor != value)
            {
                _headerColor = value;
                OnPropertyChanged(nameof(HeaderColor));
            }
        }
    }

    private Color _textColor = System.Windows.Media.Color.FromScRgb(1f, 1f, 1f, 1f);
    public Color TextColor
    {
        get => _textColor;
        set
        {
            if (_textColor != value)
            {
                _textColor = value;
                OnPropertyChanged(nameof(TextColor));
            }
        }
    }

    private Color _labelColor = System.Windows.Media.Color.FromScRgb(0.8f, 0, 0, 0);
    public Color LabelColor
    {
        get => _labelColor;
        set
        {
            if (_labelColor != value)
            {
                _labelColor = value;
                OnPropertyChanged(nameof(LabelColor));
            }
        }
    }

    public bool _hideTopBar = true;
    public bool HideTopBar
    {
        get => _hideTopBar;
        set
        {
            if (_hideTopBar != value)
            {
                _hideTopBar = value;
                OnPropertyChanged(nameof(HideTopBar));
            }
        }
    }

    public bool _collapseEmpty = false;
    public bool CollapseEmpty
    {
        get => _collapseEmpty;
        set
        {
            if (_collapseEmpty != value)
            {
                _collapseEmpty = value;
                OnPropertyChanged(nameof(CollapseEmpty));
            }
        }
    }

    public enum HeaderStyle
    {
        Category,
        Item
    }
}

public class SpacerBlock : ToolTipBlock
{
    public int Height { get; set; } = 10;
    public bool ShowBar { get; set; } = false;
    public float BarOpacity { get; set; } = 0.25f;
}

public class TextsBlock : ToolTipBlock
{
    public string Text { get; set; }
    public bool Italic { get; set; } = false;
}

public class PerkBlock : ToolTipBlock
{
    public ImageSource Icon { get; set; }
    public string Description { get; set; }
}

public class EnergyModBlock : ToolTipBlock
{
    public ImageSource Icon { get; set; }
    public int Cost { get; set; }
}

public class NotificationBlock : ToolTipBlock
{
    public string Text { get; set; }
    public DestinyUIDisplayStyle Style { get; set; }
}

public class ObjectiveBlock : ToolTipBlock
{
    public ImageSource Icon { get; set; }
    public string Description { get; set; }
    public int Value { get; set; }
    public DestinyUnlockValueUIStyle Style { get; set; }

    public bool IsInterval { get; set; } = false; // Interval objectives
    public int IntervalIndex { get; set; } // aka Step
}

public class RewardBlock : ToolTipBlock
{
    public ImageSource Icon { get; set; }
    public ImageSource LargeIcon { get; set; }
    public string ItemName { get; set; }
    public bool IsEmblem { get; set; } // Can be better
    public bool RewardOnComplete { get; set; } = false; // "Rewards" vs "Triumph Completed Rewards"

    public bool IsInterval { get; set; } = false; // Interval rewards
    public int IntervalIndex { get; set; } // aka Step

    public List<RewardBlock> Rewards { get; set; } = new(); // lol this is dumb but it works
}

public class EmblemBlock : ToolTipBlock
{
    public ImageSource Emblem { get; set; }
}

public class InputBlock : ToolTipBlock
{
    public string Key { get; set; }
    public string KeyPress { get; set; }
    public string Action { get; set; }
}

public class BlockColorSelector : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var blockColor = values[1] as Color?;
        var overrideColor = values[0] as Color?;

        return new SolidColorBrush((Color)(overrideColor ?? blockColor ?? Color.FromArgb(0, 0, 0, 0)));
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ObjBlockTemplateSelector : DataTemplateSelector
{
    public DataTemplate CheckboxTemplate { get; set; }
    public DataTemplate PercentageTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        switch (((dynamic)item).Style)
        {
            case DestinyUnlockValueUIStyle.Checkbox:
                return CheckboxTemplate;
            default:
                return PercentageTemplate;
        }
    }
}

public class GenericTooltip
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string Label { get; set; }
    public HeaderBlock.HeaderStyle Style { get; set; } = HeaderBlock.HeaderStyle.Item;
    public TranslateTransform ExtraOffset { get; set; } = new(0, 0);
}

public static class GenericTooltipProperties
{
    public static readonly DependencyProperty TooltipDataProperty =
    DependencyProperty.RegisterAttached(
        "TooltipData",
        typeof(GenericTooltip),
        typeof(GenericTooltipProperties),
        new PropertyMetadata(null));

    public static void SetTooltipData(UIElement element, GenericTooltip value)
    {
        element.SetValue(TooltipDataProperty, value);
    }

    public static GenericTooltip GetTooltipData(UIElement element)
    {
        return (GenericTooltip)element.GetValue(TooltipDataProperty);
    }

    public static bool HasTooltipData(UIElement element)
    {
        return GenericTooltipProperties.GetTooltipData(element) != null;
    }
}


