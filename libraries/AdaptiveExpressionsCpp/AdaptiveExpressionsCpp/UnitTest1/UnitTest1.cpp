#include "pch.h"
#include "CppUnitTest.h"

#include "../Code/pch.h"
#include "ExpressionParser.h"
#include "FunctionUtils.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace UnitTest1
{
	TEST_CLASS(UnitTest1)
	{
	public:
		
		TEST_METHOD(TestMethod1)
		{
            auto parsed = Expression::Parse("1 + 2");

            ValueErrorTuple valueAndError = parsed->TryEvaluate(nullptr);

            bool cast{};
            Assert::AreEqual(3, FunctionUtils::castToType<int>(valueAndError.first, cast));

            std::cout << "Error " << valueAndError.second << std::endl;
		}
	};
}
