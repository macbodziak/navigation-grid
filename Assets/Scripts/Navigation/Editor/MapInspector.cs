using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Navigation
{
    [CustomEditor(typeof(Map))]
    public class MapInspector : Editor
    {
        FloatField TileSizeField;
        IntegerField WidthField;
        IntegerField HeightField;
        Button CreateMapButton;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            TileSizeField = new FloatField("Tile Size");
            TileSizeField.AddToClassList("unity-base-field__aligned");
            TileSizeField.bindingPath = "tileSize";

            WidthField = new IntegerField("Width");
            WidthField.AddToClassList("unity-base-field__aligned");
            WidthField.bindingPath = "width";

            HeightField = new IntegerField("Height");
            HeightField.AddToClassList("unity-base-field__aligned");
            HeightField.bindingPath = "height";

            CreateMapButton = new Button(OnCreateMapButtonClicked);
            CreateMapButton.text = "Create Map";
            CreateMapButton.AddToClassList("unity-base-field__aligned");
            // CreateMapButton.clicked+=
            // CreateMapButton
            // CreateMapButton
            // CreateMapButton

            root.Add(TileSizeField);
            root.Add(WidthField);
            root.Add(HeightField);
            root.Add(CreateMapButton);

            return root;
        }


        private void OnCreateMapButtonClicked()
        {
            Map map = target as Map;
            map.CreateMap(WidthField.value, HeightField.value);
            EditorUtility.SetDirty(map);
        }
    }
}