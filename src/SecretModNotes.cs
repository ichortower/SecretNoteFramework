using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;

namespace ichortower.SNF
{
    internal class SecretModNotes
    {
        public static string NotesAsset = $"Mods/{SNF.ModId}/Notes";
        public static string DefaultObjectId = $"(O){SNF.ModId}_DefaultNote";

        public static HashSet<string> ActiveObjectIds = new();
        public static HashSet<string> AvailableNoteIds = new();

        // notes registered via API
        internal static Dictionary<string, SecretModNoteData> RegisteredNotes = new();

        private static Dictionary<string, SecretModNoteData> _data = null;
        public static Dictionary<string, SecretModNoteData> Data
        {
            get {
                if (_data is null) {
                    _data = Load(Game1.content);
                    ActivateObjects();
                }
                return _data;
            }
            set {
                _data = value;
                ActivateObjects();
            }
        }

        public static void RefreshAvailableNotes()
        {
            AvailableNoteIds.Clear();
            foreach (var entry in Data) {
                if (GameStateQuery.CheckConditions(entry.Value.Conditions)) {
                    AvailableNoteIds.Add(entry.Key);
                }
            }
        }

        public static Dictionary<string, SecretModNoteData> Load(
                LocalizedContentManager content)
        {
            var data = content.Load<Dictionary<string, SecretModNoteData>>(NotesAsset);
            foreach (var entry in data) {
                if (String.IsNullOrEmpty(entry.Value.ObjectId)) {
                    continue;
                }
                string qid = ItemRegistry.QualifyItemId(entry.Value.ObjectId);
                if (qid == null) {
                    Log.Warn($"In note entry '{entry.Key}': failed to qualify" +
                            $" item id '{entry.Value.ObjectId}'");
                    continue;
                }
                entry.Value.ObjectId = qid;
            }
            return data;
        }

        public static void ActivateObjects()
        {
            ActiveObjectIds.Clear();
            if (_data is null) {
                return;
            }
            foreach (var entry in _data) {
                if (String.IsNullOrEmpty(entry.Value.ObjectId)) {
                    ActiveObjectIds.Add(ItemRegistry.QualifyItemId(DefaultObjectId));
                    continue;
                }
                // these should already be qualified, per Load, but play it safe
                ActiveObjectIds.Add(ItemRegistry.QualifyItemId(entry.Value.ObjectId));
            }
        }

        public static void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo(NotesAsset)) {
                e.LoadFrom(() => new Dictionary<string, SecretModNoteData>(),
                        AssetLoadPriority.Exclusive);
                e.Edit(asset => {
                    var dict = asset.AsDictionary<string, SecretModNoteData>();
                    foreach (var entry in RegisteredNotes) {
                        dict.Data[entry.Key] = entry.Value;
                    }
                });
            }
            else if (e.Name.IsEquivalentTo("Data/Objects")) {
                var modAsset = SecretNoteFramework.instance.Helper.ModContent.Load
                        <Dictionary<string, ObjectData>>("assets/items.json");
                e.Edit(asset => {
                    var dict = asset.AsDictionary<string, ObjectData>();
                    foreach (var entry in modAsset) {
                        dict.Data[entry.Key] = entry.Value;
                    }
                });
            }
            else if (e.Name.IsEquivalentTo("Strings/Objects")) {
                e.Edit(asset => {
                    var dict = asset.AsDictionary<string, string>();
                    dict.Data[$"{SNF.ModId}_DefaultNote_Name"] =
                            TR.Get("Objects.DefaultNote.Name");
                    dict.Data[$"{SNF.ModId}_DefaultNote_Description"] =
                            TR.Get("Objects.DefaultNote.Description");
                });
            }
            else if (e.Name.IsEquivalentTo($"Mods/{SNF.ModId}/DefaultNoteTexture")) {
                e.LoadFrom(() => {
                    return SecretNoteFramework.instance.Helper.ModContent.Load
                            <Texture2D>("assets/icon_default.png");
                }, AssetLoadPriority.Medium);
            }
        }

        public static void OnAssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            foreach (var name in e.Names) {
                if (name.IsEquivalentTo(NotesAsset)) {
                    Log.Trace("Invalidating note data");
                    Data = null;
                }
            }
        }

    }

    public class SecretModNoteData : API.INoteData
    {
        public string Contents { get; set; } = "";
        public string Title { get; set; } = null;
        public string Conditions { get; set; } = null;
        public string Location { get; set; } = null;
        public string LocationContext { get; set; } = "!Island";
        public string ObjectId { get; set; } = null;
        public string NoteTexture { get; set; } = null;
        public int NoteTextureIndex { get; set; } = 0;
        public string NoteTextColor { get; set; } = null;
        public string NoteImageTexture { get; set; } = null;
        public int NoteImageTextureIndex { get; set; } = -1;
        public List<string> ActionsOnFirstRead { get; set; } = new();
    }

}
