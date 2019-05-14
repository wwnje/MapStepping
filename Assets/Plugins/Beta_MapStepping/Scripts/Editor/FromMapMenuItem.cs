using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.MapStepping.Internal
{
    public class FromMapMenuEditor : Editor
    {

        [MenuItem("FromMap/Create")]
        public static void CreateReporter()
        {
            var fromMapObj = new GameObject();
            fromMapObj.name = "FromMap";
            var fromMap = fromMapObj.AddComponent<FromMap>();

            // Register root object for undo.
            Undo.RegisterCreatedObjectUndo(fromMapObj, "Create FromMap Object");
        }
    }
}