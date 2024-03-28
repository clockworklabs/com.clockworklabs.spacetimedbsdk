using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using static SpacetimeDB.Editor.ReducerMeta;

namespace SpacetimeDB.Editor
{
    /// Binds style and click events to the SpacetimeDB Reducer Window
    /// Note the dynamic init sequence logic @ initDynamicEventsFromReducerWindow()
    /// (!) This window assumes that SpacetimeDB tool is already installed.
    /// (!) This window has view-only access to servers/identities.
    /// (!) Errors involving cli installation, identity and servers will refer to Publisher window.
    public partial class ReducerWindow : EditorWindow
    {
        #region Window State
        private EntityStructure _entityStructure; // For reducersTreeView, set @ setReducersTreeViewAsync()
        #endregion // Window State
        
        
        #region UI Visual Elements
        // ##################################################################
        // Use `camelCase` naming conventions to utilize nameof and match UI.
        // This is due to implicit bindings, making it less error-prone.
        // ##################################################################
        private Button topBannerBtn;

        private TextField serverNameTxt;
        private TextField identityNameTxt;
        private TextField moduleNameTxt;

        private Button refreshReducersBtn;
        private TreeView reducersTreeView;
        
        private Foldout actionsFoldout;
        private TextField actionArgsTxt;
        private Label actionsSyntaxHintLabel;
        private TextField actionCallAsIdentityTxt;
        private Button actionsCallReducerBtn;

        private Foldout actionsResultFoldout;
        private Label actionsResultLabel;

        private VisualElement errorCover;
        private Label errorCoverLabel;
        #endregion // UI Visual Elements
        
        
        #region Init
        /// Show the Reducer window via top Menu item
        [MenuItem("Window/SpacetimeDB/Reducer #&d")] // (SHIFT+ALT+D) - not "R" because that's Nvidia's overlay default
        public static void ShowReducerWindow()
        {
            ReducerWindow window = GetWindow<ReducerWindow>();
            window.titleContent = new GUIContent("Reducer");
        }

        /// Add style to the UI window; subscribe to click actions.
        /// High-level event chain handler.
        /// (!) Persistent vals loaded from a ViewDataKey prop will NOT
        ///     load immediately here; await them elsewhere.
        public async void CreateGUI()
        {
            // Init styles, bind fields to ui, validate integrity
            initVisualTreeStyles();
            setUiElements();
            sanityCheckUiElements();

            // Reset the UI (since all UI shown in UI Builder), sub to click/interaction events
            resetUi(); // (!) ViewDataKey persistence loads sometime *after* CreateGUI().
            setOnActionEvents(); // @ ReducerWindowCallbacks.cs

            try
            {
                // Async init chain (pulling data from Publisher cache):
                // Ensure CLI is installed -> Load last-published server -> Load last-published identity ->
                // Get list of reducers -> Populate reducers list UI
                await initDynamicEventsFromReducerWindow(); // @ ReducerWindowActions
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }
        }

        private void initVisualTreeStyles()
        {
            // Load visual elements and stylesheets
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PathToUxml);
            StyleSheet commonStyles = AssetDatabase.LoadAssetAtPath<StyleSheet>(SpacetimeMeta.PathToCommonUss);
            StyleSheet reducerStyles = AssetDatabase.LoadAssetAtPath<StyleSheet>(PathToUss);
            
            // Sanity check, before applying styles (since these are all loaded via implicit paths)
            // Ensure all elements and styles were found
            Assert.IsNotNull(visualTree, "Failed to load ReducerWindow: " +
                $"Expected {nameof(visualTree)} (UXML) to be at: {PathToUxml}");
            
            Assert.IsNotNull(commonStyles, "Failed to load ReducerWindow: " +
                $"Expected {nameof(commonStyles)} (USS) to be at: '{SpacetimeMeta.PathToCommonUss}'");
            
            Assert.IsNotNull(reducerStyles, "Failed to load ReducerWindow: " +
                $"Expected {nameof(reducerStyles)} (USS) to be at: '{PathToUss}'");
            
            // Clone the visual tree (UXML)
            visualTree.CloneTree(rootVisualElement);
            
            // apply style (USS)
            rootVisualElement.styleSheets.Add(commonStyles);
            rootVisualElement.styleSheets.Add(reducerStyles);
        }

        /// All VisualElement field names should match their #newIdentity in camelCase
        private void setUiElements()
        {
            topBannerBtn = rootVisualElement.Q<Button>(nameof(topBannerBtn));
            
            serverNameTxt = rootVisualElement.Q<TextField>(nameof(serverNameTxt));
            identityNameTxt = rootVisualElement.Q<TextField>(nameof(identityNameTxt));
            moduleNameTxt = rootVisualElement.Q<TextField>(nameof(moduleNameTxt));
            
            refreshReducersBtn = rootVisualElement.Q<Button>(nameof(refreshReducersBtn));
            reducersTreeView = rootVisualElement.Q<TreeView>(nameof(reducersTreeView));
            
            actionsFoldout = rootVisualElement.Q<Foldout>(nameof(actionsFoldout));
            actionArgsTxt = rootVisualElement.Q<TextField>(nameof(actionArgsTxt));
            actionsSyntaxHintLabel = rootVisualElement.Q<Label>(nameof(actionsSyntaxHintLabel));
            actionCallAsIdentityTxt = rootVisualElement.Q<TextField>(nameof(actionCallAsIdentityTxt));
            actionsCallReducerBtn = rootVisualElement.Q<Button>(nameof(actionsCallReducerBtn));
            
            actionsResultFoldout = rootVisualElement.Q<Foldout>(nameof(actionsResultFoldout));
            actionsResultLabel = rootVisualElement.Q<Label>(nameof(actionsResultLabel));
            
            errorCover = rootVisualElement.Q<VisualElement>(nameof(errorCover));
            errorCoverLabel = rootVisualElement.Q<Label>(nameof(errorCoverLabel));
        }

        /// Changing implicit names can easily cause unexpected nulls
        /// All VisualElement field names should match their #newIdentity in camelCase
        private void sanityCheckUiElements()
        {
            try
            {
                Assert.IsNotNull(topBannerBtn, $"Expected `#{nameof(topBannerBtn)}`");
                
                Assert.IsNotNull(serverNameTxt, $"Expected `#{nameof(serverNameTxt)}`");
                Assert.IsNotNull(identityNameTxt, $"Expected `#{nameof(identityNameTxt)}`");
                Assert.IsNotNull(moduleNameTxt, $"Expected `#{nameof(moduleNameTxt)}`");
                
                Assert.IsNotNull(refreshReducersBtn, $"Expected `#{nameof(refreshReducersBtn)}`");
                Assert.IsNotNull(reducersTreeView, $"Expected `#{nameof(reducersTreeView)}`");
                
                Assert.IsNotNull(actionsFoldout, $"Expected `#{nameof(actionsFoldout)}`");
                Assert.IsNotNull(actionArgsTxt, $"Expected `#{nameof(actionArgsTxt)}`");
                Assert.IsNotNull(actionsSyntaxHintLabel, $"Expected `#{nameof(actionsSyntaxHintLabel)}`");
                Assert.IsNotNull(actionCallAsIdentityTxt, $"Expected `#{nameof(actionCallAsIdentityTxt)}`");
                Assert.IsNotNull(actionsCallReducerBtn, $"Expected `#{nameof(actionsCallReducerBtn)}`");
                
                Assert.IsNotNull(actionsResultFoldout, $"Expected `#{nameof(actionsResultFoldout)}`");
                Assert.IsNotNull(actionsResultLabel, $"Expected `#{nameof(actionsResultLabel)}`");
                
                Assert.IsNotNull(errorCover, $"Expected `#{nameof(errorCover)}`");
                Assert.IsNotNull(errorCoverLabel, $"Expected `#{nameof(errorCoverLabel)}`");
            }
            catch (Exception e)
            {
                // Show err cover
                errorCover = rootVisualElement.Q<VisualElement>(nameof(errorCover));
                if (errorCover != null)
                    errorCover.style.display = DisplayStyle.Flex;
                
                Debug.LogError($"Error: {e}");
                throw;
            }
        }
        #endregion // Init
    }
}