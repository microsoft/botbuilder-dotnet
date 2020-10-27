// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma once

#define _SILENCE_CXX17_CODECVT_HEADER_DEPRECATION_WARNING
#define _SILENCE_ALL_CXX17_DEPRECATION_WARNINGS
#pragma warning (disable: 4996) // codecvt in C++17

#include "../antlr4-runtime/antlr4-runtime.h"
#include <string>
#include <map>
#include <any>
#include <functional>
#include <algorithm>
#include "../Parser/ReturnType.h"

class Expression;
class Constant;
class ExpressionEvaluator;
class FunctionTable;

typedef ExpressionEvaluator* (*EvaluatorLookup)(std::string type);
typedef std::pair<std::string, std::string> (*EvaluateExpressionFunction)(Expression*, void*, void*);
typedef void (*EvaluateExpressionValidatorFunction)(Expression*);

typedef std::pair<std::any, std::string> ValueErrorTuple;
typedef std::function<ValueErrorTuple (Expression*, void*, void*)> EvaluateExpressionLambda;

#include "../Parser/Expression.h"
#include "../Parser/ExpressionEvaluator.h"
#include "../Parser/ExpressionParser.h"

