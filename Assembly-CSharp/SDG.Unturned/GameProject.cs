using System;
using Unturned.UnityEx;

namespace SDG.Unturned;

public class GameProject
{
    private static string _projectPath;

    /// <summary>
    /// Absolute path to project directory, e.g. C:/U3
    /// </summary>
    [Obsolete("Replaced by UnityPaths.ProjectDirectory")]
    public static string PROJECT_PATH
    {
        get
        {
            if (string.IsNullOrEmpty(_projectPath))
            {
                if (UnityPaths.ProjectDirectory != null)
                {
                    _projectPath = UnityPaths.ProjectDirectory.FullName;
                }
                else
                {
                    _projectPath = UnityPaths.GameDirectory.FullName;
                }
            }
            return _projectPath;
        }
    }
}
