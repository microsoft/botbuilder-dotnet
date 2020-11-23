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

        TEST_METHOD(SimpleAddTest)
		{
            auto parsed = Expression::Parse("1 + 2");

            ValueErrorTuple valueAndError = parsed->TryEvaluate(nullptr);

            bool cast{};
            Assert::AreEqual(3, FunctionUtils::castToType<int>(valueAndError.first, cast));

            std::cout << "Error " << valueAndError.second << std::endl;
		}

        TEST_METHOD(SimpleSubtractTest)
        {
            auto parsed = Expression::Parse("5 - 3");

            ValueErrorTuple valueAndError = parsed->TryEvaluate(nullptr);

            bool cast{};
            Assert::AreEqual(2, FunctionUtils::castToType<int>(valueAndError.first, cast));

            std::cout << "Error " << valueAndError.second << std::endl;
        }
	};
}
