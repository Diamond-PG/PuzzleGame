using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(TutorialTextController))]
[CanEditMultipleObjects]
public class TutorialTextControllerEditor : Editor
{
    private class Section
    {
        public string title;
        public string[] properties;
        public bool expanded;

        public Section(string title, params string[] properties)
        {
            this.title = title;
            this.properties = properties;
        }
    }

    private readonly List<Section> sections =
        new List<Section>
        {
            new Section(
                "TUTORIAL TEXT",
                "tutorialText",
                "pages"
            ),

            new Section(
                "LOCALIZATION",
                "useLocalization",
                "localizationTableName",
                "localizationKeyPrefix"
            ),

            new Section(
                "NAVIGATION BUTTONS",
                "nextButton",
                "previousButton"
            ),

            new Section(
                "START SETTINGS",
                "startPage"
            ),

            new Section(
                "NAME INPUT",
                "nameInputPage",
                "nameConfirmationPage",
                "playerNameInputController"
            ),

            new Section(
                "KEEPER - SHARED REFERENCES",
                "puzzleKeeperImage",
                "keeperPuzzleIcon"
            ),

            new Section(
                "KEEPER 1 - DIALOGUE",
                "dialogueKeeperSprite",
                "dialogueKeeperPosition",
                "dialogueKeeperSize",
                "dialogueKeeperRotationZ",
                "dialogueKeeperScale",
                "dialoguePuzzleIconPosition",
                "dialoguePuzzleIconSize",
                "dialoguePuzzleIconRotation",
                "dialoguePuzzleIconScale"
            ),

            new Section(
                "KEEPER 2 - TRAINING",
                "trainingKeeperSprite",
                "trainingStartPage",
                "trainingKeeperPosition",
                "trainingKeeperSize",
                "trainingKeeperRotationZ",
                "trainingKeeperScale",
                "trainingPuzzleIconPosition",
                "trainingPuzzleIconSize",
                "trainingPuzzleIconRotation",
                "trainingPuzzleIconScale"
            ),

            new Section(
                "KEEPER 3 - REJOICES",
                "rejoicesKeeperSprite",
                "rejoicesStartPage",
                "rejoicesEndPage",
                "rejoicesKeeperPosition",
                "rejoicesKeeperSize",
                "rejoicesKeeperRotationZ",
                "rejoicesKeeperScale",
                "rejoicesPuzzleIconPosition",
                "rejoicesPuzzleIconSize",
                "rejoicesPuzzleIconRotation",
                "rejoicesPuzzleIconScale"
            ),

            new Section(
                "KEEPER 4 - DOOR",
                "doorKeeperSprite",
                "doorStartPage",
                "doorEndPage",
                "doorKeeperPosition",
                "doorKeeperSize",
                "doorKeeperRotationZ",
                "doorKeeperScale",
                "doorPuzzleIconPosition",
                "doorPuzzleIconSize",
                "doorPuzzleIconRotation",
                "doorPuzzleIconScale"
            ),

            new Section(
                "MOVEMENT TUTORIAL",
                "movementIntroPage",
                "movementPracticePage",
                "movementSuccessPage"
            ),

            new Section(
                "JUMP TUTORIAL",
                "jumpPracticePage",
                "jumpSuccessPage",
                "jumpControlGroup"
            ),

            new Section(
                "KICK TUTORIAL",
                "kickIntroPage",
                "kickPracticePage",
                "kickSuccessPage",
                "kickControlGroup"
            ),

            new Section(
                "TUTORIAL CONTROL VISUALS",
                "leftControlGroup",
                "rightControlGroup",
                "lockedControlGroups",
                "lockedControlAlpha",
                "unlockedControlAlpha"
            ),

            // ====================================================
            // NEW - DARKNESS MASK TUTORIAL WINDOWS
            // ====================================================

            new Section(
                "DARKNESS MASK TUTORIAL WINDOWS",
                "prisonDarknessMask"
            ),

            // ====================================================
            // NEW - AUTOSAVE RIGHT KEEPER PAGE
            // ====================================================

            new Section(
                "AUTOSAVE - MAIN KEEPER",
                "enableExactPageAutosave"
            )
        };

    private GUIStyle headerStyle;
    private GUIStyle contentStyle;

    private readonly Color headerBackground =
        new Color32(37, 65, 105, 255);

    private readonly Color headerTextColor =
        new Color32(135, 199, 255, 255);

    // ============================================================
    // ENABLE
    // ============================================================

    private void OnEnable()
    {
        foreach (Section section in sections)
        {
            section.expanded =
                SessionState.GetBool(
                    GetFoldoutKey(section.title),
                    false
                );
        }
    }

    private string GetFoldoutKey(string title)
    {
        return "TutorialTextControllerEditor_" + title;
    }

    // ============================================================
    // STYLES
    // ============================================================

    private void EnsureStyles()
    {
        if (headerStyle == null)
        {
            headerStyle =
                new GUIStyle(EditorStyles.foldout);

            headerStyle.fontStyle =
                FontStyle.Bold;

            headerStyle.fontSize = 12;

            headerStyle.normal.textColor =
                headerTextColor;

            headerStyle.onNormal.textColor =
                headerTextColor;

            headerStyle.focused.textColor =
                headerTextColor;

            headerStyle.onFocused.textColor =
                headerTextColor;

            headerStyle.active.textColor =
                headerTextColor;

            headerStyle.onActive.textColor =
                headerTextColor;
        }

        if (contentStyle == null)
        {
            contentStyle =
                new GUIStyle(EditorStyles.helpBox);

            contentStyle.padding =
                new RectOffset(12, 8, 8, 8);
        }
    }

    // ============================================================
    // INSPECTOR
    // ============================================================

    public override void OnInspectorGUI()
    {
        InitializeKeeperTransformsInEditor();
        serializedObject.Update();

        EnsureStyles();

        EditorGUILayout.Space(5);

        foreach (Section section in sections)
        {
            DrawSection(section);
        }

        EditorGUILayout.Space(5);

        serializedObject.ApplyModifiedProperties();
    }

    // ============================================================
    // SAFE ONE-TIME KEEPER TRANSFORM MIGRATION
    // ============================================================
    // Newly added per-pose fields must start at the Keeper's CURRENT
    // scene placement. No default (0,0) jump and no lost icon tuning.
    private void InitializeKeeperTransformsInEditor()
    {
        foreach (Object targetObject in targets)
        {
            TutorialTextController controller =
                targetObject as TutorialTextController;
            if (controller == null)
                continue;

            SerializedObject objectData = new SerializedObject(controller);
            SerializedProperty initialized = objectData.FindProperty(
                "keeperTransformsInitialized");
            SerializedProperty keeperImage = objectData.FindProperty(
                "puzzleKeeperImage");

            if (initialized == null || initialized.boolValue ||
                keeperImage == null)
                continue;

            Image image = keeperImage.objectReferenceValue as Image;
            if (image == null)
                continue;

            RectTransform rect = image.rectTransform;
            Vector2 position = rect.anchoredPosition;
            Vector2 size = rect.sizeDelta;
            float rotation = rect.localEulerAngles.z;
            Vector2 scale = new Vector2(
                rect.localScale.x, rect.localScale.y);

            Undo.RecordObject(controller, "Initialize Keeper Pose Transforms");
            foreach (string prefix in new[]
                     { "dialogue", "training", "rejoices", "door" })
            {
                objectData.FindProperty(prefix + "KeeperPosition")
                    .vector2Value = position;
                objectData.FindProperty(prefix + "KeeperSize")
                    .vector2Value = size;
                objectData.FindProperty(prefix + "KeeperRotationZ")
                    .floatValue = rotation;
                objectData.FindProperty(prefix + "KeeperScale")
                    .vector2Value = scale;
            }

            SerializedProperty iconTransform = objectData.FindProperty(
                "keeperPuzzleIcon");
            RectTransform icon = iconTransform != null
                ? iconTransform.objectReferenceValue as RectTransform
                : null;
            Vector2 iconScale = icon != null
                ? new Vector2(icon.localScale.x, icon.localScale.y)
                : Vector2.one;
            foreach (string prefix in new[]
                     { "dialogue", "training", "rejoices", "door" })
            {
                objectData.FindProperty(prefix + "PuzzleIconScale")
                    .vector2Value = iconScale;
            }

            initialized.boolValue = true;
            objectData.ApplyModifiedProperties();
        }
    }

    // ============================================================
    // DRAW SECTION
    // ============================================================

    private void DrawSection(Section section)
    {
        EditorGUILayout.Space(3);

        Rect headerRect =
            GUILayoutUtility.GetRect(
                0,
                27,
                GUILayout.ExpandWidth(true)
            );

        EditorGUI.DrawRect(
            headerRect,
            headerBackground
        );

        Rect foldoutRect =
            new Rect(
                headerRect.x + 8,
                headerRect.y + 4,
                headerRect.width - 16,
                headerRect.height - 8
            );

        EditorGUI.BeginChangeCheck();

        bool newExpanded =
            EditorGUI.Foldout(
                foldoutRect,
                section.expanded,
                section.title,
                true,
                headerStyle
            );

        if (EditorGUI.EndChangeCheck())
        {
            section.expanded =
                newExpanded;

            SessionState.SetBool(
                GetFoldoutKey(section.title),
                newExpanded
            );
        }

        if (!section.expanded)
            return;

        EditorGUILayout.BeginVertical(
            contentStyle
        );

        foreach (string propertyName in section.properties)
        {
            if (propertyName.EndsWith("KeeperPosition"))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField(
                    "KEEPER RECT TRANSFORM", EditorStyles.boldLabel);
            }
            else if (propertyName.EndsWith("PuzzleIconPosition"))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField(
                    "BLUE PUZZLE MEDALLION", EditorStyles.boldLabel);
            }

            SerializedProperty property =
                serializedObject.FindProperty(
                    propertyName
                );

            if (property == null)
            {
                EditorGUILayout.HelpBox(
                    "Property not found: " +
                    propertyName,
                    MessageType.Warning
                );

                continue;
            }

            string label = property.displayName;
            if (propertyName.EndsWith("KeeperPosition"))
                label = "Keeper Position";
            else if (propertyName.EndsWith("KeeperSize"))
                label = "Keeper Size";
            else if (propertyName.EndsWith("KeeperRotationZ"))
                label = "Keeper Rotation Z";
            else if (propertyName.EndsWith("KeeperScale"))
                label = "Keeper Scale";
            else if (propertyName.EndsWith("PuzzleIconPosition"))
                label = "Puzzle Position";
            else if (propertyName.EndsWith("PuzzleIconSize"))
                label = "Puzzle Size";
            else if (propertyName.EndsWith("PuzzleIconRotation"))
                label = "Puzzle Rotation Z";
            else if (propertyName.EndsWith("PuzzleIconScale"))
                label = "Puzzle Scale";

            EditorGUILayout.PropertyField(
                property,
                new GUIContent(label),
                true
            );
        }

        EditorGUILayout.EndVertical();
    }
}
