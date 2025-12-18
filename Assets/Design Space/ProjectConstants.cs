using UnityEditor.SearchService;

public static class ProjectConstants
{
    public const string ProjectName = "Design Space";

    public const string ProjectFolder = ProjectName + "/";

    public const string CommonFolder = ProjectFolder + "Common/";
    public const string ContentFolder = ProjectFolder + "Content/";
    public const string ServicesFolder = ProjectFolder + "Services/";
    public const string ActionsFolder = ProjectFolder + "Editor Actions/";
    public const string ToolbarActionsFolder = ProjectFolder + "Toolbar Actions/";
    public const string AudioFolder = ProjectFolder + "Audio/";
    public const string SettingsFolder = ProjectFolder + "Settings/";

    public const string ToolMenuItems = ProjectFolder + "Tools/";

    public const string CreateAssetMenuValidators = "Assets/Validators/";
    public const int CreateAssetMenuValidatorsPriority = -10000;
}
