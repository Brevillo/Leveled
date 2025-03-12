using System;

[Flags]
public enum GameState
{
    Editing = 1 << 0,
    Playing = 1 << 1,
}