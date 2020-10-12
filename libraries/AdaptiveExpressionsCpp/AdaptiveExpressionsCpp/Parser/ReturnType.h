#pragma once

#include "../Code/pch.h"

enum class ReturnType { 
    Boolean = 1, 
    Number = 2,
    Object = 4,
    String = 8,
    Array = 16
};