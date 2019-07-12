// Version 2.1
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace PrimitivesPro.Editor.MeshEditor
{
    public class Grid
    {
        private int size;
        private float dim;
        private bool show;
        private bool snap;

        public int Size
        {
            get { return size; }
            set
            {
                if (size != value)
                {
                    size = value;
                    SerializeSettings();
                }
            }
        }

        public float Dim
        {
            get { return dim; }
            set
            {
                if (Mathf.Abs(value - dim) > Mathf.Epsilon)
                {
                    dim = value;
                    SerializeSettings();
                }
            }
        }

        public bool Show
        {
            get { return show; }
            set
            {
                if (show != value)
                {
                    show = value;
                    SerializeSettings();
                }
            }
        }

        public bool Snap
        {
            get { return snap; }
            set
            {
                if (snap != value)
                {
                    snap = value;
                    SerializeSettings();
                }
            }
        }

        public Grid(int size, float dim)
        {
            if (!DeserializeSettings())
            {
                this.Dim = dim;
                this.Size = size;
                this.Snap = true;
            }
        }

        public void ShowToggle()
        {
            Show = !Show;
        }

        public void SerializeSettings()
        {
            var dic = new Dictionary<string, object>
            {
                {"GridSize", Size},
                {"GridDim", Dim},
                {"GridShow", Show},
                {"GridSnap", Snap}
            };

            var jsonString = ThirdParty.Json.Serialize(dic);
            Utils.WriteTextFile(Application.dataPath + "/PrimitivesPro/Config/config.json", jsonString);
        }

        public bool DeserializeSettings()
        {
            var jsonString = Utils.ReadTextFile(Application.dataPath + "/PrimitivesPro/Config/config.json");

            if (jsonString != null)
            {
                var dic = ThirdParty.Json.Deserialize(jsonString) as Dictionary<string, object>;

                if (dic != null)
                {
                    Size = System.Convert.ToInt32(dic["GridSize"]);
                    Dim = System.Convert.ToSingle(dic["GridDim"]);
                    Show = System.Convert.ToBoolean(dic["GridShow"]);
                    Snap = System.Convert.ToBoolean(dic["GridSnap"]);

                    return true;
                }
            }

            return false;
        }

        public bool IsVisible()
        {
            return Show;
        }

        public Vector3 FindClosestGridPointXZ(Vector3 point)
        {
            var gridSize = Dim/Size;

            var xCoord = (int)System.Math.Round(point.x/gridSize, 0);
            var yCoord = (int)System.Math.Round(point.z/gridSize, 0);

            return new Vector3(xCoord*gridSize, point.y, yCoord*gridSize);
        }

        public void Draw()
        {
            if (!Show)
            {
                return;
            }

            var perSide = Size;

            var min = -Dim / 2;
            var max = Dim / 2;

            for (int i = 0; i <= perSide; i++)
            {
                var t = ((float) i/perSide);

                var p0 = new Vector3(t*min + (1 - t)*max, 0.0f, min);
                var p1 = new Vector3(t * min + (1 - t) * max, 0.0f, max);

                Handles.color = Color.gray;
                Handles.DrawLine(p0, p1);

                MeshUtils.Swap(ref p0.x, ref p0.z);
                MeshUtils.Swap(ref p1.x, ref p1.z);
                Handles.DrawLine(p0, p1);
            }
        }
    }
}
