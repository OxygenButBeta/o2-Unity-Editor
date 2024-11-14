﻿using System;
using System.IO;
using o2.EditorTools;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using o2.Runtime.ScriptGeneration;
using UnityEditorInternal;

[CustomEditor(typeof(o2EditorDataAsset))]
public class o2EditorDataAssetCustomEditor : Editor {
    const string AUTO_GENERATED_SCRIPT_FILE_NAME = "o2EditorAutoGeneratedGUIContext.cs";
    const string AUTO_GENERATED_TAG_ENUM_FILE_NAME = "o2EditorAutoGeneratedTagEnum.cs";
    const string AUTO_GENERATED_CLASS_NAME = "o2EditorAutoGeneratedGUIContext";

    public override void OnInspectorGUI() {
        var editorDataAsset = (o2EditorDataAsset)target;
        base.OnInspectorGUI();


        GUILayout.Space(10);
        if (!GUILayout.Button("Re-Generate Scripts"))
            return;

        ScriptBuilder scriptBuilder = new(AUTO_GENERATED_CLASS_NAME)
        {
            ClassType = ClassType._sealed,
            Namespace = "o2.Editor_Runtime_Generated"
        };
        scriptBuilder
            .AddUsing("o2.EditorTools")
            .AddUsing("UnityEditor")
            .AddComment("ReSharper disable All")
            .AddComment($"Generation Time UTC {DateTime.UtcNow}");

        for (var i = 0; i < editorDataAsset.QuickFab.Prefabs.Count; i++)
        {
            var subMenu = editorDataAsset.QuickFab.Prefabs[i].SubMenu;
            subMenu = string.IsNullOrEmpty(subMenu) ? "" : subMenu + "/";

            MethodBuilder methodBuilder = new()
            {
                ExpressionBody = true,
                MethodType = MethodType._static
            };

            var attributePath = $"GameObject/o2/Quick Fab/{subMenu + editorDataAsset.QuickFab.Prefabs[i].Name}";
            methodBuilder.Name = $"Draw_ContextMenu_{GenerateRandomString(6)}";
            methodBuilder.Body =
                $"PrefabUtility.InstantiatePrefab(o2EditorUtility.GetDataAsset().QuickFabAssets.Prefabs[{i.ToString()}].Prefab);";
            methodBuilder.AddAttribute("MenuItem", $"\"{attributePath}\", false, 0");
            scriptBuilder.AddMethod(methodBuilder);
        }


        foreach (var draftPreset in editorDataAsset.ScriptDrafts)
        {
            MethodBuilder methodBuilder = new()
            {
                MethodType = MethodType._static,
                AccessModifier = AccessModifier._public
            };
            methodBuilder.ReturnType = typeof(void);
            methodBuilder.Name = $"Draw_ScriptDraft_ContextMenu_{GenerateRandomString(6)}";
            var attribute = $"Assets/Script Drafts/{draftPreset.Name}";
            methodBuilder.AddAttribute("MenuItem", $"\"{attribute}\", false, 0");
            scriptBuilder.AddMethod(methodBuilder);
            methodBuilder.Body =
                $"string fileName = \"{draftPreset.Name}\";\n " +
                $"foreach (var draftPreset in o2EditorUtility.GetDataAsset().ScriptDrafts)\n {{\n " +
                $"  if (draftPreset.Name == fileName)\n " +
                $"{{\n  o2EditorUtility.CreateScript(o2EditorUtility.GetCurrentFolderPath(), \"New{draftPreset.Name}Script\", draftPreset.Build());\n  " +
                $"    break;\n  }}\n   }}";
        }

        if (editorDataAsset.ValidatorOptions.Enable)
        {
            MethodBuilder builder = new()
            {
                MethodType = MethodType._static,
                AccessModifier = AccessModifier._private,
                ReturnType = typeof(void),
            };

            builder.Name = GenerateRandomString(8);
            builder.AddAttributeLine("[MenuItem(\"O2/Validation Utility\")]");
            builder.Body = "ValidatorUtilityGUI.DeployUtility();";
            scriptBuilder.AddMethod(builder);
        }

        if (editorDataAsset.SceneUtility.Enable)
        {
            MethodBuilder builder = new()
            {
                MethodType = MethodType._static,
                AccessModifier = AccessModifier._private,
                ReturnType = typeof(void),
            };

            builder.Name = GenerateRandomString(8);
            builder.AddAttributeLine("[MenuItem(\"O2/Scene Utility\")]");
            builder.Body = "SceneUtilityGUI.Init();";
            scriptBuilder.AddMethod(builder);
        }

        scriptBuilder.AddUsing("o2.EditorTools.Validator");
        var fullFolderPath = Path.Combine(Application.dataPath, editorDataAsset.AutoGeneratedScriptSavePath);
        var fullFilePath = fullFolderPath + "/" + AUTO_GENERATED_SCRIPT_FILE_NAME;

        if (!Directory.Exists(fullFolderPath))
            Directory.CreateDirectory(fullFolderPath);

        File.WriteAllText(fullFilePath, scriptBuilder.Build());

        EnumBuilder enumBuilder = new("TagEnumAutoGenerated", "o2.Editor.Tools.HierarchyHighlighter");
        enumBuilder.AddEnumValue(InternalEditorUtility.tags);

        var enumFilePath = Path.GetDirectoryName(o2EditorUtility.GetAssemblyDefinitionPath()) + "/Hierarchy Highlighter/Auto Generated/" +
                           AUTO_GENERATED_TAG_ENUM_FILE_NAME;

        if (File.Exists(enumFilePath))
        {
            File.Delete(enumFilePath);
        }

        File.WriteAllText(enumFilePath, enumBuilder.Build());

        AssetDatabase.Refresh();
    }

    public static string GenerateRandomString(int length) {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        char[] stringChars = new char[length];

        for (int i = 0; i < length; i++)
            stringChars[i] = chars[Random.Range(0, chars.Length)];

        return new string(stringChars);
    }

    void x() {
        var methodBuilder = new MethodBuilder()
        {
            AccessModifier = AccessModifier._public,
            Name = "MyMethod",
            ReturnType = typeof(int),
            Body = "Console.WriteLine(\"Hello World\");",
            ExpressionBody = false,
            MethodType = MethodType._instance
        };

        methodBuilder.AddAttribute("__DynamicallyInvokable", "")
            .AddParameter("int", "myParam")
            .AddParameter("string", "myStringParam")
            .AddOptionalParameter("int", "myOptionalParam", "-1");


        var scriptBuilder = new ScriptBuilder("MyGeneratedClass")
            .SetBaseClass<MonoBehaviour>()
            .AddInterface<IBuildable>()
            .AddUsing("System")
            .AddUsing("UnityEngine")
            .AddField(new FieldBuilder("myField", "int"))
            .AddMethod(methodBuilder)
            .Build();

        // Save the script to a file or something..
    }
}