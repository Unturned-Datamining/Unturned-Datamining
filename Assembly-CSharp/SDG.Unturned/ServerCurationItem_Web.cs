using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class ServerCurationItem_Web : ServerCurationItem, IAssetErrorContext
{
    public ServerListCurationWebLink webLink;

    public bool isWaitingForResponse;

    private ServerListCurationFile file;

    private Coroutine coroutine;

    public override string DisplayName => file?.Name ?? webLink.url;

    public override string DisplayOrigin => webLink.url;

    public override Texture2D Icon => null;

    public override string IconUrl => file?.IconUrl;

    public override bool IsDeletable => webLink.recommendationId < 1;

    public override int LatestBlockedServerCount => file?.latestBlockedServerCount ?? 0;

    public string AssetErrorPrefix => "Server List Curator at \"" + webLink.url + "\"";

    public override void Reload()
    {
        if (!isWaitingForResponse && (bool)Provider.allowWebRequests)
        {
            isWaitingForResponse = true;
            coroutine = curation.webRequestHandler.StartCoroutine(curation.webRequestHandler.SendRequest(this));
        }
    }

    public override void Delete()
    {
        if (isWaitingForResponse)
        {
            isWaitingForResponse = false;
            curation.webRequestHandler.StopCoroutine(coroutine);
        }
        IConvenientSavedata convenientSavedata = ConvenientSavedata.get();
        string key = $"ServerCurationWebLink_{webLink.id}_Active";
        convenientSavedata.DeleteBool(key);
        curation.RemoveUrl(this);
    }

    public override List<ServerListCurationRule> GetRules()
    {
        return file?.rules;
    }

    public override void ResetBlockedServerCounts()
    {
        if (file == null)
        {
            return;
        }
        file.latestBlockedServerCount = 0;
        if (file.rules == null)
        {
            return;
        }
        foreach (ServerListCurationRule rule in file.rules)
        {
            rule.latestBlockedServerCount = 0;
        }
    }

    protected override void SaveActive()
    {
        string key = $"ServerCurationWebLink_{webLink.id}_Active";
        ConvenientSavedata.get().write(key, _isActive);
    }

    public void ReportAssetError(string message)
    {
        base.ErrorMessage = message;
    }

    internal void NotifyRequestComplete(ServerListCurationFile file)
    {
        isWaitingForResponse = false;
        coroutine = null;
        bool num = file != null || (this.file != null && file == null);
        this.file = file;
        InvokeDataChanged();
        if (num)
        {
            curation.MarkDirty();
        }
    }

    public ServerCurationItem_Web(ServerListCuration curation, ServerListCurationWebLink link)
        : base(curation)
    {
        webLink = link;
        string key = $"ServerCurationWebLink_{webLink.id}_Active";
        if (!ConvenientSavedata.get().read(key, out _isActive))
        {
            _isActive = true;
        }
        if ((bool)Provider.allowWebRequests)
        {
            isWaitingForResponse = true;
            coroutine = curation.webRequestHandler.StartCoroutine(curation.webRequestHandler.SendRequest(this));
        }
    }
}
