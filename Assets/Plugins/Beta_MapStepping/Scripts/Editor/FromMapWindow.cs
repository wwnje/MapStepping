using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Game.MapStepping.Internal
{
    public class FromMapWindow : EditorWindow
    {
        const string BtnDeleteMenu = "删除";
        const string BtnAddMenu = "添加";
        const string BtnGoto = "定位资源";
        const string BTN_SAVE = "保存";
        const string BtnRefresh = "刷新";
        const string BtnImport = "导入Json";
        const string BTN_EXPORT = "输出Json";
        const string BtnClear= "清空当前";

        const string RECORED_TITLE = "类别";
        const string USE_TITLE = "Goto列表";

        // 当前操作的数据
        public List<TileTarget> CurBackupDataLst = new List<TileTarget>();

        int lastChooseTab = -1;
        public int CurChooseTab = 0;

        // 正在使用的列表
        int oriChooseIndex = 0;
        public int CurChooseIndex = 0;

        public ReorderableList menuReorderableList;
        public ReorderableList DetailReorderableList;

        private bool _isReorderableListChange = false;
        private Vector2 _scrollPos;
        private Vector2 _scrollPos_Use;

        const int BtnSettingWidth = 60;

        // 类别菜单
        private Rect menuScrollRect = new Rect(10, 70, 500, 280);
        private Rect _reorderableListRect = new Rect(10, 20, 480, 280);
        Rect _scrollViewRect = new Rect(0, 20, 480, 280);

        // 具体菜单
        private Rect _scrollRectDetail = new Rect(10, 360, 500, 380);
        private Rect _reorderableListRect_Use= new Rect(10, 320, 480, 380);
        Rect _scrollViewRect_Use = new Rect(0, 320, 480, 380);

        // Menu修改
        private Rect textUpdateMenuTip = new Rect(20, 50, 30, 20);
        private Rect textUpdateMenu = new Rect(60, 50, 100, 20);

        private Rect _clearBtnRect = new Rect(375, 50, BtnSettingWidth, BtnSettingHeight);
        private Rect _exportBtnRect = new Rect(440, 50, BtnSettingWidth, BtnSettingHeight);

        // 窗口固定宽度 / 窗口固定高度
        Vector2 _WinSize = new Vector2(520, 800);

        const int BtnSettingHeight = 20;
        const int BtnSettingY = 20;

        // 下拉框
        private Rect selectTabRect = new Rect(20, BtnSettingY, 50, BtnSettingHeight);

        private Rect _saveBtnRect = new Rect(75, BtnSettingY, 100, BtnSettingHeight);
        private Rect btnDeleteRect = new Rect(180, BtnSettingY, BtnSettingWidth, BtnSettingHeight);
        private Rect _refreshBtnRect = new Rect(245, BtnSettingY, BtnSettingWidth, BtnSettingHeight);
        private Rect _gotoDataBtnRect = new Rect(310, BtnSettingY, BtnSettingWidth, BtnSettingHeight);
        private Rect _importBtnRect = new Rect(375, BtnSettingY, BtnSettingWidth, BtnSettingHeight);
        private Rect btnAddRect = new Rect(440, BtnSettingY, BtnSettingWidth, BtnSettingHeight);

        public FromMapWindow()
        {
            this.position = new Rect(Screen.width / 2 - minSize.x / 2, Screen.height / 2 - minSize.y / 2, minSize.x, minSize.y);
            titleContent = new GUIContent("布点选择工具");

            this.minSize = _WinSize;
            this.maxSize = _WinSize;
        }

        FromMap _fromMap;
        public FromMap FromMap
        {
            get
            {
                if (_fromMap == null)
                {
                    _fromMap = GameObject.FindObjectOfType<FromMap>();
                }
                return _fromMap;
            }
        }

        private void OnEnable()
        {
            if (null == FromMap.TargetData)
            {
                Debug.LogError("SettingData Not Find");
                return;
            }
        }

        void InitChooseDataTab()
        {
            // 根据 curType选择
            CurBackupDataLst = FromMap.GetTileTargetLst(CurChooseTab);

            InitTab();
            InitDetail();
        }

        void OnGUI()
        {
            DrawButtonSettingView();
            if (FromMap.TargetData.MenuDataLst.Count == 0)
            {
                return;
            }

            CurChooseTab = EditorGUI.Popup(selectTabRect, (int)CurChooseTab, _fromMap.GetShowTypes().ToArray());

            OnCurrentShowTabChange();
            DrawTabBtn();
            DrawReorderableList();
            DrawDetailReorderableList();
            OnDetailLstRefreh();
        }

        void OnCurrentShowTabChange()
        {
            if (lastChooseTab != CurChooseTab)
            {
                Debug.LogFormat("切换到:{0}", _fromMap.GetShowTypes()[CurChooseTab]);
                ResetIndex();
                InitChooseDataTab();
                lastChooseTab = CurChooseTab;
            }
        }

        void DrawTabBtn()
        {
            GUI.Label(textUpdateMenuTip, "Name-");
            _fromMap.TargetData.MenuDataLst[CurChooseTab].Name = EditorGUI.TextField(textUpdateMenu, _fromMap.TargetData.MenuDataLst[CurChooseTab].Name);

            if (GUI.Button(_clearBtnRect, BtnClear))
            {
                var isClear = EditorUtility.DisplayDialog("Tip", "是否要清空当前列表?", "Yes", "Cancle");
                if (isClear)
                {
                    OnClear();
                }
            }

            if (GUI.Button(_exportBtnRect, BTN_EXPORT))
            {
                ExportJson();
            }
        }

        void OnDetailLstRefreh()
        {
            if (CurBackupDataLst.Count <= CurChooseIndex || oriChooseIndex == CurChooseIndex)
            {
                return;
            }

            GUI.Label(new Rect(10, 320, 200, 30), string.Format("当前选择:{0}", CurChooseIndex));
            oriChooseIndex = CurChooseIndex;
            InitDetail();
        }

        private void DrawButtonSettingView()
        {
            if (GUI.Button(btnAddRect, BtnAddMenu))
            {
                var newTab = new MenuData
                {
                    Name = "New_" + FromMap.TargetData.MenuDataLst.Count
                };

                FromMap.TargetData.MenuDataLst.Add(newTab);
                CurChooseTab = FromMap.TargetData.MenuDataLst.Count - 1;
                lastChooseTab = -1;
            }

            if (GUI.Button(_importBtnRect, BtnImport))
            {
                ImportJson();
            }

            if (GUI.Button(_gotoDataBtnRect, BtnGoto))
            {
                Selection.activeObject = FromMap.TargetData;
            }

            if (FromMap.TargetData.MenuDataLst.Count > 0)
            {
                if (GUI.Button(btnDeleteRect, BtnDeleteMenu))
                {
                    var isClear = EditorUtility.DisplayDialog("Tip", "是否要删除当前分页?", "Yes", "Cancle");
                    if (isClear)
                    {
                        OnRemoveMenu();
                    }
                }

                if (GUI.Button(_refreshBtnRect, BtnRefresh))
                {
                    InitChooseDataTab();
                }

                if (GUI.Button(_saveBtnRect, BTN_SAVE))
                {
                    _isReorderableListChange = false;
                    SaveData();
                }
            }
        }

        private void DrawReorderableList()
        {
            _scrollViewRect.height = menuReorderableList.GetHeight();
            _scrollPos = GUI.BeginScrollView(menuScrollRect, _scrollPos, _scrollViewRect);
            menuReorderableList.DoList(_reorderableListRect);
            GUI.EndScrollView();
        }

        private void DrawDetailReorderableList()
        {
            if (CurBackupDataLst.Count <= 0)
            {
                return;
            }

            _scrollViewRect_Use.height = DetailReorderableList.GetHeight();
            _scrollPos_Use = GUI.BeginScrollView(_scrollRectDetail, _scrollPos_Use, _scrollViewRect_Use);
            DetailReorderableList.DoList(_reorderableListRect_Use);
            GUI.EndScrollView();
        }

        // 类别列表信息
        private void InitTab()
        {
            menuReorderableList = new ReorderableList(CurBackupDataLst, CurBackupDataLst.GetType())
            {
                elementHeight = 20,
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (index >= CurBackupDataLst.Count || CurBackupDataLst[index] == null)
                        return;

                    var labelRect = new Rect(rect.x, rect.y, 60, 20);
                    GUI.Label(labelRect, "数量-" + CurBackupDataLst[index].TileDataLst.Count.ToString());

                    var iconTipRect = new Rect(rect.x + 60, rect.y, 50, 15);
                    GUI.Label(iconTipRect, "图标-");
                    var iconNameRect = new Rect(rect.x + 110, rect.y, 30, 15);
                    CurBackupDataLst[index].IconName = EditorGUI.TextField(iconNameRect, CurBackupDataLst[index].IconName);

                    var sizeLabelRect = new Rect(rect.x + 140, rect.y, 40, 15);
                    GUI.Label(sizeLabelRect, "点大小-");
                    var pointRect = new Rect(rect.x + 180, rect.y, 40, 15);
                    CurBackupDataLst[index].PointSize = EditorGUI.FloatField(pointRect, CurBackupDataLst[index].PointSize);

                    var longTipRect = new Rect(rect.x + 220, rect.y, 40, 15);
                    GUI.Label(longTipRect, "范围-");
                    var longXRect = new Rect(rect.x + 260, rect.y, 20, 15);
                    var longYRect = new Rect(rect.x + 280, rect.y, 20, 15);

                    // 范围
                    CurBackupDataLst[index].LongX = EditorGUI.IntField(longXRect, CurBackupDataLst[index].LongX);
                    CurBackupDataLst[index].LongY = EditorGUI.IntField(longYRect, CurBackupDataLst[index].LongY);

                    var colorRect = new Rect(rect.x + 300, rect.y, 50, 15);
                    var btnRect = new Rect(rect.x + 370, rect.y, 80, 15);
                    CurBackupDataLst[index].Color = EditorGUI.ColorField(colorRect, CurBackupDataLst[index].Color);

                    if (GUI.Button(btnRect, "详细列表O"))
                    {
                        // 设置正在使用的列表
                        CurChooseIndex = index;
                    }
                },

                // 添加类别
                onAddCallback = delegate
                {
                    CurBackupDataLst.Add(
                        new TileTarget
                        {
                            TileDataLst = new List<TileData>(),
                        });
                },

                // 删除
                onRemoveCallback = delegate
                {
                    var isDelete = EditorUtility.DisplayDialog("Tip", "Do you want to delete it?", "Delete", "Cancle");

                    if (isDelete)
                    {
                        CurBackupDataLst.RemoveAt(menuReorderableList.index);
                        CurChooseIndex = Mathf.Max(0, CurBackupDataLst.Count - 1);
                    }
                },

                // 修改
                onChangedCallback = (list) =>
                {
                    _isReorderableListChange = true;
                }
            };

            menuReorderableList.drawHeaderCallback = (Rect rect) => { GUI.Label(rect, RECORED_TITLE); };
            menuReorderableList.onSelectCallback = (list) =>
            {
                int index = menuReorderableList.index;
            };
        }

        private void InitDetail()
        {
            List<TileData> tileDataList = new List<TileData>();

            if (CurBackupDataLst == null || CurBackupDataLst.Count <= 0)
            {
                Debug.LogFormat("数据为空！");
            }
            else
            {
                TileTarget curData = CurBackupDataLst[CurChooseIndex];
                tileDataList = curData.TileDataLst;
            }

            DetailReorderableList = new ReorderableList(tileDataList, tileDataList.GetType())
            {
                elementHeight = 20,

                // 绘制列表
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (index >= tileDataList.Count || tileDataList[index] == null)
                    {
                        Debug.Log("错误");
                        return;
                    }

                    var textFieldRect = new Rect(rect.x, rect.y, 80, 15);
                    EditorGUI.LabelField(textFieldRect, index.ToString());

                    var posRect = new Rect(rect.x + 150, rect.y, 200, 15);
                    EditorGUI.LabelField(posRect, tileDataList[index].MyV3Int.ToString());

                    var btnRect = new Rect(rect.x + 360, rect.y, 80, 15);
                    if (GUI.Button(btnRect, "Goto:" + index.ToString()))
                    {
                        DetailReorderableList.index = index;
                        SceneView.lastActiveSceneView.pivot = _fromMap.GetPos(tileDataList[index].MyV3Int);
                        SceneView.lastActiveSceneView.Repaint();
                    }
                },

                // 添加Pos
                onAddCallback = delegate
                {
                    tileDataList.Add(
                        new TileData
                        {
                            MyV3Int = MyVector3Int.one,
                        });

                    // 刷新
                    InitTab();
                },

                onRemoveCallback = delegate
                {
                    var isDelete = EditorUtility.DisplayDialog("Tip", "Do you want to delete it?", "Delete", "Cancle");
                    if (isDelete)
                    {
                        tileDataList.RemoveAt(DetailReorderableList.index);
                        InitTab();
                    }
                },

                onChangedCallback = (list) =>
                {
                    CurBackupDataLst[CurChooseIndex].TileDataLst = tileDataList;
                }
            };

            DetailReorderableList.drawHeaderCallback = (Rect rect) => {

                if (CurBackupDataLst.Count <= CurChooseIndex)
                {
                    Debug.LogError(CurChooseIndex);
                }

                TileTarget data = null != CurBackupDataLst  && CurBackupDataLst.Count > 0 ? CurBackupDataLst[CurChooseIndex] : null;
                var iconName = FromMap.TargetData.MenuDataLst[CurChooseTab].TileTargetLst[CurChooseIndex].IconName;
                GUI.Label(rect, string.Format("当前:Index:{0}, IconName:{1}", CurChooseIndex, iconName));
            };

            // 选择
            DetailReorderableList.onSelectCallback = (list) =>
            {
                int index = DetailReorderableList.index;
            };
        }

        private void OnExitSettingCallBack()
        {
            if (_isReorderableListChange)
            {
                var isSave = EditorUtility.DisplayDialog("Tip", "The Value is Change,Do you want to save it?", "Save", "Cancle");

                if (isSave)
                {
                    SaveData();
                }
            
                _isReorderableListChange = false;
            }
        }

        public void AddMyV3Int(Vector3Int V3Int)
        {
            var myV3Int = V3Int.ToMyVector3Int();
            if (_fromMap.HasTile(myV3Int))
            {
                return;
            }

            var tileData = new TileData
            {
                MyV3Int = myV3Int,
            };

            CurBackupDataLst[CurChooseIndex].TileDataLst.Add(tileData);
        }

        void ResetIndex()
        {
            CurChooseIndex = 0;
            oriChooseIndex = 0;
        }

        #region Button
        void SaveData()
        {
            EditorUtility.SetDirty(FromMap.TargetData);
        }

        void OnClear()
        {
            CurBackupDataLst.Clear();
            CurBackupDataLst = new List<TileTarget>();
            ResetIndex();
            SaveData();
            InitChooseDataTab();
        }

        void OnRemoveMenu()
        {
            CurBackupDataLst.Clear();
            CurBackupDataLst = new List<TileTarget>();
            ResetIndex();

            FromMap.TargetData.MenuDataLst.RemoveAt(CurChooseTab);
            CurChooseTab = Mathf.Max(0, CurChooseTab - 1);
            SaveData();
        }
        #endregion

        #region 导入导出
        void ImportJson()
        {
            // 打开原有json
            string filePathOpen = EditorUtility.OpenFilePanel("Open data file", Application.streamingAssetsPath, "json");

            if (!string.IsNullOrEmpty(filePathOpen))
            {
                string dataJson = File.ReadAllText(filePathOpen);
                var json = JsonUtility.FromJson<TileTargetJson>(dataJson);
                var list = _fromMap.GetTileTargetLst(json.MenuName);
                if (null == list)
                {
                    _fromMap.TargetData.MenuDataLst.Add(new MenuData
                    {
                        Name = json.MenuName,
                    });

                    list = _fromMap.GetTileTargetLst(json.MenuName);
                }
                list.Clear();

                foreach (var data in json.TileDataLst)
                {
                    var tileTarget = new TileTarget
                    {
                        IconName = data.IconName
                    };

                    tileTarget.TileDataLst = data.PosLst;
                    list.Add(tileTarget);
                }

                SaveData();
            }
        }

        // 输出
        void ExportJson()
        {
            SaveData();
            foreach (var data in FromMap.TargetData.MenuDataLst)
            {
                ExportJson(data);
            }
        }

        void ExportJson(MenuData menuData)
        {
            if (menuData.TileTargetLst.Count == 0)
            {
                Debug.LogFormat("{0} 没有可以导出的数据！", menuData.Name);
                return;
            }

            if (string.IsNullOrEmpty(menuData.Name))
            {
                return;
            }

            var filePath = EditorUtility.SaveFilePanel("Save data file", Application.streamingAssetsPath, menuData.Name, "json");

            var exportData = new TileTargetJson
            {
                MenuName = menuData.Name
            };

            foreach (var data in menuData.TileTargetLst)
            {
                var pointData = new TileDataJson
                {
                    IconName = data.IconName
                };

                foreach (var detail in data.TileDataLst)
                {
                    var tileData = new TileData
                    {
                        MyV3Int = detail.MyV3Int,
                    };

                    pointData.PosLst.Add(tileData);
                }

                exportData.TileDataLst.Add(pointData);
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                string dataAsJson = JsonUtility.ToJson(exportData);
                File.WriteAllText(filePath, dataAsJson);
            }
        }
        #endregion
        private void OnDestroy()
        {
            ResetIndex();
            OnExitSettingCallBack();
        }
    }
}
