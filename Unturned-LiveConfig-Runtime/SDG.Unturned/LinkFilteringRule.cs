using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SDG.Unturned;

public class LinkFilteringRule : IDatParseable
{
    public ELinkFilteringAction action;

    public string[] hosts;

    public string[] pathRegexInputs;

    public List<Regex> pathRegexes;

    public bool TryParse(IDatNode node)
    {
        if (node is DatDictionary datDictionary)
        {
            action = datDictionary.ParseEnum("Action", ELinkFilteringAction.Allow);
            if (datDictionary.TryGetList("Hosts", out var node2))
            {
                List<string> list = new List<string>(node2.Count);
                foreach (IDatNode item2 in node2)
                {
                    if (item2 is DatValue datValue && !string.IsNullOrEmpty(datValue.value))
                    {
                        list.Add(datValue.value);
                    }
                }
                hosts = list.ToArray();
            }
            else
            {
                hosts = new string[1] { datDictionary.GetString("Host") };
            }
            if (datDictionary.TryGetList("Paths", out var node3))
            {
                List<string> list2 = new List<string>(node3.Count);
                foreach (IDatNode item3 in node3)
                {
                    if (item3 is DatValue datValue2 && !string.IsNullOrEmpty(datValue2.value))
                    {
                        list2.Add(datValue2.value);
                    }
                }
                pathRegexInputs = list2.ToArray();
            }
            else
            {
                pathRegexInputs = new string[1] { datDictionary.GetString("Path") };
            }
            if (pathRegexInputs != null && pathRegexInputs.Length != 0)
            {
                pathRegexes = new List<Regex>(pathRegexInputs.Length);
                string[] array = pathRegexInputs;
                foreach (string text in array)
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        Regex item = new Regex(text);
                        pathRegexes.Add(item);
                    }
                }
            }
            return true;
        }
        return false;
    }
}
