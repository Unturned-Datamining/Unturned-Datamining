using System;
using System.IO;
using System.Xml;
using Unturned.SystemEx;
using Unturned.UnityEx;

namespace SDG.Unturned;

internal class UnturnedCodeDocsHelper
{
    private XmlDocument documentation;

    public string GetSummary(string className, string fieldName)
    {
        if (documentation == null)
        {
            return null;
        }
        string xpath = "//member[@name='F:SDG.Unturned." + className + "." + fieldName + "']/summary";
        XmlNode xmlNode = documentation.SelectSingleNode(xpath);
        if (xmlNode == null || string.IsNullOrEmpty(xmlNode.InnerText))
        {
            return null;
        }
        return xmlNode.InnerText.Trim('\r', '\n');
    }

    public UnturnedCodeDocsHelper()
    {
        string text = PathEx.Join(UnityPaths.GameDataDirectory, "Managed", "Assembly-CSharp.xml");
        try
        {
            if (File.Exists(text))
            {
                documentation = new XmlDocument();
                documentation.Load(text);
            }
        }
        catch (Exception e)
        {
            documentation = null;
            UnturnedLog.exception(e, "Caught exception loading code documentation:");
        }
    }
}
