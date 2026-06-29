using UnityEngine;

public enum InteractionObjects
{
    None,
    Hat,
    TeddyBear,
    Clown,
    Cube,
    Chair,
    Guitar,
    Ball,
    Canvas,
    Computer,
    Skate,
    Locker,
    PostIt,
    PostItReaded,
    Bench,
    Sky
}

public enum InteractionResult
{
    Default,
    Correct,
    Incorrect
}

public struct InteractionContext
{
    public Transform player;
    public Transform target;
    public InteractionObjects objectType;
    public InteractionResult result;
}