using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

namespace Navigation
{
    [CustomEditor(typeof(Map))]
    public class MapInspector : Editor
    {
        FloatField TileSizeBakeField;
        IntegerField WidthBakeField;
        IntegerField HeightBakeField;
        LayerMaskField NotWalkableLayerMaskField;
        FloatField TileSizeField;
        IntegerField WidthField;
        IntegerField HeightField;
        Button CreateMapButton;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            Box MapCreationBox = new Box();
            MapCreationBox.style.borderTopWidth = 2;
            MapCreationBox.style.borderLeftWidth = 2;
            MapCreationBox.style.borderRightWidth = 2;
            MapCreationBox.style.borderBottomWidth = 2;
            MapCreationBox.style.borderTopColor = new Color(0.35f, 0.35f, 0.35f);
            MapCreationBox.style.borderLeftColor = new Color(0.35f, 0.35f, 0.35f);
            MapCreationBox.style.borderRightColor = new Color(0.35f, 0.35f, 0.35f);
            MapCreationBox.style.borderBottomColor = new Color(0.35f, 0.35f, 0.35f);
            MapCreationBox.style.paddingTop = 3;
            MapCreationBox.style.paddingLeft = 3;
            MapCreationBox.style.paddingRight = 6;
            MapCreationBox.style.paddingBottom = 3;
            MapCreationBox.style.marginBottom = 15;

            Label MapCreationLabel = new Label("Map Baking:");
            MapCreationLabel.style.marginBottom = 6;
            MapCreationLabel.style.fontSize = 15;

            TileSizeBakeField = new FloatField("Tile Size");
            TileSizeBakeField.AddToClassList("unity-base-field__aligned");
            TileSizeBakeField.value = 2f;
            TileSizeBakeField.RegisterValueChangedCallback(OnTileSizeFieldValueChanged);

            WidthBakeField = new IntegerField("Width");
            WidthBakeField.AddToClassList("unity-base-field__aligned");
            WidthBakeField.value = 20;
            SetRangeOnIntegerField(WidthBakeField, 1, 100);

            HeightBakeField = new IntegerField("Height");
            HeightBakeField.AddToClassList("unity-base-field__aligned");
            HeightBakeField.value = 20;
            SetRangeOnIntegerField(HeightBakeField, 1, 100);

            NotWalkableLayerMaskField = new LayerMaskField("Not Walkable Layers");
            NotWalkableLayerMaskField.AddToClassList("unity-base-field__aligned");

            CreateMapButton = new Button(OnCreateMapButtonClicked);
            CreateMapButton.text = "Bake Map";
            CreateMapButton.AddToClassList("unity-base-field__aligned");


            MapCreationBox.Add(MapCreationLabel);
            MapCreationBox.Add(TileSizeBakeField);
            MapCreationBox.Add(WidthBakeField);
            MapCreationBox.Add(HeightBakeField);
            MapCreationBox.Add(NotWalkableLayerMaskField);
            MapCreationBox.Add(CreateMapButton);


            Box ActualMapDataBox = new Box();
            ActualMapDataBox.style.borderTopWidth = 2;
            ActualMapDataBox.style.borderLeftWidth = 2;
            ActualMapDataBox.style.borderRightWidth = 2;
            ActualMapDataBox.style.borderBottomWidth = 2;
            ActualMapDataBox.style.borderTopColor = new Color(0.35f, 0.35f, 0.35f);
            ActualMapDataBox.style.borderLeftColor = new Color(0.35f, 0.35f, 0.35f);
            ActualMapDataBox.style.borderRightColor = new Color(0.35f, 0.35f, 0.35f);
            ActualMapDataBox.style.borderBottomColor = new Color(0.35f, 0.35f, 0.35f);
            ActualMapDataBox.style.paddingTop = 3;
            ActualMapDataBox.style.paddingLeft = 3;
            ActualMapDataBox.style.paddingRight = 6;
            ActualMapDataBox.style.paddingBottom = 3;
            ActualMapDataBox.style.marginBottom = 15;

            HeightField = new IntegerField("Height");
            HeightField.bindingPath = "height";
            HeightField.SetEnabled(false);

            WidthField = new IntegerField("Width");
            WidthField.bindingPath = "width";
            WidthField.SetEnabled(false);

            TileSizeField = new FloatField("Tile size");
            TileSizeField.bindingPath = "tileSize";
            TileSizeField.SetEnabled(false);

            ActualMapDataBox.Add(HeightField);
            ActualMapDataBox.Add(WidthField);
            ActualMapDataBox.Add(TileSizeField);

            root.Add(MapCreationBox);
            root.Add(ActualMapDataBox);

            return root;
        }


        private void OnCreateMapButtonClicked()
        {
            Map map = target as Map;

            map.CreateMap(WidthBakeField.value, HeightBakeField.value, TileSizeBakeField.value, NotWalkableLayerMaskField.value);
            EditorUtility.SetDirty(map);
        }


        private void OnTileSizeFieldValueChanged(ChangeEvent<float> evt)
        {
            if (evt.newValue < 0.1f)
            {
                TileSizeBakeField.value = 0.1f;
                return;
            }

            TileSizeBakeField.value = evt.newValue;
        }


        private void SetRangeOnIntegerField(IntegerField integerField, int min, int max)
        {
            integerField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue < min)
                {
                    integerField.value = min;
                }
                else if (evt.newValue > max)
                {
                    integerField.value = max;
                }
                else
                {
                    integerField.value = evt.newValue;
                }
            });
        }
    }
}