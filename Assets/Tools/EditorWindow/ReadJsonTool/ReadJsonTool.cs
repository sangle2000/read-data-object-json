using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Drawing;
using UnityEditor.VersionControl;
using UnityEngine.Assertions;
using Unity.VisualScripting;

namespace MyTools
{
    public class ReadJsonTool : EditorWindow
    {
        static ReadJsonTool win;

        private string dir = "";
        private string nameObject = "";
        private string nameProperty = "";

        private bool readSuccess = false;

        Dictionary<string, bool> showData = new Dictionary<string, bool>();

        JObject dataReaded = new JObject();
        JObject dataDisplay = new JObject();

        Vector2 paletteScrollPos = new Vector2(0, 0);
        List<string> prefabGO = new List<string>();

        string[] prefabList = { "plane", "cube", "sphere", "capsule", "cylinder" };

        public static void InitWindow()
        {
            win = EditorWindow.GetWindow<ReadJsonTool>("Read Object's Data");
            win.minSize = new Vector2(1000, 500);
            win.maxSize = new Vector2(1000, 500);
            win.Show();
        }

        void OnGUI()
        {
            dir = EditorGUILayout.TextField("File path", dir);

            GUILayout.Space(10);

            if (GUILayout.Button("Find Object's Data", GUILayout.Height(20), GUILayout.Width(150)))
            {
                try
                {
                    dataReaded = dataDisplay = LoadJson(dir);
                    readSuccess = true;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    readSuccess = false;
                }
            }

            if (readSuccess)
            {
                paletteScrollPos = GUILayout.BeginScrollView(paletteScrollPos, GUILayout.ExpandWidth(true));
                foreach (JProperty child in dataDisplay.Properties())
                {
                    GUILayout.Space(20);
                    if (!showData.ContainsKey(child.Name))
                    {
                        showData.Add(child.Name, true);
                    }
                    showData[child.Name] = EditorGUILayout.Foldout(showData[child.Name], child.Name);

                    if (showData[child.Name])
                    {
                        if (Selection.activeTransform)
                        {
                            JObject childValue = (JObject)child.Value;

                            Dictionary<string, Vector3> displayGO = new Dictionary<string, Vector3>();
                            string prefabName = "";
                            foreach (JProperty child2 in childValue.Properties())
                            {
                                if (child2.Value.ToString().Contains("(") && child2.Value.ToString().Contains(")") && child2.Value.ToString().Contains(","))
                                {
                                    GUILayout.BeginHorizontal();
                                    EditorGUI.indentLevel++;
                                    string[] listCoord = child2.Value.ToString().Replace("(", ",").Replace(")", ",").Split(",");
                                    Vector3 coord = new Vector3(float.Parse(listCoord[1]), float.Parse(listCoord[2]), float.Parse(listCoord[3]));
                                    coord = EditorGUILayout.Vector3Field(child2.Name, coord);
                                    EditorGUI.indentLevel--;
                                    GUILayout.EndHorizontal();

                                    if (!displayGO.ContainsKey(child2.Name))
                                    {
                                        displayGO.Add(child2.Name, coord);
                                    }
                                }
                                else
                                {
                                    GUILayout.Space(20);
                                    GUILayout.BeginHorizontal();
                                    EditorGUI.indentLevel++;
                                    nameProperty = EditorGUILayout.TextField(child2.Name, child2.Value.ToString(), GUILayout.Width(170 + child2.Value.ToString().Length * 5));
                                    EditorGUI.indentLevel--;
                                    GUILayout.EndHorizontal();

                                    prefabName = child2.Value.ToString();


                                    if (!prefabGO.Contains(child2.Value.ToString()))
                                    {
                                        prefabGO.Add(child2.Value.ToString());
                                        PrefabHandle(child2.Value.ToString());
                                    }
                                }
                            }


                            CreateGameObject(child.Name, displayGO["pos"], displayGO["orient"], displayGO["scale"], prefabName);

                        }
                        else if (!Selection.activeTransform)
                        {
                            showData[child.Name] = false;
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
        }

        private JObject LoadJson(string filePath)
        {
            JObject data = JObject.Parse(File.ReadAllText(filePath));
            return data;
        }

        private void PrefabHandle(string prefabName)
        {
            string direction = "Assets/Prefabs/" + prefabName + ".prefab";
            if (!Directory.Exists("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            if (!Directory.Exists(direction))
            {
                for (int i = 0; i < prefabList.Length; i++)
                {
                    if (prefabList[i] == prefabName.ToLower())
                    {
                        PrefabUtility.CreatePrefab(direction, CreatePrefab(i));
                        break;
                    }
                }
            }
        }

        private GameObject CreatePrefab(int input)
        {
            GameObject prefab;
            switch (input)
            {
                case 0:
                    prefab = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    break;
                case 1:
                    prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case 2:
                    prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                case 3:
                    prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    break;
                case 4:
                    prefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    break;
                default:
                    return null;
            }
            return prefab;
        }

        private void CreateGameObject(string goName, Vector3 pos, Vector3 orient, Vector3 scale, string prefabName)
        {
            string name = goName + "_" + prefabName;
            if (GameObject.Find(prefabName))
            {
                GameObject go = GameObject.Find(prefabName);
                DestroyImmediate(go);
            }

            if (!GameObject.Find(goName))
            {
                GameObject go = new GameObject();
                go.name = goName;
                go.transform.position = pos;

                Quaternion target = Quaternion.Euler(orient.x, orient.y, orient.z);

                go.transform.rotation = Quaternion.Slerp(go.transform.rotation, target, 1);
                go.transform.localScale = scale;

                var prefabObject = LoadPrefabFromFile(prefabName);

                GameObject pNewObject = (GameObject)Instantiate(prefabObject, Vector3.zero, Quaternion.identity);

                pNewObject.name = name;

                pNewObject.transform.SetParent(go.transform, false);
            }
            else
            {
                GameObject go = GameObject.Find(goName);
                go.transform.position = pos;

                Quaternion target = Quaternion.Euler(orient.x, orient.y, orient.z);

                go.transform.rotation = Quaternion.Slerp(go.transform.rotation, target, 1);
                go.transform.localScale = scale;

                var allKids = go.GetComponentsInChildren<Transform>();
                var kid = allKids.FirstOrDefault(k => k.gameObject.name == name);
                var checkKid = allKids.FirstOrDefault(k => prefabList.Contains(k.gameObject.name));

                if (kid == null && checkKid == null)
                {
                    var prefabObject = LoadPrefabFromFile(prefabName);

                    GameObject pNewObject = (GameObject)Instantiate(prefabObject, Vector3.zero, Quaternion.identity);

                    pNewObject.name = name;

                    pNewObject.transform.SetParent(go.transform, false);
                } else if(kid == null && checkKid != null)
                {
                    DestroyImmediate(checkKid);

                    var prefabObject = LoadPrefabFromFile(prefabName);

                    GameObject pNewObject = (GameObject)Instantiate(prefabObject, Vector3.zero, Quaternion.identity);

                    pNewObject.name = name;

                    pNewObject.transform.SetParent(go.transform, false);
                }
            }
        }

        private UnityEngine.Object LoadPrefabFromFile(string filename)
        {
            //Debug.Log("Trying to load LevelPrefab from file (" + filename + ")...");
            //var loadedObject = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/" + filename + ".prefab", typeof(PrefabUtility));
            var loadedObject = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + filename + ".prefab");
            if (loadedObject == null)
            {
                throw new FileNotFoundException("...no file found - please check the configuration");
            }
            return loadedObject;
        }
    }
}

