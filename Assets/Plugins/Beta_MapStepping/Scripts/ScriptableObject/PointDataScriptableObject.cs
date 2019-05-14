using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MapStepping
{
    [CreateAssetMenu(fileName = "MapStepping", menuName = "MapStepping/PointData", order = 10)]
    public class PointDataScriptableObject : ScriptableObject
    {
        [SerializeField]
        public List<MenuData> MenuDataLst = new List<MenuData>();
    }

    [Serializable]
    public class MenuData
    {
        public string Name = "";
        public List<TileTarget> TileTargetLst = new List<TileTarget>();
    }

    [Serializable]
    public class TileTarget
    {
        public float PointSize = 1.28f;
        public int LongX = 1;
        public int LongY = 1;
        public string IconName = "1";
        public Color Color = new Color(1f,1f,1f,1f);
        public List<TileData> TileDataLst = new List<TileData>();
    }

    [Serializable]
    public class TileData
    {
        public MyVector3Int MyV3Int;
    }

    [Serializable]
    public class TileTargetJson
    {
        public string MenuName;
        public List<TileDataJson> TileDataLst = new List<TileDataJson>();
    }

    [Serializable]
    public class TileDataJson
    {
        public string IconName = "1";
        public List<TileData> PosLst = new List<TileData>();
    }
}
