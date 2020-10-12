// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma once

#include "../antlr4-runtime/antlr4-runtime.h"
#include <string>
#include <map>

class Expression;
class Constant;
class ExpressionEvaluator;
class FunctionTable;

typedef ExpressionEvaluator* (*EvaluatorLookup)(std::string type);
typedef std::pair<std::string, std::string> (*EvaluateExpressionFunction)(Expression*, void*, void*);
typedef void (*EvaluateExpressionValidatorFunction)(Expression*);

typedef std::pair<void*, std::string> ValueErrorTuple;

#include "../Parser/Expression.h"
#include "../Parser/ExpressionEvaluator.h"
#include "../Parser/ExpressionParser.h"

