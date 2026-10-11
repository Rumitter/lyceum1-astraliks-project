using System.Collections.Generic;

[System.Serializable]
public class ObjectData
{
    public string uniqueId;
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
    public bool isActive;
}

[System.Serializable]
public class SaveData
{
    public List<ObjectData> objects = new List<ObjectData>();
    public string sceneName;
}