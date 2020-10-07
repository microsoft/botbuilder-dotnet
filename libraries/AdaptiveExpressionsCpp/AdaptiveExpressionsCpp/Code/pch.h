// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma once

#include "../antlr4-runtime/antlr4-runtime.h"
#include <string>

class Expression;
class Constant;
class ExpressionEvaluator;

typedef ExpressionEvaluator* (*EvaluatorLookup)(std::string type);

#include "../Parser/Expression.h"
#include "../Parser/ExpressionEvaluator.h"
#include "../Parser/ExpressionParser.h"

