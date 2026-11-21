using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(LevelSelectionController))]
internal sealed class LevelSelectionControllerEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var view = (LevelSelectionController)target;
        var root = new VisualElement();
        var testMode = new Toggle("Enable Levels Test Mode") { name = "levels-test-mode" };
        var actions = new VisualElement();
        var started = new Toggle("New Game Started");
        started.RegisterValueChangedCallback(change => view.SetLevelsTestGameStarted(change.newValue));
        var level = new IntegerField("Test Level") { value = 1, name = "levels-test-level" };

        for (int count = 0; count <= 3; count++)
        {
            int stars = count;
            actions.Add(new Button(() => view.SetLevelsTestStars(level.value, stars))
            {
                text = $"Set {stars} {(stars == 1 ? "Star" : "Stars")}",
                name = $"levels-set-{stars}"
            });
        }

        actions.Add(new Button(view.ResetLevelsTest) { text = "Reset Test", name = "levels-reset" });
        level.RegisterValueChangedCallback(change =>
            level.SetValueWithoutNotify(UnityEngine.Mathf.Clamp(change.newValue, 1, view.LevelsTestLevelCount)));
        testMode.RegisterValueChangedCallback(change =>
        {
            view.SetLevelsTestMode(change.newValue);
            UpdateState();
        });

        void UpdateState()
        {
            if (view == null)
                return;

            testMode.SetValueWithoutNotify(view.LevelsTestModeEnabled);
            started.SetValueWithoutNotify(view.LevelsTestGameStarted);
            testMode.SetEnabled(EditorApplication.isPlaying);
            actions.SetEnabled(EditorApplication.isPlaying && view.LevelsTestModeEnabled);
        }

        root.Add(testMode);
        actions.Insert(0, level);
        actions.Insert(0, started);
        root.Add(actions);
        root.schedule.Execute(UpdateState).Every(150);
        UpdateState();
        InspectorElement.FillDefaultInspector(root, serializedObject, this);
        return root;
    }
}
