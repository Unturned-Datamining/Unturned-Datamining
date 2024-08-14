using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SDG.Unturned;

public class LinkFilteringLiveConfig
{
    public LinkFilteringRule[] rules = new LinkFilteringRule[0];

    public ELinkFilteringAction defaultAction = ELinkFilteringAction.Deny;

    public ELinkFilteringAction Match(string host, string path)
    {
        if (rules != null)
        {
            LinkFilteringRule[] array = rules;
            foreach (LinkFilteringRule linkFilteringRule in array)
            {
                if (linkFilteringRule.hosts == null)
                {
                    continue;
                }
                bool flag = false;
                string[] hosts = linkFilteringRule.hosts;
                for (int j = 0; j < hosts.Length; j++)
                {
                    if (string.Equals(hosts[j], host, StringComparison.InvariantCultureIgnoreCase))
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    continue;
                }
                if (linkFilteringRule.pathRegexes == null || linkFilteringRule.pathRegexes.Count < 1)
                {
                    return linkFilteringRule.action;
                }
                foreach (Regex pathRegex in linkFilteringRule.pathRegexes)
                {
                    if (pathRegex.IsMatch(path))
                    {
                        return linkFilteringRule.action;
                    }
                }
            }
        }
        return defaultAction;
    }

    public void Parse(DatDictionary data)
    {
        if (data.TryGetList("Rules", out var node))
        {
            List<LinkFilteringRule> list = new List<LinkFilteringRule>(node.Count);
            foreach (IDatNode item in node)
            {
                LinkFilteringRule linkFilteringRule = new LinkFilteringRule();
                if (linkFilteringRule.TryParse(item))
                {
                    list.Add(linkFilteringRule);
                }
            }
            rules = list.ToArray();
        }
        else
        {
            rules = new LinkFilteringRule[0];
        }
        defaultAction = data.ParseEnum("DefaultAction", ELinkFilteringAction.Deny);
    }
}
