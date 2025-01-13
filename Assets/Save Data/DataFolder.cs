/* Made by Oliver Beebe 2023 */
using UnityEngine;
using System.Linq;
using System.IO;
using System;

/// <summary> DataFolder with constructor and destructor for scriptableObjects. </summary>
public class SODataFolder<TData> : DataFolder<TData> where TData : ScriptableObject
{
    /// <summary> Constructs a new ScriptableObject DataFolder. </summary>
    /// <param name="extension">The file extension to append to created files. Must include the "." <para>Example: .save</para></param>
    /// <param name="folderName"> The name of the folder to read and write files from. Will be a sub folder under Application.persistentData</param>
    /// <param name="getFileName"> Method for deriving file names based on each data object. </param>
    public SODataFolder(string extension, string folderName, Func<TData, string> getFileName)
        : base(extension, folderName, getFileName, SaveDataConstructor, SaveDataDestructor)
    {
    }

    private static TData SaveDataConstructor() => ScriptableObject.CreateInstance<TData>();
    private static void SaveDataDestructor(TData data) => UnityEngine.Object.DestroyImmediate(data, allowDestroyingAssets: true);
}

/// <summary> Utility for easily storing objects in a Json format within a specified subfolder of Application.persistentData. </summary>
public class DataFolder<TData>
{
    private readonly string extension;
    private readonly string folderName;
    private readonly Func<TData, string> getFileName;
    private readonly Func<TData> dataConstructor;
    private readonly Action<TData> dataDestructor;
    private readonly DirectoryInfo directory;

    /// <summary> Constructs a new DataFolder. </summary>
    /// <param name="extension"> The file extension to append to created files. Must include the "." <para>Example: .save</para></param>
    /// <param name="folderName"> The name of the folder to read and write files from. Will be a sub folder under Application.persistentData</param>
    /// <param name="getFileName"> Method for deriving file names based on each data object. </param>
    /// <param name="dataConstructor"> Method for constructing new objects to fill with Json data. </param>
    /// <param name="dataDestructor"> Method for destroying objects when their file counterpart is being deleted. <para> Can be left as null if you don't need one. </para></param>
    public DataFolder(

        string extension,
        string folderName,
        Func<TData, string> getFileName,
        Func<TData> dataConstructor,
        Action<TData> dataDestructor = null)
    {
        this.extension = extension;
        this.folderName = folderName;

        this.getFileName = getFileName ?? throw new Exception("Tried to construct a DataFolder with a null getFileName paramater");
        this.dataConstructor = dataConstructor ?? throw new Exception("Tried to construct a DataFolder with a null dataConstructor parameter");
        this.dataDestructor = dataDestructor;

        directory = new(FolderPath);
        if (!directory.Exists) directory.Create();
    }

    /// <summary> Retrieves all data in the folder as objects. </summary>
    public TData[] GetAllSavedData()
        => directory.GetFiles()                                      // get files
        .Where(file => file.Extension == extension)                  // only files matching extension
        .Select(file => JsonToData(File.ReadAllText(file.FullName))) // convert to TData
        .Where(data => data != null)                                 // get rid of nulls
        .ToArray();

    /// <summary> Adds or updates the file corresponding to the given data </summary>
    /// <param name="data"> The data to be saved. </param>
    public void Save(TData data)
    {
        var file = new FileInfo(DataPath(data));
        file.Directory.Create();
        File.WriteAllText(file.FullName, DataToJson(data));
    }

    /// <summary> Removes the specified data from the folder. </summary>
    /// <param name="data"> The data to be deleted. </param>
    public void Delete(TData data)
    {
        File.Delete(DataPath(data));
        dataDestructor?.Invoke(data);
    }

    /// <summary> Deletes all data from the folder. </summary>
    public void Clear()
        => directory.GetFiles()                     // get files
        .Where(file => file.Extension == extension) // only files matching extension
        .ToList()                                   // to list (duh)
        .ForEach(file => File.Delete(file.Name));   // delete each file

    #region Internals

    private string FolderPath => $"{Application.persistentDataPath}/{folderName}";
    private string DataPath(TData data) => $"{FolderPath}/{getFileName.Invoke(data)}{extension}";

    private TData JsonToData(string jsonText)
    {
        var data = dataConstructor.Invoke();
        JsonUtility.FromJsonOverwrite(jsonText, data);
        return data;
    }

    private string DataToJson(TData data)
        => JsonUtility.ToJson(data);

    #endregion
}