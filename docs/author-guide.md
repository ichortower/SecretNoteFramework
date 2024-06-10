# Secret Note Framework - Author Guide

This document explains how to use this mod to add your custom Secret Notes to
Stardew Valley.

## Contents

* [Introduction](#introduction)
* [Adding Notes](#adding-notes)
* [Querying Notes](#querying-notes)
* [Querying Notes](#querying-notes)


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

**insert table here**

For example, a Content Patcher patch to add a secret note to a mod might look
like this:

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
