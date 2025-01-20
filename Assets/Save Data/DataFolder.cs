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
    public SODataFolder(string extension, string folderName, Func<TData, string> getFileName = null)
        : base(extension, folderName, SaveDataConstructor, SaveDataDestructor, getFileName)
    {
    }

    private static TData SaveDataConstructor() => ScriptableObject.CreateInstance<TData>();
    private static void SaveDataDestructor(TData data) => UnityEngine.Object.DestroyImmediate(data, allowDestroyingAssets: true);
}

/// <summary> Utility for easily storing objects in a Json format within a specified subfolder of Application.persistentData. </summary>
public class DataFolder<TData>
{
    public readonly string extension;
    public readonly string folderName;
    public readonly Func<TData, string> getFileName;
    public readonly Func<TData> dataConstructor;
    public readonly Action<TData> dataDestructor;
    public readonly DirectoryInfo directory;

    /// <summary> Constructs a new DataFolder. </summary>
    /// <param name="extension"> The file extension to append to created files. Must include the "." <para>Example: .save</para></param>
    /// <param name="folderName"> The name of the folder to read and write files from. Will be a sub folder under Application.persistentData</param>
    /// <param name="dataConstructor"> Method for constructing new objects to fill with Json data. </param>
    /// <param name="dataDestructor"> Method for destroying objects when their file counterpart is being deleted. <para> Can be left as null if you don't need one. </para></param>
    /// <param name="getFileName"> Method for deriving file names based on each data object. </param>
    public DataFolder(
        string extension,
        string folderName,
        Func<TData> dataConstructor,
        Action<TData> dataDestructor = null,
        Func<TData, string> getFileName = null)
    {
        this.extension = extension;
        this.folderName = folderName;

        this.dataConstructor = dataConstructor ?? throw new Exception("Tried to construct a DataFolder with a null dataConstructor parameter");
        this.dataDestructor = dataDestructor;
        this.getFileName = getFileName;

        directory = new(folderName);
        if (!directory.Exists) directory.Create();
    }

    /// <summary> Retrieves all data in the folder as objects. </summary>
    public TData[] GetAllSavedData()
        => directory.GetFiles()                                      // get files
        .Where(file => file.Extension == extension)                  // only files matching extension
        .Select(file => JsonToData(File.ReadAllText(file.FullName))) // convert to TData
        .Where(data => data != null)                                 // get rid of nulls
        .ToArray();

    /// <summary> Retrieves all data in the folder as FileInfos. </summary>
    public FileInfo[] GetAllFiles()
        => directory.GetFiles()
            .Where(file => file.Extension == extension)
            .ToArray();

    /// <summary> Reads from the file and creates a data object. </summary>
    /// <param name="file"> The file to read from. </param>
    public TData GetDataFromFile(FileInfo file) => JsonToData(File.ReadAllText(file.FullName));

    /// <summary> Adds or updates the file corresponding to the given data </summary>
    /// <param name="data"> The data to be saved. </param>
    public void Save(TData data) => Save(data, getFileName?.Invoke(data) ?? throw new Exception("No file name action defined!"));
    /// <summary> Adds or updates the file corresponding to the given data </summary>
    /// <param name="data"> The data to be saved. </param>
    /// <param name="name"> The name of file. </param>
    public void Save(TData data, string name)
    {
        var file = new FileInfo(DataPath(name));
        file.Directory.Create();
        File.WriteAllText(file.FullName, DataToJson(data));
    }

    /// <summary> Removes the specified data from the folder. </summary>
    /// <param name="data"> The data to be deleted. </param>
    public void Delete(TData data) => Delete(data, getFileName?.Invoke(data) ?? throw new Exception("No file name action defined!"));
    /// <summary> Removes the specified data from the folder and calls the defined destructor on the data object. </summary>
    /// <param name="data"> The data object to be deleted. </param>
    /// <param name="name"> The name of the file. </param>
    public void Delete(TData data, string name)
    {
        File.Delete(DataPath(name));
        dataDestructor?.Invoke(data);
    }
    /// <summary> Removes the specified data from the folder. </summary>
    /// <param name="name"> The name of the file. </param>
    public void Delete(string name)
    {
        File.Delete(DataPath(name));
    }

    /// <summary> Deletes all data from the folder. </summary>
    public void Clear()
        => directory.GetFiles()                     // get files
        .Where(file => file.Extension == extension) // only files matching extension
        .ToList()                                   // to list (duh)
        .ForEach(file => File.Delete(file.Name));   // delete each file

    #region Internals
    
    private string DataPath(string name) => $"{folderName}/{name}{extension}";

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
