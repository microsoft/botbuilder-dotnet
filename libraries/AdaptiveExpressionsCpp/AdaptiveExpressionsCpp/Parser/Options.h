#pragma once
#include "../Code/pch.h"

/// <summary>
/// Options used to define evaluation behaviors.
/// </summary>
struct Options
{
    // CPP_PORT_TODO - C# version defaults locale to Thread.CurrentThread.CurrentCulture.Name;
    std::string locale;

    // CPP_PORT_TODO - In C# this class includes a Null Substitution function:
    // "function that been called when there is null value hit in memory."
    // public Func<string, object> NullSubstitution { get; set; } = null;
    void* nullSubstitution = nullptr;
};
