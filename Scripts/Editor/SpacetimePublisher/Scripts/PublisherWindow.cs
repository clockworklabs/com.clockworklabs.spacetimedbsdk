using System;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using static SpacetimeDB.Editor.PublisherMeta;

namespace SpacetimeDB.Editor
{
    /// Binds style and click events to the Spacetime Publisher Window
    /// Note the dynamic init sequence logic @ initDynamicEventsFromPublisherWindow()
    public partial class PublisherWindow : EditorWindow
    {
        #region Operational State Vars
        /// <summary>
        /// Since we have FocusOut events, this will sometimes trigger
        /// awkwardly if you jump from input to a file picker button
        /// </summary>
        private bool _isFilePicking;

        /// There's a known bug where default servers get wiped with no apparent pattern.
        /// We'll try once to add them back in.
        private bool _isRegeneratingDefaultServers;

        private CancellationTokenSource _cts;
        
        /// This will be null on init (window may still load limited cache)
        /// We'll prioritize this over the path, in case it was changed post-publish
        private PublishResult _cachedPublishResult;
        #endregion // Operational State Vars
        

        #region UI Visual Elements
        private Button topBannerBtn;
        
        private GroupBox installCliGroupBox;
        private ProgressBar installCliProgressBar;
        private Label installCliStatusLabel;

        private Foldout serverFoldout;
        private DropdownField serverSelectedDropdown; // Don't set ViewDataKey; we'll set the default set in CLI
        private Button serverAddNewShowUiBtn;
        private GroupBox serverNewGroupBox;
        private TextField serverNicknameTxt;
        private TextField serverHostTxt;
        private Button serverAddBtn;
        private Label serverStatusLabel;

        private Foldout identityFoldout;
        private DropdownField identitySelectedDropdown; // Don't set ViewDataKey; we'll set the default set in CLI
        private Button identityAddNewShowUiBtn;
        private GroupBox identityNewGroupBox;
        private TextField identityNicknameTxt;
        private TextField identityEmailTxt;
        private Button identityAddBtn;
        private Label identityStatusLabel;

        private Foldout publishFoldout;
        private GroupBox publishPathGroupBox;
        private Button publishPathSetDirectoryBtn; // "Browse"
        private TextField publishModulePathTxt;
        
        private TextField publishModuleNameTxt; // Always has a val (fallback system)

        private GroupBox publishGroupBox;
        private Toggle publishModuleClearDataToggle;
        private Toggle publishModuleDebugModeToggle;
        private Button publishBtn;
        private Button publishCancelBtn;
        private ProgressBar publishInstallProgressBar;
        private Label publishStatusLabel;

        private TextField publishResultDateTimeTxt; // readonly
        private Foldout publishResultFoldout;
        private TextField publishResultHostTxt; // readonly
        private TextField publishResultDbAddressTxt; // readonly
        private Toggle publishResultIsOptimizedBuildToggle; // Set readonly via hacky workaround (SetEnabled @ ResetUi)
        private Button installWasmOptBtn; // Only shows after a publishAsync where wasm-opt was !found
        private ProgressBar installWasmOptProgressBar; // Shows after installWasmOptBtn clicked
        private Button publishResultGenerateClientFilesBtn;
        private Label publishResultStatusLabel;
        
        private VisualElement errorCover;
        #endregion // UI Visual Elements
        
        
        #region Init
        /// Show the publisher window via top Menu item
        [MenuItem("Window/SpacetimeDB/Publisher #&p")] // (SHIFT+ALT+P)
        public static void ShowPublisherWindow()
        {
            PublisherWindow window = GetWindow<PublisherWindow>();
            window.titleContent = new GUIContent("Publisher");
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
            setOnActionEvents(); // @ PublisherWindowCallbacks.cs

            try
            {
                // Async init chain: Ensure CLI is installed -> Load default servers ->
                // Load default identities -> Load cached publish result, if any
                await initDynamicEventsFromPublisherWindow(); // @ PublisherWindowActions
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
            StyleSheet publisherStyles = AssetDatabase.LoadAssetAtPath<StyleSheet>(PathToUss);
            
            // Sanity check, before applying styles (since these are all loaded via implicit paths)
            // Ensure all elements and styles were found
            Assert.IsNotNull(visualTree, "Failed to load PublisherWindow: " +
                $"Expected {nameof(visualTree)} (UXML) to be at: {PathToUxml}");
            
            Assert.IsNotNull(commonStyles, "Failed to load PublisherWindow: " +
                $"Expected {nameof(commonStyles)} (USS) to be at: '{SpacetimeMeta.PathToCommonUss}'");
            
            Assert.IsNotNull(publisherStyles, "Failed to load PublisherWindow: " +
                $"Expected {nameof(publisherStyles)} (USS) to be at: '{PathToUss}'");
            
            // Clone the visual tree (UXML)
            visualTree.CloneTree(rootVisualElement);
            
            // apply style (USS)
            rootVisualElement.styleSheets.Add(commonStyles);
            rootVisualElement.styleSheets.Add(publisherStyles);
        }

        /// All VisualElement field names should match their #newIdentity in camelCase
        private void setUiElements()
        {
            topBannerBtn = rootVisualElement.Q<Button>(nameof(topBannerBtn));
            errorCover = rootVisualElement.Q<VisualElement>(nameof(errorCover));

            installCliGroupBox = rootVisualElement.Q<GroupBox>(nameof(installCliGroupBox));
            installCliProgressBar = rootVisualElement.Q<ProgressBar>(nameof(installCliProgressBar));
            installCliStatusLabel = rootVisualElement.Q<Label>(nameof(installCliStatusLabel));
            
            serverFoldout = rootVisualElement.Q<Foldout>(nameof(serverFoldout));
            serverSelectedDropdown = rootVisualElement.Q<DropdownField>(nameof(serverSelectedDropdown));
            serverAddNewShowUiBtn = rootVisualElement.Q<Button>(nameof(serverAddNewShowUiBtn));
            serverNewGroupBox = rootVisualElement.Q<GroupBox>(nameof(serverNewGroupBox));
            serverNicknameTxt = rootVisualElement.Q<TextField>(nameof(serverNicknameTxt));
            serverHostTxt = rootVisualElement.Q<TextField>(nameof(serverHostTxt));
            serverAddBtn = rootVisualElement.Q<Button>(nameof(serverAddBtn));
            serverStatusLabel = rootVisualElement.Q<Label>(nameof(serverStatusLabel));
            
            identityFoldout = rootVisualElement.Q<Foldout>(nameof(identityFoldout));
            identitySelectedDropdown = rootVisualElement.Q<DropdownField>(nameof(identitySelectedDropdown));
            identityAddNewShowUiBtn = rootVisualElement.Q<Button>(nameof(identityAddNewShowUiBtn));
            identityNewGroupBox = rootVisualElement.Q<GroupBox>(nameof(identityNewGroupBox));
            identityNicknameTxt = rootVisualElement.Q<TextField>(nameof(identityNicknameTxt));
            identityEmailTxt = rootVisualElement.Q<TextField>(nameof(identityEmailTxt));
            identityAddBtn = rootVisualElement.Q<Button>(nameof(identityAddBtn));
            identityStatusLabel = rootVisualElement.Q<Label>(nameof(identityStatusLabel));
            
            publishFoldout = rootVisualElement.Q<Foldout>(nameof(publishFoldout));
            publishModuleNameTxt = rootVisualElement.Q<TextField>(nameof(publishModuleNameTxt));
            publishPathGroupBox = rootVisualElement.Q<GroupBox>(nameof(publishPathGroupBox));
            publishPathSetDirectoryBtn = rootVisualElement.Q<Button>(nameof(publishPathSetDirectoryBtn));
            publishModulePathTxt = rootVisualElement.Q<TextField>(nameof(publishModulePathTxt));

            publishGroupBox = rootVisualElement.Q<GroupBox>(nameof(publishGroupBox));
            publishModuleClearDataToggle = rootVisualElement.Q<Toggle>(nameof(publishModuleClearDataToggle));
            publishModuleDebugModeToggle = rootVisualElement.Q<Toggle>(nameof(publishModuleDebugModeToggle));
            publishBtn = rootVisualElement.Q<Button>(nameof(publishBtn));
            publishCancelBtn = rootVisualElement.Q<Button>(nameof(publishCancelBtn));
            publishInstallProgressBar = rootVisualElement.Q<ProgressBar>(nameof(publishInstallProgressBar));
            publishStatusLabel = rootVisualElement.Q<Label>(nameof(publishStatusLabel));

            publishResultDateTimeTxt = rootVisualElement.Q<TextField>(nameof(publishResultDateTimeTxt));
            publishResultFoldout = rootVisualElement.Q<Foldout>(nameof(publishResultFoldout));
            publishResultHostTxt = rootVisualElement.Q<TextField>(nameof(publishResultHostTxt));
            publishResultDbAddressTxt = rootVisualElement.Q<TextField>(nameof(publishResultDbAddressTxt));
            publishResultIsOptimizedBuildToggle = rootVisualElement.Q<Toggle>(nameof(publishResultIsOptimizedBuildToggle));
            installWasmOptBtn = rootVisualElement.Q<Button>(nameof(installWasmOptBtn));
            installWasmOptProgressBar = rootVisualElement.Q<ProgressBar>(nameof(installWasmOptProgressBar));
            publishResultGenerateClientFilesBtn = rootVisualElement.Q<Button>(nameof(publishResultGenerateClientFilesBtn));
            publishResultStatusLabel = rootVisualElement.Q<Label>(nameof(publishResultStatusLabel));
        }

        /// Changing implicit names can easily cause unexpected nulls
        /// All VisualElement field names should match their #newIdentity in camelCase
        private void sanityCheckUiElements()
        {
            try
            {
                Assert.IsNotNull(topBannerBtn, $"Expected `#{nameof(topBannerBtn)}`");
                Assert.IsNotNull(errorCover, $"Expected `#{nameof(errorCover)}`");

                Assert.IsNotNull(installCliGroupBox, $"Expected `#{nameof(installCliGroupBox)}`");
                Assert.IsNotNull(installCliProgressBar, $"Expected `#{nameof(installCliProgressBar)}`");
                Assert.IsNotNull(installCliStatusLabel, $"Expected `#{nameof(installCliStatusLabel)}`");
                
                Assert.IsNotNull(serverFoldout, $"Expected `#{nameof(serverFoldout)}`");
                Assert.IsNotNull(serverSelectedDropdown, $"Expected `#{nameof(serverSelectedDropdown)}`");
                Assert.IsNotNull(serverAddNewShowUiBtn, $"Expected `#{nameof(serverAddNewShowUiBtn)}`");
                Assert.IsNotNull(serverNewGroupBox, $"Expected `#{nameof(serverNewGroupBox)}`");
                Assert.IsNotNull(serverNicknameTxt, $"Expected `#{nameof(serverNicknameTxt)}`");
                Assert.IsNotNull(serverHostTxt, $"Expected `#{nameof(serverHostTxt)}`");
                
                Assert.IsNotNull(identityFoldout, $"Expected `#{nameof(identityFoldout)}`");
                Assert.IsNotNull(identitySelectedDropdown, $"Expected `#{nameof(identitySelectedDropdown)}`");
                Assert.IsNotNull(identityAddNewShowUiBtn, $"Expected `#{nameof(identityAddNewShowUiBtn)}`");
                Assert.IsNotNull(identityNewGroupBox, $"Expected `#{nameof(identityNewGroupBox)}`");
                Assert.IsNotNull(identityNicknameTxt, $"Expected `#{nameof(identityNicknameTxt)}`");
                Assert.IsNotNull(identityEmailTxt, $"Expected `#{nameof(identityEmailTxt)}`");
                Assert.IsNotNull(identityAddBtn, $"Expected `#{nameof(identityAddBtn)}`");
                Assert.IsNotNull(identityStatusLabel, $"Expected `#{nameof(identityStatusLabel)}`");
                
                Assert.IsNotNull(publishFoldout, $"Expected `#{nameof(publishFoldout)}`");
                Assert.IsNotNull(publishModuleNameTxt, $"Expected `#{nameof(publishModuleNameTxt)}`");
                Assert.IsNotNull(publishPathGroupBox, $"Expected `#{nameof(publishPathGroupBox)}`");
                Assert.IsNotNull(publishPathSetDirectoryBtn, $"Expected `#{nameof(publishPathSetDirectoryBtn)}`");
                Assert.IsNotNull(publishModulePathTxt, $"Expected `#{nameof(publishModulePathTxt)}`");
                
                Assert.IsNotNull(publishModuleNameTxt, $"Expected `#{nameof(publishModuleNameTxt)}`");
                
                Assert.IsNotNull(publishGroupBox, $"Expected `#{nameof(publishGroupBox)}`");
                Assert.IsNotNull(publishModuleClearDataToggle, $"Expected `#{nameof(publishModuleClearDataToggle)}`");
                Assert.IsNotNull(publishModuleDebugModeToggle, $"Expected `#{nameof(publishModuleDebugModeToggle)}`");
                Assert.IsNotNull(publishBtn, $"Expected `#{nameof(publishBtn)}`");
                Assert.IsNotNull(publishCancelBtn, $"Expected `#{nameof(publishCancelBtn)}`");
                Assert.IsNotNull(publishInstallProgressBar, $"Expected `#{nameof(publishInstallProgressBar)}`");
                Assert.IsNotNull(publishStatusLabel, $"Expected `#{nameof(publishStatusLabel)}`");

                Assert.IsNotNull(publishResultDateTimeTxt, $"Expected `#{nameof(publishResultDateTimeTxt)}`");
                Assert.IsNotNull(publishResultFoldout, $"Expected `#{nameof(publishResultFoldout)}`");
                Assert.IsNotNull(publishResultHostTxt, $"Expected `#{nameof(publishResultHostTxt)}`");
                Assert.IsNotNull(publishResultDbAddressTxt, $"Expected `#{nameof(publishResultDbAddressTxt)}`");
                Assert.IsNotNull(publishResultIsOptimizedBuildToggle, $"Expected `#{nameof(publishResultIsOptimizedBuildToggle)}`");
                Assert.IsNotNull(installWasmOptBtn, $"Expected `#{nameof(installWasmOptBtn)}`");
                Assert.IsNotNull(installWasmOptProgressBar, $"Expected `#{nameof(installWasmOptProgressBar)}`");
                Assert.IsNotNull(publishResultGenerateClientFilesBtn, $"Expected `#{nameof(publishResultGenerateClientFilesBtn)}`");
                Assert.IsNotNull(publishResultStatusLabel, $"Expected `#{nameof(publishResultStatusLabel)}`");
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