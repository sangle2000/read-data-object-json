using UnityEngine;
using UnityEditor;
using MyTools;

public class EditorMenu : MonoBehaviour
{
    [MenuItem("Tools/Json Tools/Read Object's Data")]
    public static void initProjectSetupTool()
    {
        ReadJsonTool.InitWindow();
    }
}