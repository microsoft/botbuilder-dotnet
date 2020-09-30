// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma once

#include "Expression.h"

class Constant : public Expression
{

public:
    Constant(int value);
};