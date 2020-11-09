#include "pch.h"
#include "CppUnitTest.h"

#include "../Code/pch.h"
#include "ExpressionAntlrLexer.h"
#include "ExpressionAntlrParser.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace AdaptiveExpressionsCppTest
{
	TEST_CLASS(AdaptiveExpressionsCppTest)
	{
	public:
		
		TEST_METHOD(TestMethod1)
		{
            auto chars = new antlr4::ANTLRInputStream("a string");
            auto lexer = new ExpressionAntlrLexer(chars);
            auto tokens = new antlr4::CommonTokenStream(lexer);
            auto parser = new ExpressionAntlrParser(tokens);

            parser->setBuildParseTree(true);

            auto tree = parser->expression();
            // auto visitor = new 

            // antlr4::tree::ParseTreeWalker::DEFAULT.walk(visitor, tree);
		}
	};
}
