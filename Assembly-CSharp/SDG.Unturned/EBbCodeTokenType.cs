namespace SDG.Unturned;

public enum EBbCodeTokenType
{
    /// <summary>
    /// Null token.
    /// </summary>
    Invalid,
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
    /// Nelson 2025-07-02: manually written lists typically don't have a ListItemClose token.
    /// </summary>
    ListItemOpen,
    /// <summary>
    /// [/*]
    /// Nelson 2025-07-02: Steam's new visual editor adds closing tokens to list items, but
    /// manually-written list items typically don't have them.
    /// </summary>
    ListItemClose,
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
    LineBreak,
    /// <summary>
    /// [quote=value] (value is author)
    /// </summary>
    QuoteOpen,
    /// <summary>
    /// [/quote]
    /// </summary>
    QuoteClose,
    /// <summary>
    /// [p]
    /// </summary>
    ParagraphOpen,
    /// <summary>
    /// [/p]
    /// </summary>
    ParagraphClose,
    /// <summary>
    /// [u]
    /// </summary>
    UnderlineOpen,
    /// <summary>
    /// [/u]
    /// </summary>
    UnderlineClose
}
