#include "pch.h"
#include "CppUnitTest.h"

#include "../Code/pch.h"
#include "ExpressionParser.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace UnitTest1
{
	TEST_CLASS(UnitTest1)
	{
	public:
		
		TEST_METHOD(TestMethod1)
		{
            ExpressionParser::AntlrParse("a string");
		}
	};
}
