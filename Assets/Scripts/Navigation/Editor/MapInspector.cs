using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;
using System;

namespace Navigation
{
    [CustomEditor(typeof(Map))]
    public class MapInspector : Editor
    {
        private const string TILESIZE_PREF_KEY = "TileSizeBakeField";
        private const string WIDTH_PREF_KEY = "WidthBakeField";
        private const string HEIGHT_PREF_KEY = "HeightBakeField";
        private const string NOTWALKABLELAYERMASK_PREF_KEY = "NotWalkableLayerMaskField";
        private const int MIN_GRID_SIZE = 1;
        private const int MAX_GRID_SIZE = 100;
        private const string RAYLENGTH_PREF_KEY = "RayLengthField";
        private const string COLLIDER_SIZE_PREF_KEY = "ColliderSizeField";

        FloatField TileSizeBakeField;
        IntegerField WidthBakeField;
        IntegerField HeightBakeField;
        LayerMaskField NotWalkableLayerMaskField;
        FloatField RayLengthField;
        FloatField ColliderSizeField;
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
            TileSizeBakeField.value = EditorPrefs.GetFloat(TILESIZE_PREF_KEY, 2);
            TileSizeBakeField.RegisterValueChangedCallback(OnTileSizeFieldValueChanged);

            WidthBakeField = new IntegerField("Width");
            WidthBakeField.AddToClassList("unity-base-field__aligned");
            WidthBakeField.value = EditorPrefs.GetInt(WIDTH_PREF_KEY, 10);
            WidthBakeField.RegisterValueChangedCallback(OnWidthFieldChanged);

            HeightBakeField = new IntegerField("Height");
            HeightBakeField.AddToClassList("unity-base-field__aligned");
            HeightBakeField.value = EditorPrefs.GetInt(HEIGHT_PREF_KEY, 10);
            HeightBakeField.RegisterValueChangedCallback(OnHeightFieldChanged);

            NotWalkableLayerMaskField = new LayerMaskField("Not Walkable Layers");
            NotWalkableLayerMaskField.AddToClassList("unity-base-field__aligned");
            NotWalkableLayerMaskField.value = EditorPrefs.GetInt(NOTWALKABLELAYERMASK_PREF_KEY);
            NotWalkableLayerMaskField.RegisterValueChangedCallback(evt =>
            {
                EditorPrefs.SetInt(NOTWALKABLELAYERMASK_PREF_KEY, evt.newValue);
            });

            RayLengthField = new FloatField("Ray Length");
            RayLengthField.AddToClassList("unity-base-field__aligned");
            RayLengthField.value = EditorPrefs.GetFloat(RAYLENGTH_PREF_KEY, 100);
            RayLengthField.RegisterValueChangedCallback(OnRayLengthChanged);

            ColliderSizeField = new FloatField("Box Collider Size (Normalized)");
            ColliderSizeField.AddToClassList("unity-base-field__aligned");
            ColliderSizeField.value = EditorPrefs.GetFloat(COLLIDER_SIZE_PREF_KEY, 0.9f);
            ColliderSizeField.RegisterValueChangedCallback(OnColliderSizeChanged);

            CreateMapButton = new Button(OnCreateMapButtonClicked);
            CreateMapButton.text = "Bake Map";
            CreateMapButton.AddToClassList("unity-base-field__aligned");

            MapCreationBox.Add(MapCreationLabel);
            MapCreationBox.Add(TileSizeBakeField);
            MapCreationBox.Add(WidthBakeField);
            MapCreationBox.Add(HeightBakeField);
            MapCreationBox.Add(NotWalkableLayerMaskField);
            MapCreationBox.Add(RayLengthField);
            MapCreationBox.Add(ColliderSizeField);
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

        private void OnColliderSizeChanged(ChangeEvent<float> evt)
        {
            if (evt.newValue < 0.1f)
            {
                ColliderSizeField.value = 0.1f;
            }
            else if (evt.newValue > 2.0f)
            {
                ColliderSizeField.value = 2.0f;
            }
            else
            {
                ColliderSizeField.value = evt.newValue;
            }

            EditorPrefs.SetFloat(COLLIDER_SIZE_PREF_KEY, ColliderSizeField.value);
        }


        private void OnRayLengthChanged(ChangeEvent<float> evt)
        {
            if (evt.newValue < 0.1f)
            {
                RayLengthField.value = 0.1f;
            }
            else
            {
                RayLengthField.value = evt.newValue;
            }

            EditorPrefs.SetFloat(RAYLENGTH_PREF_KEY, RayLengthField.value);
        }


        private void OnCreateMapButtonClicked()
        {
            Map map = target as Map;

            map.CreateMap(WidthBakeField.value, HeightBakeField.value, TileSizeBakeField.value, NotWalkableLayerMaskField.value, ColliderSizeField.value, RayLengthField.value);
            EditorUtility.SetDirty(map);
        }


        private void OnTileSizeFieldValueChanged(ChangeEvent<float> evt)
        {
            if (evt.newValue < 0.1f)
            {
                TileSizeBakeField.value = 0.1f;
                EditorPrefs.SetFloat(TILESIZE_PREF_KEY, TileSizeBakeField.value);
                return;
            }

            TileSizeBakeField.value = evt.newValue;
            EditorPrefs.SetFloat(TILESIZE_PREF_KEY, TileSizeBakeField.value);
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


        private void OnHeightFieldChanged(ChangeEvent<int> evt)
        {
            if (evt.newValue < MIN_GRID_SIZE)
            {
                HeightBakeField.value = MIN_GRID_SIZE;
            }
            else if (evt.newValue > MAX_GRID_SIZE)
            {
                HeightBakeField.value = MAX_GRID_SIZE;
            }
            else
            {
                HeightBakeField.value = evt.newValue;
            }
            EditorPrefs.SetInt(HEIGHT_PREF_KEY, HeightBakeField.value);
        }


        private void OnWidthFieldChanged(ChangeEvent<int> evt)
        {
            if (evt.newValue < MIN_GRID_SIZE)
            {
                WidthBakeField.value = MIN_GRID_SIZE;
            }
            else if (evt.newValue > MAX_GRID_SIZE)
            {
                WidthBakeField.value = MAX_GRID_SIZE;
            }
            else
            {
                WidthBakeField.value = evt.newValue;
            }
            EditorPrefs.SetInt(WIDTH_PREF_KEY, WidthBakeField.value);
        }
    }
}