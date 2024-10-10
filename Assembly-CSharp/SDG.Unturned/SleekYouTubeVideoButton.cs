namespace SDG.Unturned;

public class SleekYouTubeVideoButton : SleekWrapper
{
    private SleekWebLinkButton linkButton;

    private SleekWebImage webImage;

    public void Refresh(string videoId)
    {
        string url = "https://www.youtube.com/watch?v=" + videoId;
        linkButton.Url = url;
        string url2 = "https://img.youtube.com/vi/" + videoId + "/maxresdefault.jpg";
        webImage.Refresh(url2, shouldCache: false);
    }

    public SleekYouTubeVideoButton()
    {
        linkButton = new SleekWebLinkButton();
        linkButton.SizeOffset_X = 1300f;
        linkButton.SizeOffset_Y = 740f;
        AddChild(linkButton);
        webImage = new SleekWebImage();
        webImage.PositionOffset_X = 10f;
        webImage.PositionOffset_Y = 10f;
        webImage.SizeOffset_X = 1280f;
        webImage.SizeOffset_Y = 720f;
        AddChild(webImage);
    }
}
