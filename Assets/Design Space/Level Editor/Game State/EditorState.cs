using System;

[Flags]
public enum EditorState
{
    Editing = 1 << 0,
    Playing = 1 << 1,
    TextInput = 1 << 2,
}