using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.MapStepping.Internal
{
    [CustomEditor(typeof(FromMap))]
    public class FromMapEditor : Editor
    {
        readonly string BtnOpenWindow = "打开 编辑窗口";
        readonly float AxisLength = 10f;

        private static FromMapWindow fromMapWindow;
        private Vector2 labelSize;
        private GUIStyle guiStyle;
        FromMap _fromMap;
        Tilemap tileMap;

        int tileSize;
        Vector3 leftPos;
        Vector3 bottomPos;
        Vector3 rightPos;
        Vector3 topPos;

        Vector3Int minV3Int = Vector3Int.zero;
        Vector3Int maxV3Int;

        Vector3Int leftV3IntForPos;
        Vector3Int bottomV3IntForPos;
        Vector3Int rightV3IntForPos;
        Vector3Int topV3IntForPos;

        [MenuItem("GameObject/MapStepping", false, 10)]
        private static void Create()
        {
            GameObject obj = new GameObject("MapStepping");
            obj.AddComponent<FromMap>();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(Selection.activeObject);
            Undo.RegisterCreatedObjectUndo(obj, "Create GameObject");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public static void OpenQuickSwitchScene()
        {
            if (null == fromMapWindow)
            {
                fromMapWindow = ScriptableObject.CreateInstance<FromMapWindow>();
                fromMapWindow.Show();
                return;
            }

            //Key again Close
            if (EditorWindow.focusedWindow == fromMapWindow)
                fromMapWindow.Close();
            else
                fromMapWindow.Focus();
        }

        private void OnEnable()
        {
            _fromMap = (FromMap)target;
            tileMap = _fromMap.tileMap;

            tileSize = _fromMap.TileSize;

            maxV3Int = new Vector3Int(tileSize - 1, tileSize - 1, 0);

            // 菱形地图轮廓
            leftV3IntForPos = new Vector3Int(0, tileSize - 1, 0) + new Vector3Int(0, 1, 0); ;
            bottomV3IntForPos = Vector3Int.zero;
            rightV3IntForPos = new Vector3Int(tileSize - 1, 0, 0) + new Vector3Int(1, 0, 0);
            topV3IntForPos = new Vector3Int(tileSize - 1, tileSize - 1, 0) + new Vector3Int(1, 1, 0);

            leftPos = tileMap.CellToWorld(leftV3IntForPos);
            bottomPos = tileMap.CellToWorld(bottomV3IntForPos);
            rightPos = tileMap.CellToWorld(rightV3IntForPos);
            topPos = tileMap.CellToWorld(topV3IntForPos);

            labelSize = new Vector2(EditorGUIUtility.singleLineHeight * 2, EditorGUIUtility.singleLineHeight * 2);

            guiStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter
            };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (null != _fromMap.TargetData)
            {
                if (GUILayout.Button(BtnOpenWindow))
                {
                    OpenQuickSwitchScene();
                }
            }
        }

        void OnSceneGUI()
        {
            DrawMap();
            DrawTarget();

            // OnWindow
            if (null == fromMapWindow || _fromMap.TargetData.MenuDataLst.Count == 0)
            {
                return;
            }

            var curTab = fromMapWindow.CurChooseTab;
            var curMenuData = _fromMap.GetMenuData(curTab).TileTargetLst;
            for (int dataIndex = 0; dataIndex < curMenuData.Count; dataIndex++)
            {
                for (int i = 0; i < curMenuData[dataIndex].TileDataLst.Count; i++)
                {
                    DrawSelectionHandle(curMenuData[dataIndex], i);
                }
            }

            if (fromMapWindow.CurBackupDataLst.Count > 0)
            {
                var useIndex = fromMapWindow.DetailReorderableList.index;
                var curIndex = fromMapWindow.CurChooseIndex;
                if (useIndex >= 0 && useIndex < fromMapWindow.CurBackupDataLst[curIndex].TileDataLst.Count)
                {
                    DrawControl(fromMapWindow.CurBackupDataLst[curIndex], useIndex, UnityEditor.Tools.current);
                }
            }
        }

        void DrawTarget()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit rayHit = new RaycastHit();

            if (Physics.Raycast(ray, out rayHit))
            {
                // 地图范围内
                var pointV3Int = tileMap.WorldToCell(rayHit.point);
                if (IsInMap(pointV3Int))
                {
                    if (Event.current.type == EventType.KeyDown)
                    {
                        OnKeyDown(pointV3Int);
                    }

                    Handles.color = new Color(.953f, .659f, .157f, .4f);
                    Handles.DrawDottedLine(_fromMap.transform.position, rayHit.point, 1f);
                    Handles.color = new Color(.953f, .659f, .157f, 1f);

                    //Rings on pointer
                    Handles.DrawWireArc(rayHit.point, Vector3.up, new Vector3(1f, 0f, 1f), 360f, 1f);
                    Handles.DrawWireArc(rayHit.point, Vector3.up, new Vector3(1f, 0f, 1f), 360f, 1.05f);
                    Handles.color = new Color(.953f, .659f, .157f, 1f);
                    Handles.DrawWireArc(rayHit.point, Vector3.up, new Vector3(1f, 0f, 1f), 360f, 1.1f);

                    Handles.color = Color.white;

                    // 区域
                    var rangeLst = _fromMap.GetRangeLst(pointV3Int);
                    Handles.DrawLine(rangeLst[0], rangeLst[1]);
                    Handles.DrawLine(rangeLst[1], rangeLst[2]);
                    Handles.DrawLine(rangeLst[2], rangeLst[3]);
                    Handles.DrawLine(rangeLst[3], rangeLst[0]);
                }
            }
        }

        void OnKeyDown(Vector3Int v3Int)
        {
            int curIndex = fromMapWindow.CurChooseIndex;
            if (null == fromMapWindow.CurBackupDataLst || fromMapWindow.CurBackupDataLst.Count == 0) return;

            fromMapWindow.AddMyV3Int(v3Int);
        }

        /// <summary>
        /// 绘制选择对象的控制
        /// </summary>
        /// <param name="i">点对象索引值</param>
        private void DrawSelectionHandle(TileTarget data, int i)
        {
            if (Event.current.button != 0)
                return;

            MyVector3Int myV3Int = data.TileDataLst[i].MyV3Int;

            Handles.color = data.Color;
            var v3Int = myV3Int.ToVector3Int();
            var pos = _fromMap.GetPos(v3Int);

            var pointSize = GetPointSize(myV3Int, data);
            if (Handles.Button(new Vector3(pos.x, -1f, pos.z), Quaternion.identity, pointSize, pointSize, Handles.SphereHandleCap) && fromMapWindow.DetailReorderableList.index != i)
            {
                fromMapWindow.DetailReorderableList.index = i;
                InternalEditorUtility.RepaintAllViews();
            }

            DrawPointAxisLine(myV3Int, Vector3.right, Color.red, data);
            DrawPointAxisLine(myV3Int, Vector3.up, Color.green, data);
            DrawPointAxisLine(myV3Int, Vector3.forward, Color.blue, data);

            // TODO 报错
            //Handles.BeginGUI();
            //GUILayout.BeginArea(LabelRect(pos));
            //guiStyle.normal.textColor = data.Color;
            //guiStyle.fontSize = data.IndexFontSize;
            //GUILayout.Label(new GUIContent(i.ToString(), string.Format("Point {0}\nPosition: {1}", i, myV3Int)), guiStyle);
            //GUILayout.EndArea();
            //Handles.EndGUI();
        }

        /// <summary>
        /// 根据是否使用屏幕大小，获取点大小
        /// </summary>
        /// <param name="myV3Int">点的位置</param>
        /// <returns>点的实际大小</returns>
        private float GetPointSize(MyVector3Int myV3Int, TileTarget data)
        {
            float pointSize;
            var v3Int = myV3Int.ToVector3Int();
            var pos = _fromMap.GetPos(v3Int);

            if (_fromMap.Use_ScreenSize)
                pointSize = HandleUtility.GetHandleSize(pos) * data.PointSize;
            else
                pointSize = data.PointSize;
            return pointSize;
        }

        /// <summary>
        /// 绘制点对象自身的轴
        /// </summary>
        /// <param name="i">索引</param>
        /// <param name="dir">方向</param>
        /// <param name="color">颜色</param>
        private void DrawPointAxisLine(MyVector3Int myV3Int, Vector3 dir, Color color, TileTarget data)
        {
            var v3Int = myV3Int.ToVector3Int();
            var pos = _fromMap.GetPos(v3Int);
            Handles.color = color;

            if (_fromMap.Use_ScreenSize)
            {
                Handles.DrawLine(pos, pos + dir * AxisLength * GetPointSize(myV3Int, data));
            }
            else
            {
                Handles.DrawLine(pos, pos + dir * AxisLength);
            }
        }

        /// <summary>
        /// 获取索引标签矩形
        /// </summary>
        /// <param name="pos">点对象所在位置</param>
        /// <returns>引标签矩形</returns>
        private Rect LabelRect(Vector3 pos)
        {
            Vector2 labelPos = HandleUtility.WorldToGUIPoint(pos);
            labelPos.y -= labelSize.y / 2;
            labelPos.x -= labelSize.x / 2;
            return new Rect(labelPos, labelSize);
        }

        /// <summary>
        /// 绘制移动和旋转的控制
        /// </summary>
        /// <param name="i">点对象的索引</param>
        /// <param name="type">工具类型</param>
        private void DrawControl(TileTarget data, int i, Tool type)
        {
            if (type != Tool.Move && type != Tool.Rotate)
                return;

            Handles.color = data.Color;

            EditorGUI.BeginChangeCheck();

            var v3Int = data.TileDataLst[i].MyV3Int.ToVector3Int();
            Handles.SphereHandleCap(0, _fromMap.GetPos(v3Int), Quaternion.identity, GetPointSize(data.TileDataLst[i].MyV3Int, data), EventType.Repaint);

            Vector3 newPos = new Vector3();
            if (type == Tool.Move)
            {
                newPos = Handles.PositionHandle(_fromMap.GetPos(v3Int), Quaternion.identity);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (type == Tool.Move)
                {
                    MyVector3Int newV3Int = _fromMap.GetMyVector3Int(newPos);
                    if (_fromMap.HasTile(newV3Int))
                    {
                        return;
                    }

                    Undo.RecordObject(target, "Move Point");
                    data.TileDataLst[i].MyV3Int = newV3Int;
                }

                // 保存
                SaveData();
            }
        }

        void SaveData()
        {
            EditorUtility.SetDirty(_fromMap.TargetData);
        }

        void DrawMap()
        {
            Handles.color = Color.green;
            Handles.DrawLine(leftPos, bottomPos);
            Handles.DrawLine(bottomPos, rightPos);
            Handles.DrawLine(rightPos, topPos);
            Handles.DrawLine(topPos, leftPos);

            // 区域
            Handles.color = Color.grey;
            var count = _fromMap.MapZoneNum;
            var zoneBorder = tileSize / count;

            // 右到左
            for (int i = 0; i < count - 1; i++)
            {
                // 0 + 25 (菱形的下坐标 所以用25来算)
                var v3Int = new Vector3Int(minV3Int.x + (i + 1) * zoneBorder, minV3Int.y, 0);
                Handles.DrawLine(tileMap.CellToWorld(v3Int), tileMap.CellToWorld(new Vector3Int(v3Int.x, leftV3IntForPos.y, 0)));
            }

            // 左到右
            var leftV3Int = new Vector3Int(0, tileSize - 1, 0);
            for (int i = 0; i < count - 1; i++)
            {
                // 菱形的下，所以要 + 1来用算坐标
                // 999 + 1 - 25
                var v3Int = new Vector3Int(leftV3Int.x, leftV3Int.y + 1 - (i + 1) * zoneBorder, 0);
                Handles.DrawLine(tileMap.CellToWorld(v3Int), tileMap.CellToWorld(new Vector3Int(topV3IntForPos.x, v3Int.y, 0)));
            }
        }

        bool IsInMap(Vector3Int v3Int)
        {
            return v3Int.x >= minV3Int.x && v3Int.y >= minV3Int.y
                && maxV3Int.x >= v3Int.x && maxV3Int.y >= v3Int.y;
        }
    }
}
