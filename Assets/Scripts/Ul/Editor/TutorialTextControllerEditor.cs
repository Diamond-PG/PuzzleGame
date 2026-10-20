using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
                "dialoguePuzzleIconPosition",
                "dialoguePuzzleIconSize",
                "dialoguePuzzleIconRotation"
            ),

            new Section(
                "KEEPER 2 - TRAINING",
                "trainingKeeperSprite",
                "trainingStartPage",
                "trainingPuzzleIconPosition",
                "trainingPuzzleIconSize",
                "trainingPuzzleIconRotation"
            ),

            new Section(
                "KEEPER 3 - REJOICES",
                "rejoicesKeeperSprite",
                "rejoicesStartPage",
                "rejoicesEndPage",
                "rejoicesPuzzleIconPosition",
                "rejoicesPuzzleIconSize",
                "rejoicesPuzzleIconRotation"
            ),

            new Section(
                "KEEPER 4 - DOOR",
                "doorKeeperSprite",
                "doorStartPage",
                "doorEndPage",
                "doorPuzzleIconPosition",
                "doorPuzzleIconSize",
                "doorPuzzleIconRotation"
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

            EditorGUILayout.PropertyField(
                property,
                true
            );
        }

        EditorGUILayout.EndVertical();
    }
}