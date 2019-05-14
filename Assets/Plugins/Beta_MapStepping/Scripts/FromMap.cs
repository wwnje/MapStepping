using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace Game.MapStepping
{
    public class FromMap : MonoBehaviour
    {
        public Tilemap tileMap;
        public bool KeepGizoms = true;
        public bool ShowPoint = true;
        public bool Use_ScreenSize = false;
        public PointDataScriptableObject TargetData;

        public int TileSize = 1000;
        public int MapZoneNum = 20;

        public List<string> GetShowTypes()
        {
            var strs = new List<string>();
            for (int i = 0; i < TargetData.MenuDataLst.Count; i++)
            {
                var menu = TargetData.MenuDataLst[i];
                strs.Add(menu.Name);
            }
            return strs;
        }

        public MenuData GetMenuData(int index)
        {
            return TargetData.MenuDataLst[index];
        }

        public bool HasTile(MyVector3Int myVector3Int)
        {
            for (int i = 0; i < TargetData.MenuDataLst.Count; i++)
            {
                foreach (var lst in GetTileTargetLst(i))
                {
                    var tileData = lst.TileDataLst.Find(x => (x.MyV3Int.x == myVector3Int.x) && (x.MyV3Int.y == myVector3Int.y) && (x.MyV3Int.z == myVector3Int.z));

                    if (null != tileData)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public List<TileTarget> GetTileTargetLst(int index)
        {
            return TargetData.MenuDataLst[index].TileTargetLst;
        }

        public List<TileTarget> GetTileTargetLst(string name)
        {
            var menuData = TargetData.MenuDataLst.Find(x => x.Name == name);
            return null != menuData ? menuData.TileTargetLst : null;
        }

        public Vector3 GetPos(Vector3Int v3Int)
        {
            return tileMap.GetCellCenterWorld(v3Int);
        }

        public Vector3 GetPos(MyVector3Int myV3Int)
        {
            var v3Int = myV3Int.ToVector3Int();
            return GetPos(v3Int);
        }

        public Vector3 ToWorldPos(MyVector3Int myV3Int, int longX, int longY)
        {
            var bottomV3Int = myV3Int.ToVector3Int(); 
            var leftV3Int = new Vector3Int(0, longY - 1, 0) + bottomV3Int;
            var rightV3Int = new Vector3Int(longX - 1, 0, 0) + bottomV3Int;
            var topV3Int = new Vector3Int(rightV3Int.x, leftV3Int.y, 0);

            var posX = (tileMap.CellToWorld(leftV3Int).x + tileMap.CellToWorld(rightV3Int).x) / 2.0f;
            var posY = (tileMap.CellToWorld(bottomV3Int).z + (tileMap.CellToWorld(topV3Int + new Vector3Int(1, 1, 0)).z)) / 2.0f;
            return new Vector3(posX, 0, posY);
        }

        public MyVector3Int GetMyVector3Int(Vector3 v3)
        {
            var v3Int = tileMap.WorldToCell(v3);
            return v3Int.ToMyVector3Int();
        }

        void OnDrawGizmos()
        {
            if (!KeepGizoms || TargetData == null)
                return;

            foreach (var data in TargetData.MenuDataLst)
            {
                DrawData(data.TileTargetLst);
            }
        }

        void DrawData(List<TileTarget> lst)
        {
            int count = lst.Count;
            for (int dataIndex = 0; dataIndex < count; dataIndex++)
            {
                for (int i = 0; i < lst[dataIndex].TileDataLst.Count; i++)
                {
                    var data = lst[dataIndex];

                    var myV3Int = data.TileDataLst[i].MyV3Int;
                    var pos = ToWorldPos(myV3Int, data.LongX, data.LongY);

                    Gizmos.color = data.Color;

                    if (ShowPoint)
                    {
                        Gizmos.DrawSphere(pos, data.PointSize);
                    }

                    Gizmos.DrawIcon(pos, "BuildingIcon/" + data.IconName, true);

                    // 画区域线
                    var v3Int = myV3Int.ToVector3Int();
                    var rangeLst = GetRangeLst(v3Int, data.LongX, data.LongY);
                    Gizmos.DrawLine(rangeLst[0], rangeLst[1]);
                    Gizmos.DrawLine(rangeLst[1], rangeLst[2]);
                    Gizmos.DrawLine(rangeLst[2], rangeLst[3]);
                    Gizmos.DrawLine(rangeLst[3], rangeLst[0]);
                }
            }
        }

        public List<Vector3> GetRangeLst(Vector3Int pointV3Int, int longX = 1, int longY = 1)
        {
            var bottom = tileMap.CellToWorld(pointV3Int);
            var top = tileMap.CellToWorld(pointV3Int + new Vector3Int(longX, longY, 0));
            var left = tileMap.CellToWorld(pointV3Int + new Vector3Int(0, longY, 0));
            var right = tileMap.CellToWorld(pointV3Int + new Vector3Int(longX, 0, 0));

            return new List<Vector3>
            {
                bottom,
                left,
                top,
                right
            };
        }
    }
}
