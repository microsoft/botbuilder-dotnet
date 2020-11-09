#pragma once

#include "../Code/pch.h"

enum class ReturnType { 
    Boolean = 1, 
    Number = 2,
    Object = 4,
    String = 8,
    Array = 16
};

inline bool operator==(const ReturnType& rt, const int& val)
{
    return ((int)rt) == val;
}

inline ReturnType operator&(const ReturnType& r1, const ReturnType& r2)
{
    return (ReturnType)(((int)r1) & ((int)r2));
}

inline ReturnType operator|(const ReturnType& r1, const ReturnType& r2)
{
    return (ReturnType)(((int)r1) | ((int)r2));
}