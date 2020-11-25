#pragma once

#include "../Code/pch.h"
#include "ExpressionEvaluator.h"

static class ExpressionFunctions
{

public:
    static const std::map<std::string, ExpressionEvaluator*> standardFunctions;

private:
    static std::map<std::string, ExpressionEvaluator*> getStandardFunctions();


};