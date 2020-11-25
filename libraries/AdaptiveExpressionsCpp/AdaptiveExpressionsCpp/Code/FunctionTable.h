#pragma once

#include "../Code/pch.h"
#include "ExpressionEvaluator.h"

// this just needs to prepopulate, we could do something similar to AC, it may not be needed at all
class FunctionTable : public std::map<std::string, ExpressionEvaluator*>
{
public:
    FunctionTable();

private:
    void PopulateStandardFunctions();
};