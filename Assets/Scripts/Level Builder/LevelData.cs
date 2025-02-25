using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
    public string levelName;
    public int[] gridSize; // [X, Y, Z]
    public List<LevelObject> objects;
}

[Serializable]
public class LevelObject
{
    public string type;
    public float[] position; // [x, y, z]
    public float[] scale; // [x, y, z]

    public LevelObject(string type, float x, float y, float z, float scaleX, float scaleY, float scaleZ)
    {
        this.type = type;
        this.position = new float[] { x, y, z };
        this.scale = new float[] { scaleX, scaleY, scaleZ };
    }
}