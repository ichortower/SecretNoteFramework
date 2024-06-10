# Secret Note Framework - Author Guide

This document explains how to use this mod to add your custom Secret Notes to
Stardew Valley.

## Contents

* [Introduction](#introduction)
* [Adding Notes](#adding-notes)
  * [Content Patcher example](#content-patcher-example)
  * [Image Notes](#image-notes)
  * [Custom Items](#custom-items)
  * [Querying Notes](#querying-notes)
  * [Marking Notes as Seen](#marking-notes-as-seen)
* [How Modded Notes Work](#how-modded-notes-work)
  * [Spawning](#spawning)
  * [Saving](#saving)
* [Debugging](#debugging)


## Introduction

Secret Note Framework interacts with other mods by providing a data asset for
them to edit. At this time, other mods are expected to use Content Patcher or
SMAPI's Content API to perform their edits; in the future, I may provide
content pack support and/or a C# API, but those are not supported yet.

In addition to providing a way to add secret notes without fear of conflicts
(or of running out of space on the collections page), you can also do a few
advanced things, like:

* specify complex eligibility conditions on a note-by-note basis with game
  state queries
* determine which location context(s) your notes appear in, so you can (e.g.)
  restrict your mod's notes to spawn only in your mod's area
* use different (custom) items to represent different sets of notes, as
  desired
* specify your own assets for note content and formatting, including image
  notes
* set any number of trigger actions to be run when a note is first read

Let's begin!


## Adding Notes

The main goal of the framework is to add notes, so here's what that looks like.
The asset to target with your edits is:

```
Mods/ichortower.SecretNoteFramework/Notes
```

The asset is a `string->object` dictionary. The `string` keys are note IDs, and
should be unique string IDs, like most 1.6-era data items. The model (object)
has the following fields:

<table>

<tr>
<th>Field</th>
<th>Type</th>
<th>Purpose</th>
</tr>

<tr>
<td><code>Contents</code></td>
<td>string</td>
<td>
  
The text content of your note. Obviously, it is required for text notes.
Formatting is the same as vanilla mail and secret notes: `^` is used for line
breaks, and `@` will be replaced with the player's name. You can also use the
[special mail format
codes](https://stardewvalleywiki.com/Modding:Mail_data#Custom_mail_formatting)
`[letterbg]` and `[textcolor]`, but it is preferred to use the other fields for
that instead (the fields will supersede the in-band format codes).

*Default:* `""`

</td>
</tr>

<tr>
<td><code>Title</code></td>
<td>string</td>
<td>

The note's title, which will be displayed on the hover tooltip in the
collections menu. Vanilla secret notes generate a title from their integer id
(e.g.  "Secret Note #15"), but you can specify whatever you would like. If not
specified, the tooltip will display `???`.

*Default:* `null`

</td>
</tr>

<tr>
<td><code>Conditions</code></td>
<td>string</td>
<td>

A [game state query](https://stardewvalleywiki.com/Modding:Game_state_queries)
specifying the conditions for this note to be available to spawn. If null or
empty, the note will be available without restriction, although the player will
still need the Magnifying Glass in order to find secret notes in the first
place.

Conditions are evaluated at the start of each game day, so after fulfilling the
conditions for a note to be available, the player will need to sleep a day in
order for the note to be able to appear.

This mod adds [a query](#querying-notes) for checking whether a modded secret
note has been seen by a player, which is useful for ordering your notes.

*Default:* `null`

</td>
</tr>

<tr>
<td><code>LocationContext</code></td>
<td>string</td>
<td>

This string specifies one or more location contexts where the note is able to
appear. Vanilla journal scraps spawn only in the `Island` context (i.e. on
Ginger Island); secret notes spawn anywhere else. You can specify any value(s)
here, including modded contexts, so for example if a mod adds a mountain area
with a separate context, you can define notes which spawn only there.

Specify any number of context names, separated by commas (e.g. `Default,
Desert`), to allow a note to spawn in any of them. Alternately, you can specify
one context name preceded by a `!` to specify any location *except* that one:
the default value is `!Island`, mimicking vanilla's secret notes.

*Default:* `!Island`

</td>
</tr>

<tr>
<td><code>ObjectId</code></td>
<td>string</td>
<td>

A qualified or unqualified object ID from `Data/Objects`. This is analogous to
the vanilla items "Secret Note" (`(O)79`) and "Journal Scrap" (`(O)842`): this
item will be created as debris, and when the player uses it, one of the notes
matching it will be read (see [Spawning](#spawning), under how notes work, for
more details). This item's sprite will also be displayed in the collections
page to represent this note, either grayed out (unread) or normal (read).

If this field is `null` (the default), the mod will use its default secret note
object. For best results, you should omit it or specify a [custom
item](#custom-items).

*Default:* `null`

</td>
</tr>

<tr>
<td><code>NoteTexture</code></td>
<td>string</td>
<td>

A game asset path indicating the note background texture to use when displaying
the note. This is equivalent to specifying an asset via the [special mail format
code](https://stardewvalleywiki.com/Modding:Mail_data#Custom_mail_formatting)
`[letterbg]`, except that this field will take precedence if both are provided.
Since secret notes use the same game code as mail, all of the same formatting
caveats apply to the asset you specify here.

*Default:* `null`

</td>
</tr>

<tr>
<td><code>NoteTextureIndex</code></td>
<td>integer</td>
<td>

An integer specifying the index in the `NoteTexture` to use as a background.
This is equivalent to (but takes precedence over) specifying an index via
`[letterbg]`, as with NoteTexture.

*Default:* `0`

</td>
</tr>

<tr>
<td><code>NoteTextColor</code></td>
<td>string</td>
<td>

A string specifying what color to use to render the note's text, equivalent to
using the format code `[textcolor]` (but overriding it, as usual). This can be
any of the 10 acceptable vanilla color names:

`black`, `blue`, `red`, `purple`, `white`, `orange`, `green`, `cyan`, `gray`, `jojablue`

... or, you can specify any RGB color you like by using the form `rgb(<r>, <g>,
<b>)`, where r, g, and b are integers from 0 to 255. For example:

```
"NoteTextColor": "rgb(88, 34, 44)",
```

*Default:* `null`

</td>
</tr>

<tr>
<td><code>NoteImageTexture</code></td>
<td>string</td>
<td>

A game asset path indicating the texture to use when loading an image for an
[image note](#image-notes) (see that section for more details).
Note images are 64x64 pixels and are read in order, left-to-right and
top-to-bottom, just like other spritesheets; but be aware that there is a
hardcoded offset for the image of the piece of tape holding the image inside
the note (193/65, 14x21), so you should not use the index containing that.

If `null`, the default secret notes image texture
(`TileSheets/SecretNotesImages.png`) will be used.

*Default:* `null`

</td>
</tr>

<tr>
<td><code>NoteImageTextureIndex</code></td>
<td>integer</td>
<td>

An integer specifying the index in the `NoteImageTexture` to use for the note
image. Unlike `NoteTextureIndex`, the default value here is `-1`, since the
LetterViewerMenu itself uses this value to control rendering; as a result,
**this value controls whether your note is an image note or a text note**.

*Default:* `-1`

</td>
</tr>

<tr>
<td><code>ActionsOnFirstRead</code></td>
<td>array(string)</td>
<td>

This array of strings specifies what [trigger
actions](https://stardewvalleywiki.com/Modding:Trigger_actions) should be run
when the player reads this note by using the item from inventory (i.e. when the
note is added to the collection, versus when re-reading the note from the
collections menu).

The actions are run when the letter menu is closed.

**Note**: if you use this mod's trigger action to mark a note as seen (adding
it to a player's collection), these actions **will not** run. Likewise, if you
mark a seen note as unseen and it is collected again, the actions will run
again when the player views the note.

*Default:* `[]`

</td>
</tr>

</table>


### Content Patcher example

A Content Patcher patch to add a secret note to a mod might look like this:

```js
{
  "Action": "EditData",
  "Target": "Mods/ichortower.SecretNoteFramework/Notes",
  "Entries": {
    "{{ModId}}_SecretNote01": {
      "Contents": "I sure hope nobody finds this! It's full of embarrassing secrets.",
      "Title": "TOP SECRET DIARY",
      "Conditions": "PLAYER_HEARTS Current {{YourNpc}} 4",
      "ActionsOnFirstRead": [
        "AddMail Current {{ModId}}_Mail_HowDareYouFindMyDiary tomorrow"
      ]
    }
  }
}
```

This patch creates a note which is available only after reaching 4 hearts with
`{{YourNpc}}`. When it is found and read, it sends a mail to the current
player for the next day, presumably to scold them for reading the diary.


### Querying Notes

If you want your notes to be read in a particular order, you can use the
following [game state
query](https://stardewvalleywiki.com/Modding:Game_state_queries) added by this
mod:

```
ichortower.SecretNoteFramework_PLAYER_HAS_MOD_NOTE <player> <note_id>
```

This query is specific to notes added via this framework, since they are stored
separately from the vanilla notes.

You could create a sequence of notes by making each one require the previous
one, or you could gate a set of notes behind having first acquired a different
one, or similar. Remember that the `Conditions` field on the note object is
evaluated only at the start of each day, so each step in a chain requires a new day to
