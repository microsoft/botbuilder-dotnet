// ConsoleApplication1.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>

#include "../Code/pch.h"
#include "../Code/ExpressionParser.h"
#include <any>
#include "../Code/FunctionUtils.h"

int main()
{   
    // ExpressionParser::AntlrParse("a string");

    auto parsed = Expression::Parse("1 + 2");

    ValueErrorTuple valueAndError = parsed->TryEvaluate(nullptr);

    // std::cout << "Value " << valueAndError.first << std::endl;
    bool cast{};
    std::cout << "Value " << FunctionUtils::castToType<int>(valueAndError.first, cast) << std::endl;

    std::cout << "Error " << valueAndError.second << std::endl;

    return 0;
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
