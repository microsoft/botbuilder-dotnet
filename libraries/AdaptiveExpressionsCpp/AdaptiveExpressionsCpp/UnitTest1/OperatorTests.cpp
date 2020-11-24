#include "pch.h"
#include "CppUnitTest.h"

#include "../Code/pch.h"
#include "ExpressionParser.h"
#include "FunctionUtils.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace OperatorTests
{
	TEST_CLASS(OperatorTests)
	{
	public:


        void MathTest(std::string expression, int expectedValue)
        {
            auto parsed = Expression::Parse(expression);

            ValueErrorTuple valueAndError = parsed->TryEvaluate(nullptr);

            bool cast{};
            Assert::AreEqual(expectedValue, FunctionUtils::castToType<int>(valueAndError.first, cast));

            std::cout << "Error " << valueAndError.second << std::endl;
        }

        TEST_METHOD(AddOperatorTest)
		{
            MathTest("1 + 2", 3);
		}

        TEST_METHOD(AddFunctionTest)
        {
            MathTest("add(1, 2, 3)", 6);
        }

        TEST_METHOD(SubtractOperatorTest)
        {
            MathTest("5 - 3", 2);
        }

        TEST_METHOD(SubtractFunctionTest)
        {
            MathTest("subtract(20, 4)", 16);
        }
	};
}
