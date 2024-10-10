namespace SDG.Unturned;

public enum EBbCodeTokenType
{
    /// <summary>
    /// Text between tags.
    /// </summary>
    String,
    /// <summary>
    /// [b]
    /// </summary>
    BoldOpen,
    /// <summary>
    /// [/b]
    /// </summary>
    BoldClose,
    /// <summary>
    /// [i]
    /// </summary>
    ItalicOpen,
    /// <summary>
    /// [/i]
    /// </summary>
    ItalicClose,
    /// <summary>
    /// [list]
    /// </summary>
    BulletListOpen,
    /// <summary>
    /// [/list]
    /// </summary>
    BulletListClose,
    /// <summary>
    /// [olist]
    /// </summary>
    OrderedListOpen,
    /// <summary>
    /// [/olist]
    /// </summary>
    OrderedListClose,
    /// <summary>
    /// [*] value
    /// </summary>
    ListItem,
    /// <summary>
    /// [h1]
    /// </summary>
    H1Open,
    /// <summary>
    /// [/h1]
    /// </summary>
    H1Close,
    /// <summary>
    /// [h2]
    /// </summary>
    H2Open,
    /// <summary>
    /// [/h2]
    /// </summary>
    H2Close,
    /// <summary>
    /// [h3]
    /// </summary>
    H3Open,
    /// <summary>
    /// [/h3]
    /// </summary>
    H3Close,
    /// <summary>
    /// [url=value]
    /// </summary>
    UrlOpen,
    /// <summary>
    /// [/url]
    /// </summary>
    UrlClose,
    /// <summary>
    /// [img]
    /// </summary>
    ImgOpen,
    /// <summary>
    /// [/img]
    /// </summary>
    ImgClose,
    /// <summary>
    /// [previewyoutube=value]
    /// </summary>
    PreviewYouTubeOpen,
    /// <summary>
    /// [/previewyoutube]
    /// </summary>
    PreviewYouTubeClose,
    /// <summary>
    /// '\n' or "\r\n"
    /// </summary>
    LineBreak
}
