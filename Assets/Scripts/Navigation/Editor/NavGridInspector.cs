using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;
using System;

namespace Navigation
{
    [CustomEditor(typeof(NavGrid), true)]
    public class MapInspector : Editor
    {
        private const string TILESIZE_PREF_KEY = "TileSizeBakeField";
        private const string WIDTH_PREF_KEY = "WidthBakeField";
        private const string HEIGHT_PREF_KEY = "HeightBakeField";
        private const string NOTWALKABLELAYERMASK_PREF_KEY = "NotWalkableLayerMaskField";
        private const string COLLISIONLAYER_PREF_KEY = "CollisionLayerField";
        private const int MIN_GRID_SIZE = 1;
        private const int MAX_GRID_SIZE = 100;
        private const string RAYLENGTH_PREF_KEY = "RayLengthField";
        private const string COLLIDER_SIZE_PREF_KEY = "ColliderSizeField";
        private const string SCAN_ACTORS_PREF_KEY = "ScanActorsToggle";

        FloatField TileSizeBakeField;
        IntegerField WidthBakeField;
        IntegerField HeightBakeField;
        LayerMaskField NotWalkableLayerMaskField;
        LayerField CollisionLayerField;
        FloatField RayLengthField;
        FloatField ColliderSizeField;
        FloatField TileSizeField;
        IntegerField WidthField;
        IntegerField HeightField;
        Toggle ScanActorsToggle;
        Button CreateMapButton;
        Button ScanActorsButton;
        Toggle ShowTileCenterToggle;
        Toggle ShowTileOutlineToggle;
        Toggle ShowTileInfoTextToggle;
        SliderInt TileInfoTextSizeSlider;
        ColorField TileInfoTextColorField;
        Toggle ShowNodeGridCoordinatesTextToggle;
        Toggle ShowNodeWalkableTextToggle;
        Toggle ShowNodeMovementCostTextToggle;
        Toggle ShowOccupyingActorTextToggle;
        Foldout TextInfoDetailsBox;


        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            Foldout MapCreationFoldout = CreateMapCreationSection();

            Foldout DebugFoldout = CreateDebugSection();

            Box ActualMapDataBox = CreateActualMapDataSection();


            root.Add(MapCreationFoldout);
            root.Add(DebugFoldout);
            root.Add(ActualMapDataBox);

            return root;
        }


        private Foldout CreateMapCreationSection()
        {
            Foldout MapCreationFoldout = new Foldout();
            MapCreationFoldout.text = "Map Generation";
            MapCreationFoldout.style.borderTopWidth = 1;
            MapCreationFoldout.style.borderLeftWidth = 1;
            MapCreationFoldout.style.borderRightWidth = 1;
            MapCreationFoldout.style.borderBottomWidth = 1;
            MapCreationFoldout.style.borderTopColor = new Color(0.35f, 0.35f, 0.35f);
            MapCreationFoldout.style.borderLeftColor = new Color(0.35f, 0.35f, 0.35f);
            MapCreationFoldout.style.borderRightColor = new Color(0.35f, 0.35f, 0.35f);
            MapCreationFoldout.style.borderBottomColor = new Color(0.35f, 0.35f, 0.35f);
            MapCreationFoldout.style.paddingTop = 3;
            MapCreationFoldout.style.paddingLeft = 3;
            MapCreationFoldout.style.paddingRight = 6;
            MapCreationFoldout.style.paddingBottom = 3;
            MapCreationFoldout.style.marginBottom = 15;


            TileSizeBakeField = new FloatField("Tile Size");
            TileSizeBakeField.AddToClassList("unity-base-field__aligned");
            TileSizeBakeField.value = EditorPrefs.GetFloat(TILESIZE_PREF_KEY, 2);
            TileSizeBakeField.RegisterValueChangedCallback(OnTileSizeFieldValueChanged);

            WidthBakeField = new IntegerField("Width (X-Axis)");
            WidthBakeField.AddToClassList("unity-base-field__aligned");
            WidthBakeField.value = EditorPrefs.GetInt(WIDTH_PREF_KEY, 10);
            WidthBakeField.RegisterValueChangedCallback(OnWidthFieldChanged);

            HeightBakeField = new IntegerField("Height (Z-Axis)");
            HeightBakeField.AddToClassList("unity-base-field__aligned");
            HeightBakeField.value = EditorPrefs.GetInt(HEIGHT_PREF_KEY, 10);
            HeightBakeField.RegisterValueChangedCallback(OnHeightFieldChanged);

            NotWalkableLayerMaskField = new LayerMaskField("Not Walkable Layers");
            NotWalkableLayerMaskField.tooltip = "Which layers will make a cell not walkable during baking";
            NotWalkableLayerMaskField.AddToClassList("unity-base-field__aligned");
            NotWalkableLayerMaskField.value = EditorPrefs.GetInt(NOTWALKABLELAYERMASK_PREF_KEY);
            NotWalkableLayerMaskField.RegisterValueChangedCallback(evt =>
            {
                EditorPrefs.SetInt(NOTWALKABLELAYERMASK_PREF_KEY, evt.newValue);
            });

            CollisionLayerField = new LayerField("Grid Collision Layer");
            CollisionLayerField.AddToClassList("unity-base-field__aligned");
            CollisionLayerField.tooltip = "The Layer for detecting clicks with a mouse";
            CollisionLayerField.value = EditorPrefs.GetInt(COLLISIONLAYER_PREF_KEY);
            CollisionLayerField.RegisterValueChangedCallback(evt =>
            {
                EditorPrefs.SetInt(COLLISIONLAYER_PREF_KEY, evt.newValue);
            });

            RayLengthField = new FloatField("Ray Length");
            RayLengthField.tooltip = "The length of the ray used for testing if cell is walkable\nIf ray encounters a collider with <color=#add8e6ff>Not Walkable Layer</color> it will mark cell as not walkable";
            RayLengthField.AddToClassList("unity-base-field__aligned");
            RayLengthField.value = EditorPrefs.GetFloat(RAYLENGTH_PREF_KEY, 100);
            RayLengthField.RegisterValueChangedCallback(OnRayLengthChanged);

            ColliderSizeField = new FloatField("Box Collider Size (Normalized)");
            ColliderSizeField.tooltip = "The normalized size of the box collider used for testing if a cell is walkable\nSize of 1 means sieze of cell";
            ColliderSizeField.AddToClassList("unity-base-field__aligned");
            ColliderSizeField.value = EditorPrefs.GetFloat(COLLIDER_SIZE_PREF_KEY, 0.9f);
            ColliderSizeField.RegisterValueChangedCallback(OnColliderSizeChanged);

            ScanActorsToggle = new Toggle("Install Actors");
            ScanActorsToggle.tooltip = "If checked, the baking process will look for Actors in the scene and Install them on the grid if possible";
            ScanActorsToggle.AddToClassList("unity-base-field__aligned");
            ScanActorsToggle.value = EditorPrefs.GetBool(SCAN_ACTORS_PREF_KEY, true);
            ScanActorsToggle.RegisterValueChangedCallback(evt =>
            {
                EditorPrefs.SetBool(SCAN_ACTORS_PREF_KEY, evt.newValue);
            });


            CreateMapButton = new Button(OnCreateMapButtonClicked);
            CreateMapButton.text = "Bake Map";
            CreateMapButton.AddToClassList("unity-base-field__aligned");
            CreateMapButton.SetEnabled(!EditorApplication.isPlaying);

            ScanActorsButton = new Button(OnScnaActorsButtonClicked);
            ScanActorsButton.text = "Scan Actors";
            ScanActorsButton.tooltip = "Look for Actors in the scene and Install them on the grid if possible. Will clear null references if previously installed Actor has been deleted";
            ScanActorsButton.AddToClassList("unity-base-field__aligned");
            ScanActorsButton.SetEnabled(!EditorApplication.isPlaying);

            MapCreationFoldout.Add(TileSizeBakeField);
            MapCreationFoldout.Add(WidthBakeField);
            MapCreationFoldout.Add(HeightBakeField);
            MapCreationFoldout.Add(NotWalkableLayerMaskField);
            MapCreationFoldout.Add(CollisionLayerField);
            MapCreationFoldout.Add(RayLengthField);
            MapCreationFoldout.Add(ColliderSizeField);
            MapCreationFoldout.Add(ScanActorsToggle);
            MapCreationFoldout.Add(CreateMapButton);
            MapCreationFoldout.Add(ScanActorsButton);

            return MapCreationFoldout;
        }


        private Foldout CreateDebugSection()
        {
            Foldout DebugFoldout = new Foldout();

            DebugFoldout.text = "Debug Visualisation";
            DebugFoldout.style.borderTopWidth = 1;
            DebugFoldout.style.borderLeftWidth = 1;
            DebugFoldout.style.borderRightWidth = 1;
            DebugFoldout.style.borderBottomWidth = 1;
            DebugFoldout.style.borderTopColor = new Color(0.35f, 0.35f, 0.35f);
            DebugFoldout.style.borderLeftColor = new Color(0.35f, 0.35f, 0.35f);
            DebugFoldout.style.borderRightColor = new Color(0.35f, 0.35f, 0.35f);
            DebugFoldout.style.borderBottomColor = new Color(0.35f, 0.35f, 0.35f);
            DebugFoldout.style.paddingTop = 3;
            DebugFoldout.style.paddingLeft = 3;
            DebugFoldout.style.paddingRight = 6;
            DebugFoldout.style.paddingBottom = 3;
            DebugFoldout.style.marginBottom = 15;

            ShowTileCenterToggle = new Toggle("Show Tile Center");
            SerializedProperty ShowTileCenterProp = serializedObject.FindProperty("showTileCenterFlag");
            ShowTileCenterToggle.BindProperty(ShowTileCenterProp);
            ShowTileCenterToggle.AddToClassList("unity-base-field__aligned");

            ShowTileOutlineToggle = new Toggle("Show Tile Outline");
            SerializedProperty ShowTileOutlineProp = serializedObject.FindProperty("showTileOutlineFlag");
            ShowTileOutlineToggle.BindProperty(ShowTileOutlineProp);
            ShowTileOutlineToggle.AddToClassList("unity-base-field__aligned");

            ShowTileInfoTextToggle = new Toggle("Show Tile Info Text");
            SerializedProperty ShowTileInfoTextProp = serializedObject.FindProperty("showTileInfoTextFlag");
            ShowTileInfoTextToggle.BindProperty(ShowTileInfoTextProp);
            ShowTileInfoTextToggle.AddToClassList("unity-base-field__aligned");
            ShowTileInfoTextToggle.RegisterValueChangedCallback(OnShowTileInfoTextValueChanged);

            ShowNodeGridCoordinatesTextToggle = new Toggle("Grid Coordinates");
            SerializedProperty ShowNodeGridCoordinatesTextProp = serializedObject.FindProperty("ShowNodeGridCoordinatesTextFlag");
            ShowNodeGridCoordinatesTextToggle.BindProperty(ShowNodeGridCoordinatesTextProp);
            ShowNodeGridCoordinatesTextToggle.AddToClassList("unity-base-field__aligned");

            ShowNodeMovementCostTextToggle = new Toggle("Movement Cost");
            SerializedProperty ShowNodeMovementCostTextProp = serializedObject.FindProperty("ShowNodeMovementCostTextFlag");
            ShowNodeMovementCostTextToggle.BindProperty(ShowNodeMovementCostTextProp);
            ShowNodeMovementCostTextToggle.AddToClassList("unity-base-field__aligned");

            ShowNodeWalkableTextToggle = new Toggle("Walkable");
            SerializedProperty ShowNodeWalkableTextProp = serializedObject.FindProperty("ShowNodeWalkableTextFlag");
            ShowNodeWalkableTextToggle.BindProperty(ShowNodeWalkableTextProp);
            ShowNodeWalkableTextToggle.AddToClassList("unity-base-field__aligned");

            ShowOccupyingActorTextToggle = new Toggle("Actor");
            SerializedProperty ShowOccupyingActorTextProp = serializedObject.FindProperty("ShowOccupyingActorTextFlag");
            ShowOccupyingActorTextToggle.BindProperty(ShowOccupyingActorTextProp);
            ShowOccupyingActorTextToggle.AddToClassList("unity-base-field__aligned");

            TileInfoTextSizeSlider = new SliderInt("Font Size", 8, 20);
            SerializedProperty TileInfoTextSizeProp = serializedObject.FindProperty("TileInfoTextFontSize");
            TileInfoTextSizeSlider.BindProperty(TileInfoTextSizeProp);
            TileInfoTextSizeSlider.AddToClassList("unity-base-field__aligned");

            TileInfoTextColorField = new ColorField("Text Color");
            SerializedProperty TileInfoTextColorProp = serializedObject.FindProperty("TileInfoTextColor");
            TileInfoTextColorField.BindProperty(TileInfoTextColorProp);
            TileInfoTextColorField.AddToClassList("unity-base-field__aligned");

            TextInfoDetailsBox = new Foldout();
            TextInfoDetailsBox.text = "Info Text Details";
            TextInfoDetailsBox.Add(TileInfoTextSizeSlider);
            TextInfoDetailsBox.Add(TileInfoTextColorField);
            TextInfoDetailsBox.Add(ShowNodeGridCoordinatesTextToggle);
            TextInfoDetailsBox.Add(ShowNodeMovementCostTextToggle);
            TextInfoDetailsBox.Add(ShowNodeWalkableTextToggle);
            TextInfoDetailsBox.Add(ShowOccupyingActorTextToggle);

            DebugFoldout.Add(ShowTileCenterToggle);
            DebugFoldout.Add(ShowTileOutlineToggle);
            DebugFoldout.Add(ShowTileInfoTextToggle);
            DebugFoldout.Add(TextInfoDetailsBox);

            return DebugFoldout;
        }


        private Box CreateActualMapDataSection()
        {
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

            // VisualElement ActorInfoElement = CreateActorInfoElement();

            ActualMapDataBox.Add(HeightField);
            ActualMapDataBox.Add(WidthField);
            ActualMapDataBox.Add(TileSizeField);
            // ActualMapDataBox.Add(ActorInfoElement);

            return ActualMapDataBox;
        }

        private VisualElement CreateActorInfoElement()
        {
            Foldout root = new Foldout();
            root.style.paddingLeft = 12;

            SerializedProperty actorsDictionaryProperty = serializedObject.FindProperty("actors");
            SerializedProperty keysProp = actorsDictionaryProperty.FindPropertyRelative("keys");
            SerializedProperty valuesProp = actorsDictionaryProperty.FindPropertyRelative("values");

            NavGrid grid = target as NavGrid;

            if (keysProp.arraySize == 0)
            {
                root.text = "No Actors";
            }
            else
            {
                root.text = "Total Actors on Grid = " + keysProp.arraySize;
            }
            for (int i = 0; i < keysProp.arraySize; i++)
            {
                VisualElement row = new();
                row.style.flexDirection = FlexDirection.Row;

                Label numberLabel = new Label($"{i}");
                numberLabel.style.minWidth = 22;
                row.Add(numberLabel);

                Label coordinatesLabel = new Label($"{grid.GridCoordinatesAt(keysProp.GetArrayElementAtIndex(i).intValue)}");
                coordinatesLabel.style.minWidth = 70;
                row.Add(coordinatesLabel);

                UnityEngine.Object obj = valuesProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (obj != null)
                {
                    row.Add(new Label($"{obj.name}"));
                }
                else
                {
                    row.Add(new Label("NULL"));
                }

                root.Add(row);
            }

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
            NavGrid grid = target as NavGrid;

            grid.CreateMap(WidthBakeField.value, HeightBakeField.value, TileSizeBakeField.value, NotWalkableLayerMaskField.value, CollisionLayerField.value, ColliderSizeField.value, RayLengthField.value);

            if (ScanActorsToggle.value == true)
            {
                grid.ScanForActors(RayLengthField.value);
            }
            EditorUtility.SetDirty(grid);
        }


        private void OnScnaActorsButtonClicked()
        {
            NavGrid grid = target as NavGrid;
            grid.UninstallAllActors();
            grid.ScanForActors(RayLengthField.value);
            EditorUtility.SetDirty(grid);
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


        private void OnShowTileInfoTextValueChanged(ChangeEvent<bool> evt)
        {
            TileInfoTextSizeSlider.SetEnabled(evt.newValue);
            TileInfoTextColorField.SetEnabled(evt.newValue);
            ShowNodeGridCoordinatesTextToggle.SetEnabled(evt.newValue);
            ShowNodeWalkableTextToggle.SetEnabled(evt.newValue);
            ShowNodeMovementCostTextToggle.SetEnabled(evt.newValue);
            ShowOccupyingActorTextToggle.SetEnabled(evt.newValue);
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