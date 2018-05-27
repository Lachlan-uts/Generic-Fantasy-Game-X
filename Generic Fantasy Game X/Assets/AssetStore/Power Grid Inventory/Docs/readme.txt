Thank you for purchasing Power Grid Inventory.
Please read the Getting Started Guide, Manual, and Doxygen generated HTML documentation for more details on working with PGI.
If you have any questions, concerns, or suggestions, please feel free to contact me any time.

Contact: pgi-support@ancientcraftgames.com
Online Documentation: http://ancientcraftgames.com/pgi/docs/

Youtube Videos:
  Features:       https://www.youtube.com/watch?v=iPkzpzbvreM
  5 Minute Setup: https://www.youtube.com/watch?v=mtwW0lrCnIU
  
-----------------
 Future Features
-----------------
- Better built-in support for sockets and socketables.
- Slot Pooling to increase speed when resizing grids or creating grids.
- Reference slots (so that you can use PGI for hotkeys and macros)
- Support for click 'n drop.
- Support for gamepads and keyboards


-----------------
 Version History
-----------------
1.7 3/7/2017 - The Illiad Update
Note: The 'IgnoreUIRaycasts' behaviour has become obsolete but will remain in use for backward compatiblity with older versions of Unity.
Update Note: Be sure to remove any old PGI folders before updating as many files have been renamed, refactored, and re-purposed!

•Fixed an updated several Tutorial scenes. Also added a feature-rich demo scene.
•Added support for Unity 5.5 (and 5.6b too)
•PGIView now updates on regular intervals in order to account for the fact that the new asyncronous nature of Unity's UI. This requires extra processing overhead but ensures that it will always be properly updated.
•Fixed bug that caused grid slots to sometimes appear far away from the grid view in z-space.
•Fixed bug where equipment slots wouldn't update when a view was disabled and then enabled.
•Fixed a bug where many fields would be edited but fail to flag the scene as dirty.
•Fixed several subtle issues in PGIView and Auto-Square Slots that would cause Unity to crash when changing the size of their RectTransforms.
•If PGIView component is removed from a GameObject it now relenquishes all child slots.
•Made view updates in editor and at runtime a little more robust and reactive to changes.
•Added an additional folder called 'Toolbox'. This is to make my life easier when integrating PGI with other project. Provided many additional untility files not directly related to PGI but used by it.
•Many general-purpose utilities that were part of PGI have been moved to the Toolbox directory and had their namespaces changed to Toolbox. Some have been renamed to better reflect their use.
•Moved the serializer 'Pantagruel' into the toolbox directory. Make sure you back up any manifest files, delete the old Pantagruel directory, and then move the manifests into the new Pantagruel folder under Toolbox.
•Replaced PGIPointer utility class with a similar one from Toolbox namespace.
•Replaced PGI's slot pools with Toolbox.Lazurus for easier development and much improved feature set and performance. All scripts related to the old system have also been removed.
•Removed ability to supply a source pool for slots. Pooling is handled automatically now by Lazurus. Pools are also automatically shared by all views that use the same slot prefab.
•Slots that are created at edit time are no longer kept during runtime. They are deleted and re-created by the pooling system. This was due to several issues cropping up in Unity 5.2+ that would cause such objects to hang around and pile up after the editor exited playmode.
•Fixed several issues that could cause prefabs of views to fail during initialization or not properly be destroyed after exiting playmode.
•Removed slot batching aid. It appears to be largely uneccessary now and often causes more trouble than its worth. It can be re-enabled by defining PGI_SLOT_BATCHING_AID in the player settings.
•Pantagruel serializer no longer serializes delegates by default. If it is needed, you can always use AddTemporarySurrogate(new DelegateSurrogate()) just before serialization/deserialization.
•Pantagruel resource manifest build tool now skips all assets not located in a Resources directory. This can greatly decrease the time it takes to build a manifest in many projects.
•Pantagruel serializer can now be moved to any directory without needing to update the location in the Constants.cs file
•Pantagruel deserializer now has an optional third parameter that can be used to suppress loading resources. This will disable the ability to deserialize resources saved as path strings but will allow Panatgruel to deserialize on different threads and within OnAfterDeserialize().
•Added a progress bar with a cancel option in the resource manifest build tool.
•Added the value Constants.ManifestNameLength to Pantagruel. It can be used to adjust the number of manifest files generated which affects the final build size of a project. Please read the comment in the file Constants.cs for details.
•Removed editor skins system from tutorials.
•Changed several methods in PGIModel and PGIView to return List<> instead of arrays in order to make PGI more GC friendly.
•StackID is now called ItemTypeId. It is still used for stacking items but is now also used for model queries.
•Added several 'Query' methods to PGIModel for item searching and sorting
    Stack splitting, Stack Decomposing, Stack reducing, Get all items, Get all items of a given id, Get a specified number of items by id (useful for crafting systems)
•Banished many minor bugs that had been hidden deep within the darkest corners of PGIView and PGIModel.
•Updated documentation to match PGI 1.7

1.6 5/7/2016 - Pooling Party
• Added a Slot pooling mechanism. This can help decrease load times and make dynamically resizing large grids faster because new slots do not have to be instantiated as frequently. See the manual for details.
• Implemented XmlDeserializer.DeserializeComponent<T>().
• Added a 'Disable Rendering' toggle to the view that facilitates easy disabling/enabling of all slot rendering without deactivating the view.
• Equipment slots can now accept stacking items.
• Items with a stackId lower than '1' can no longer be stacked. This way the default value is 'do not stack'.
• Removed all Linq related code.
• Fixed a bug that caused certain important actions to be skipped when disabling a PGIView.
• Increased performance and ease of maintenance for PGIView by simplifying and re-organizing internal update events.
• Lowered the occurrance of repeated unnecessary grid resizing during certain events.
• Removed the FixedUpdate from PGISlot. This should help performance a little, especially for scenes that used very large numbers of PGISlots.
• Fixed several bugs in the stacking code, especially when stacking across different inventories.
• Fixed an object leak in the slot batching process. The leak would occur when re-sizing the grid to a smaller size where the batched sub-elements of the slots would remain when the root of the slot was deleted.
• Rearranged several files in the Pantagruel folder. There are also some stubs for the upcoming inventory database system, however it is still pre-alpha.
• Added OnDragBegin and OnDragEnd events to items.
• Added OnDragEndInvalid to PGIView. Take note that methods that were previously triggered by the OnSlotDragEnd event might not be called in all of the same circumstances now. They should be reformatted to use this new event as it provides much more useful data.
• Added a extension component called 'InventoryDropSpot'. It can be attached to any UI element and will move an item that is dropped onto it to the specified inventory.
• Added more tutorial scenes. Esepcially ones pretaining to event triggers and the use of several built-in extensions that use them.
  
1.5: 3/17/2016 - Save and Load the Last Dance for Me
• Added SaveModel and LoadModel methods to PGIModel. See the documentation and example scene for how to use them.
• Added a helper component 'SaveLoad' that can be more easily rigged to UnityEvents and used to save and load inventory models to and from disk.
• Added a new event: PGIModel.OnModelChanged - this event is used to let other objects know when a new model has been instantiated by calling LoadModel() so that they can change their internal reference from the old on to the new one.
• Added a user guide for the Pantagruel serializer and resource manifest builder system. It can be found under the Pantagruel subfolder. Please read it before attempting to use the serializer or the built-in save/load system.
• Added serialization support for multidimensional arrays.
• Fixed the 3D icons to work with 5.2+ version of Unity. Some preprocessor trickery should allow it to remain backwards compatible. However, the rotation code for item icons in general still depends on new versions of Unity. I'll try to make this backwards compatible in a future update.
• 2D sprite icons can now also be rotated with the 'PGISlotableItem.IconOrientation' property.
• The 'PGISlotItem.IconOrientation' property now works in real-time.
• PGISlot.DefaultIcon is now exposed. This is used to set the default icon rather than the Icon Image sub-object.
• Fixed aspect-ratio issue with drag icon when dragging sprites.
• Included some backend code for 'Pantagruel', an item database editor that will be added in the future.
• Added framework for an in-editor tutorial system.
• Changed the requirements of a Grid Slot prefab. If you see errors you may have to create a new slot prefab based off the default one.
• Updated the manual a bit to reflect new features including rotation, auto-arrange, save/load, and more. The intro has also been updated so please give it another read as it contains some important information regarding the new save/load system.
• Fixed a small bug in the TransformSurrogate deserializer that would cause objects to not be restored to their original position, scale, and rotation.
• Changed the way PGISlotItems get moved into their parent model when stored or socketed. They now retain their world position, scale, and rotation. This is to facilitate better serialization.
• Removed old example projects and replaced them with smaller simpler ones that show off individual elements of PGI.

1.4.2: 3/1/2016 - The Forgotten Update
• Added an Xml serializer under the namespace 'Pantagruel'. Still in beta.
• Added AutoSquareSlots Component. Just attach to a parent of a GameObject that has a PGIView and then supply the view reference. It will maintain size of the view's RectTransform to keep the slots square.
• PGIView now updates when it is enabled to reflect latest model state.
• PGIView now resizes slots when RectTransform changes rather than rebuilding all slots.
• If the model changes grid size in the editor the view will now immediately reflect that change.
• Improved View's ability to handle model reference changes. Null can now be supplied to clear a view entirely.

1.4.1: 12/16/2015 - Hot Fix
• Fixed a bug in the roation code that caused icons to not scale properly.

1.4.0: 11/11/2015 - Spin me 'Round Baby!
• Items can now be rotated using PGISlotItem.Rotate(PGISlotItem.RotationDirection). Rotation is in the form of three non-relative states. 'None', 'Clockwise', and 'Counter-Clockwise'. When rotating items in an inventory they may shift position slightly in order to attempt to fit into available spaces. *
• Added an auto-arrange method for models that can optionally support rotation of items. This is still very experimental and will likely give sub-optimal results in some cases. *
• Removed update methods from PGIView and PGISlot. This should boost performance, especially in the case of large grids.
• Greatly simplified the internal 'dirty updating' system in the model and how the view uses it. The model now triggers an event of the type 'public event DirtyEvent OnUpdateDirty' when something changes. This should greatly increase robustness, maintainablity, extensibility, and reduce bugs for future updates. It should also increase performance in the case of large inventory grids due to the fact that each slot no longer has its own update. The view might take a little extra time to update when the model changes since the whole thing is refreshed at once but this happens very infrequently so it should not be much of an issue.
• Fixed a bug that would occur when multiple 'Can..' event hooks where attached to the same event and the last one would override the 'CanPerformAction' flags set by all the previous ones. It now properly ensures that if the flag is set as false it stays that way and will not allow other events to reset it.
• Fixed an error in the slot batcher that would choke if any child elements of the slot did not have SlotBatch component attached.
• Added PGIView.UpdateViewStep() for forcing view to update.
• If a view's reference to a model is changed it now properly updates to reflect the state of the new model. **
• Model now properly supports having multiple active views attached to it at the same time. **

* These features are still very experimental and will often given results that are less than optimal. This will be an ongoing project to improve their performance and reliability.
** Currently equipment slots do not play well when switching models for a view and don't support multiple views at the same time period. The problem stems from the fact that they are attached directly to the model and bypass the view entirely. I may change this at some point in the future.


1.3.0: 9/3/2015 - Nitty Gritty, Kitty
FEATURES:
• Addition of a 3D mesh UI element similar to Unity's 'UnityEngine.UI.Image'. Its fully qualified name is 'AncientCraftGames.UI.Image3D'.
• 3D meshes now supported for icons.
• Items can now have their own individual highlight colors that the view will use for a slot's highlight when that item is stored.
• Addition of an array of MonoBehaviour references to PGISlotItem. This way you can simply cast these references for commonly and frequently access components rather than use GetComponent<>() inside hooked events.
• Added a toggle to PGIView entitled 'DiableWorldDropping' than can be used to disable users from dropping items from the inventory by dragging them into an empty region.
• Added horizontal and vertical ordering settings to PGIView. These allow the view's grid to start at different corners when displayed. Note that internally the model's grid does not change, only the way the view interprets it.

UPDATES & FIXES:
• ***breaking change*** All classes are now under a namespace. The root namespaces are 'PowerGridInventory' and 'AncientCraftGames'.
• PGIView's 'Batch Equipment' and 'Batch Grid' have now been combined into a single 'Batch Slots' toggle.
• Slot elements that are unused or completely transparent are now disabled to help improve rendering performance.
• Batching works much better overall, even when PGIView.BatchSlots is disabled.
• CanSwap...() events are now only triggered a single time when the mouse first drags over an equipment slot rather than every time the mouse changes position whiled dragging.
• Mobile devices now no longer count it as a click when a user releases the mouse after hovering over a slot for more than 3/4 of a second. This should help normalize touch and mouse input more.
• Removal of the 'DragIcon' prefab for views. It is now implicitly creates it at runtime when the first PGIView becomes active and shares that icon between all views from that point on.
• Lots and lots of null-reference excpetions fixed.
• Spruced up the inspector views a bit to make them more readable and user-friendly.
• Changed a few minor public variable names. They all use [PreviouslySerializedAs] to ensure data isn't lost during update.
• When a view is disabled during a drag operation the drag is now canceled properly.
• When using 'Batch Slots' for PGIView, the z-depth for UI elements is now properly maintained.
• Fixed a bug in 'Socketed.EmptySockets' that would return the incorrect number.
• Added 'PGIModel.HasRoomForItem' method.


1.2.0: 8/6/2015 - Socket to Me!
- FEATURE: Socketed and Socketable Items components.
- UPDATE: Added a system to re-arrange slots in order to greatly reduce draw calls at runtime (when in play mode). This should improve rendering performance in many situations.
- FIX: Removed an editor only import from PGIModel and added Conditional-compilation tag to PGIView that allow stand-alone builds to compile properly.
- FIX: Fixed null reference exception in PGIModel.
- FIX: Removed several leftover debug logs in PGIModel.


1.1.0: 6/25/2015 - Cleanup Update
- FEATURE: PGIModel can now be set to automatically detect items that were added to, or removed from, its transform hierarchy.
- FEATURE: Model grid can now change rows and columns at runtime. Items will be dropped from inventory if this happens.
- FEATURE: Provided a 'PGIModel.Drop()' convenience method that automatically triggers the necessary events for unequipping and removing from inventory.
- CHANGE: Breaking Change: Moved many files from the 'Examples' folder to the 'Extensions' folder. Also changed some example component names.
- CHANGE: 'Inventory Item' container extension now uses double-clicks rather than right-clicks to open its PGIView.
- CHANGE: Added links to online documentation to Asset Store and readme file.
- CHANGE: Updated manuals to reflect new features. Expanded the user manual's explaination of the major components. Shortened the Getting Started guide.
- CHANGE: Changed e-mail support address that is given in documentation. Now points to pgi-support@ancientcraftgames.com
- FIX: Normalized input for mouse and touchscreen. They now work identically.
- FIX: Properly initialized the UnityEvent objects in PGIModel, PGIView, PGISlot, and PGISlotItem so they don't throw null exceptions when added to a GameObject at runtime.
- FIX: Fixed errors that pooped up when a model had no equipment slots but were being queried.
- FIX: Corrected drop-down menu titles for 'Linked Equip Slot' and 'Simple Pickup'.
- FIX: Inventory grid will now properly update its view in the editor when its parent's RectTransform is resized.
- FIX: Various spelling errors in Asset Store, User Manual, and Getting Started Guide.


1.0.0: 5/25/2015 - Introducing...
- NEW: First release!

