using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;

namespace ichortower.SNF;

public class API
{
    /*
     * Return a new instance of an object implementing the INoteData interface, which you
     * can use to register secret notes. You may choose instead to provide your own
     * implementation of the interface on your end, but using this may reduce friction.
     *   Example:
     *     INoteData obj = snfApi.CreateDataObject();
     *     obj.Contents = "My note for you";
     *     snfApi.RegisterSecretNote(uniqueId, obj);
     */
    public INoteData CreateDataObject()
    {
        return new SecretModNoteData();
    }

    /*
     * Register a note, just like you would do via Content Patcher or SMAPI's content API.
     * Like those, this should be called sometime on or after GameLaunched, but before the
     * player loads into a save (i.e. before the asset is requested from the content
     * pipeline). If you wait too long, you'll need to call Reload in order to see
     * your notes appear.
     *
     * Returns true (the note was registered successfully). Not known to fail/return false.
     */
    public bool RegisterSecretNote(string uniqueId, INoteData note)
    {
        Log.Info($"Received call to register {uniqueId} with data {note}");
        Log.Warn($"note.Contents: {note.Contents}");
        return true;
    }

    /*
     * Reload the data from the mod-provided asset and/or reread the note conditions and set
     * which notes are available to spawn.
     *   data: set to true to reload the `Mods/ichortower.SecretNoteFramework/Notes` asset.
     *   conditions: set to true to reevaluate each note's Conditions field.
     *
     * Returns true if anything was done (either argument was true), or false otherwise.
     *
     * Calling this method is equivalent to running the `snf_reload` console command.
     */
    public bool Reload(bool data = false, bool conditions = false)
    {
        bool ret = false;
        if (data) {
            SecretNoteFramework.instance.Helper.GameContent.InvalidateCache(
                    SecretModNotes.NotesAsset);
            _ = SecretModNotes.Data;
            Log.Info("Reloaded note data.");
            ret = true;
        }
        if (conditions) {
            SecretModNotes.RefreshAvailableNotes();
            Log.Info("Refreshed note conditions.");
            ret = true;
        }
        return ret;
    }

    public interface INoteData
    {
        public string Contents { get; set; }
        public string Title { get; set; }
        public string Conditions { get; set; }
        public string Location { get; set; }
        public string LocationContext { get; set; }
        public string ObjectId { get; set; }
        public string NoteTexture { get; set; }
        public int NoteTextureIndex { get; set; }
        public string NoteTextColor { get; set; }
        public string NoteImageTexture { get; set; }
        public int NoteImageTextureIndex { get; set; }
        public List<string> ActionsOnFirstRead { get; set; }
    }

}
