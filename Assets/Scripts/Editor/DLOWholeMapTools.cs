using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DeadLetterOffice.Editor
{
    public static class DLOWholeMapTools
    {
        private const string WholeMapScenePath = "Assets/Scenes/GameBuildScenes/Scene_WholeMap.unity";
        private const string RootName = "DLO_BlockoutMap_FromSketch";

        private static readonly Color GroundColor = new(0.86f, 0.84f, 0.78f, 1f);
        private static readonly Color PathColor = new(0.64f, 0.42f, 0.27f, 1f);
        private static readonly Color WaterColor = new(0.14f, 0.62f, 0.95f, 0.88f);
        private static readonly Color GreenColor = new(0.2f, 0.72f, 0.32f, 1f);
        private static readonly Color RoadColor = new(0.46f, 0.47f, 0.46f, 1f);
        private static readonly Color BuildingColor = new(0.92f, 0.91f, 0.86f, 1f);
        private static readonly Color WallColor = new(0.015f, 0.015f, 0.015f, 1f);
        private static readonly Color MarkerColor = new(0.75f, 1f, 0.08f, 1f);
        private static readonly Color LandmarkColor = new(0.86f, 0.78f, 0.58f, 1f);

        private static readonly Dictionary<string, Material> Materials = new();

        [MenuItem("DLO/Setup/Rebuild Scene_WholeMap Blockout")]
        public static void RebuildWholeMapBlockout()
        {
            EditorSceneManager.OpenScene(WholeMapScenePath);
            ClearPreviousRoot();
            Materials.Clear();

            GameObject root = new(RootName);
            CreateBase(root.transform);
            CreateOuterWalls(root.transform);
            CreateRoadsAndWater(root.transform);
            CreateLeftDistrict(root.transform);
            CreateCenterDistrict(root.transform);
            CreateClockTowerDistrict(root.transform);
            CreateInteractionMarkers(root.transform);
            SetupCameraAndLight();

            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DLOWholeMapTools] Scene_WholeMap blockout rebuilt from user sketch.");
        }

        private static void CreateBase(Transform root)
        {
            CreateBox(root, "Base_Walkable_Canvas", Vector3.zero, new Vector3(130f, 0.12f, 86f), GroundColor);
        }

        private static void CreateOuterWalls(Transform root)
        {
            Transform group = CreateGroup(root, "00_OuterBoundary_BlackWalls");
            CreateBox(group, "TopOuterWall", new Vector3(-8f, 1.6f, 42f), new Vector3(118f, 3.2f, 3.2f), WallColor);
            CreateBox(group, "LeftOuterWall_Main", new Vector3(-63f, 1.6f, 5f), new Vector3(3.2f, 3.2f, 76f), WallColor);
            CreateBox(group, "LeftOuterWall_UpperBend", new Vector3(-53f, 1.6f, 33f), new Vector3(20f, 3.2f, 3.2f), WallColor);
            CreateBox(group, "LowerLeftBoundary", new Vector3(-42f, 1.2f, -38f), new Vector3(42f, 2.4f, 2.4f), RoadColor);
        }

        private static void CreateRoadsAndWater(Transform root)
        {
            Transform water = CreateGroup(root, "01_Water_BlueAreas");
            CreateBox(water, "LeftWater_Canal", new Vector3(-44f, 0.08f, 6f), new Vector3(23f, 0.16f, 58f), WaterColor);
            CreateBox(water, "CenterWater_Cross", new Vector3(-10f, 0.08f, 8f), new Vector3(34f, 0.16f, 64f), WaterColor);
            CreateBox(water, "RightWater_River", new Vector3(23f, 0.08f, 2f), new Vector3(20f, 0.16f, 78f), WaterColor);

            Transform road = CreateGroup(root, "02_Roads_GrayAndBrown");
            CreateBox(road, "CentralRoad_LeftLane", new Vector3(2f, 0.14f, -1f), new Vector3(5f, 0.18f, 76f), RoadColor);
            CreateBox(road, "CentralRoad_RightLane", new Vector3(19f, 0.14f, -1f), new Vector3(5f, 0.18f, 76f), RoadColor);
            CreateBox(road, "BrownPath_LeftPerimeter", new Vector3(-44f, 0.22f, -24f), new Vector3(42f, 0.2f, 4f), PathColor);
            CreateBox(road, "BrownPath_CenterBridgeLoop", new Vector3(-16f, 0.22f, -9f), new Vector3(24f, 0.2f, 4f), PathColor);
            CreateBox(road, "BrownPath_RightPlazaLoop_Bottom", new Vector3(47f, 0.22f, -23f), new Vector3(40f, 0.2f, 4f), PathColor);
            CreateBox(road, "BrownPath_RightPlazaLoop_Right", new Vector3(66f, 0.22f, 8f), new Vector3(4f, 0.2f, 54f), PathColor);
            CreateBox(road, "BrownPath_RightTopStreet", new Vector3(45f, 0.22f, 33f), new Vector3(44f, 0.2f, 4f), PathColor);
        }

        private static void CreateLeftDistrict(Transform root)
        {
            Transform district = CreateGroup(root, "03_LeftDistrict_BuildingsAndGardens");
            CreateBox(district, "LeftUpperGarden", new Vector3(-52f, 0.28f, 27f), new Vector3(12f, 0.28f, 16f), GreenColor);
            CreateBox(district, "LeftMiddleGarden", new Vector3(-57f, 0.28f, 5f), new Vector3(16f, 0.28f, 16f), GreenColor);
            CreateBox(district, "LeftLowerGarden", new Vector3(-37f, 0.28f, -31f), new Vector3(38f, 0.28f, 13f), GreenColor);

            CreateBuilding(district, "Building_L1", new Vector3(-56f, 1.6f, -5f), new Vector3(9f, 3.2f, 10f), "건물 1");
            CreateBuilding(district, "Building_L3", new Vector3(-43f, 1.6f, -4f), new Vector3(12f, 3.2f, 14f), "건물 3");
            CreateBuilding(district, "PostOffice_Blockout", new Vector3(-45f, 1.6f, 10f), new Vector3(8f, 3.2f, 12f), "우체국");
            CreateBuilding(district, "Building_L4", new Vector3(-52f, 1.6f, -25f), new Vector3(13f, 3.2f, 5f), "건물 4");
            CreateBuilding(district, "Building_L5", new Vector3(-36f, 1.6f, -25f), new Vector3(13f, 3.2f, 5f), "건물 5");
        }

        private static void CreateCenterDistrict(Transform root)
        {
            Transform district = CreateGroup(root, "04_CenterDistrict_BuildingClusters");
            CreateBox(district, "CenterInnerGarden", new Vector3(-18f, 0.28f, -20f), new Vector3(13f, 0.28f, 18f), GreenColor);
            CreateBox(district, "CenterTopPathLoop", new Vector3(-18f, 0.22f, 25f), new Vector3(29f, 0.2f, 24f), PathColor);
            CreateBox(district, "CenterBottomPathLoop", new Vector3(-17f, 0.22f, -8f), new Vector3(29f, 0.2f, 22f), PathColor);

            CreateBuilding(district, "Building_C3", new Vector3(-27f, 1.6f, 30f), new Vector3(9f, 3.2f, 9f), "건물 3");
            CreateBuilding(district, "Building_C1", new Vector3(-16f, 1.6f, 30f), new Vector3(9f, 3.2f, 9f), "건물 1");
            CreateBuilding(district, "Building_C2", new Vector3(-27f, 1.6f, 18f), new Vector3(11f, 3.2f, 9f), "건물 2");
            CreateBuilding(district, "Building_C4", new Vector3(-15f, 1.6f, 18f), new Vector3(10f, 3.2f, 9f), "건물 4");
            CreateBuilding(district, "Building_C5", new Vector3(-26f, 1.6f, -4f), new Vector3(11f, 3.2f, 11f), "건물 5");
            CreateBuilding(district, "Building_C6", new Vector3(-12f, 1.6f, 0f), new Vector3(8f, 3.2f, 7f), "건물 6");
            CreateBuilding(district, "Building_C7", new Vector3(-8f, 1.6f, -14f), new Vector3(8f, 3.2f, 9f), "건물 7");
            CreateBuilding(district, "Building_C8", new Vector3(-6f, 1.6f, -26f), new Vector3(8f, 3.2f, 8f), "건물 8");
        }

        private static void CreateClockTowerDistrict(Transform root)
        {
            Transform district = CreateGroup(root, "05_RightClockTowerPlaza");
            CreateBox(district, "ClockPlazaGreen_NorthWest", new Vector3(40f, 0.28f, 20f), new Vector3(20f, 0.28f, 17f), GreenColor);
            CreateBox(district, "ClockPlazaGreen_NorthEast", new Vector3(58f, 0.28f, 20f), new Vector3(13f, 0.28f, 17f), GreenColor);
            CreateBox(district, "ClockPlazaGreen_SouthWest", new Vector3(42f, 0.28f, -7f), new Vector3(17f, 0.28f, 16f), GreenColor);
            CreateBox(district, "ClockPlazaGreen_SouthEast", new Vector3(58f, 0.28f, -10f), new Vector3(12f, 0.28f, 13f), GreenColor);
            CreateBuilding(district, "ClockTower_Blockout", new Vector3(49f, 3.2f, 7f), new Vector3(8f, 6.4f, 8f), "시계탑", LandmarkColor);

            CreateBuilding(district, "Building_R3", new Vector3(38f, 1.6f, 36f), new Vector3(8f, 3.2f, 6f), "건물 3");
            CreateBuilding(district, "Building_R4", new Vector3(51f, 1.6f, 36f), new Vector3(12f, 3.2f, 6f), "건물 4");
            CreateBuilding(district, "Building_R1", new Vector3(70f, 1.6f, 36f), new Vector3(7f, 3.2f, 8f), "건물 1");
            CreateBuilding(district, "Building_R2", new Vector3(75f, 1.6f, 24f), new Vector3(8f, 3.2f, 9f), "건물 2");
            CreateBuilding(district, "Building_R5", new Vector3(78f, 1.6f, 6f), new Vector3(8f, 3.2f, 9f), "건물 5");
        }

        private static void CreateInteractionMarkers(Transform root)
        {
            Transform markers = CreateGroup(root, "06_InteractionMarkerCandidates_Lime");
            Vector3[] points =
            {
                new(-48f, 0.8f, 18f), new(-42f, 0.8f, 9f), new(-30f, 0.8f, 14f),
                new(-24f, 0.8f, 14f), new(-18f, 0.8f, 14f), new(-7f, 0.8f, 0f),
                new(-21f, 0.8f, -9f), new(-7f, 0.8f, -9f), new(-4f, 0.8f, -22f),
                new(40f, 0.8f, 15f), new(48f, 0.8f, 19f), new(55f, 0.8f, 14f),
                new(58f, 0.8f, 5f), new(52f, 0.8f, -4f), new(43f, 0.8f, -2f)
            };

            for (int i = 0; i < points.Length; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = $"InteractionCandidate_{i + 1:00}";
                marker.transform.SetParent(markers);
                marker.transform.position = points[i];
                marker.transform.localScale = Vector3.one * 1.4f;
                marker.GetComponent<Renderer>().sharedMaterial = GetMaterial("Marker", MarkerColor);
            }
        }

        private static void SetupCameraAndLight()
        {
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null)
            {
                GameObject cameraObject = new("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.tag = "MainCamera";
            }

            camera.orthographic = true;
            camera.orthographicSize = 55f;
            camera.transform.position = new Vector3(0f, 72f, -58f);
            camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            camera.clearFlags = CameraClearFlags.Skybox;

            Light light = Object.FindFirstObjectByType<Light>();
            if (light == null)
            {
                GameObject lightObject = new("Directional Light");
                light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
            }

            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.intensity = 1.25f;
        }

        private static void CreateBuilding(Transform parent, string name, Vector3 position, Vector3 size, string label)
        {
            CreateBuilding(parent, name, position, size, label, BuildingColor);
        }

        private static void CreateBuilding(Transform parent, string name, Vector3 position, Vector3 size, string label, Color color)
        {
            GameObject building = CreateBox(parent, name, position, size, color);
            CreateLabel(building.transform, label, new Vector3(0f, size.y * 0.55f + 0.05f, 0f), 2.6f, Color.black);
        }

        private static GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = GetMaterial(name, color);
            return obj;
        }

        private static void CreateLabel(Transform parent, string text, Vector3 localPosition, float size, Color color)
        {
            GameObject labelObject = new($"Label_{text}");
            labelObject.transform.SetParent(parent);
            labelObject.transform.localPosition = localPosition;
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.characterSize = size;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;
        }

        private static Transform CreateGroup(Transform parent, string name)
        {
            GameObject group = new(name);
            group.transform.SetParent(parent);
            group.transform.localPosition = Vector3.zero;
            return group.transform;
        }

        private static Material GetMaterial(string name, Color color)
        {
            string key = $"{color.r:F2}_{color.g:F2}_{color.b:F2}_{color.a:F2}";
            if (Materials.TryGetValue(key, out Material material))
            {
                return material;
            }

            material = new Material(Shader.Find("Standard"))
            {
                name = $"MAT_Blockout_{name}"
            };
            material.color = color;
            Materials[key] = material;
            return material;
        }

        private static void ClearPreviousRoot()
        {
            GameObject previous = GameObject.Find(RootName);
            if (previous != null)
            {
                Object.DestroyImmediate(previous);
            }
        }
    }
}
