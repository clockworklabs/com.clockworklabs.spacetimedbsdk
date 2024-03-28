// using UnityEditor.IMGUI.Controls;
// using System.Collections.Generic;
//
// /// State controller for the ReducerWindow's `#reducersTreeView` // TODO: Possibly obsolete?
// public class ReducersTreeView : TreeView
// {
//     public ReducersTreeView(TreeViewState state) : base(state) =>
//         Reload();
//
//     protected override TreeViewItem BuildRoot()
//     {
//         // Root item with negative ID to indicate it's the root
//         TreeViewItem root = new()
//         {
//             id = -1, 
//             depth = -1, 
//             displayName = "Reducers",
//         };
//         
//         List<TreeViewItem> allItems = new();
//
//         // Assuming you have a list of names you want to add as items
//         List<string> names = new List<string> { "Item1", "Item2", "Item3" };
//         for (int i = 0; i < names.Count; i++)
//         {
//             allItems.Add(new TreeViewItem { id = i, depth = 0, displayName = names[i] });
//         }
//
//         // Utility method that sets up internal data structure
//         SetupParentsAndChildrenFromDepths(root, allItems);
//
//         return root;
//     }
// }
