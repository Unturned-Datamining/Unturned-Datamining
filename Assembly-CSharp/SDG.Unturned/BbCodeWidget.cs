namespace SDG.Unturned;

public struct BbCodeWidget
{
    public EBbCodeWidgetType widgetType;

    public string widgetData;

    public BbCodeWidget(EBbCodeWidgetType widgetType, string widgetData)
    {
        this.widgetType = widgetType;
        this.widgetData = widgetData;
    }
}
