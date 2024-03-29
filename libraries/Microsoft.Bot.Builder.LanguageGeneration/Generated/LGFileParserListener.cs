//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.11.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from LGFileParser.g4 by ANTLR 4.11.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

#pragma warning disable 3021
using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="LGFileParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.CLSCompliant(false)]
public interface ILGFileParserListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="LGFileParser.file"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFile([NotNull] LGFileParser.FileContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LGFileParser.file"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFile([NotNull] LGFileParser.FileContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LGFileParser.paragraph"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterParagraph([NotNull] LGFileParser.ParagraphContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LGFileParser.paragraph"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitParagraph([NotNull] LGFileParser.ParagraphContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LGFileParser.commentDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCommentDefinition([NotNull] LGFileParser.CommentDefinitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LGFileParser.commentDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCommentDefinition([NotNull] LGFileParser.CommentDefinitionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LGFileParser.importDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterImportDefinition([NotNull] LGFileParser.ImportDefinitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LGFileParser.importDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitImportDefinition([NotNull] LGFileParser.ImportDefinitionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LGFileParser.optionDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterOptionDefinition([NotNull] LGFileParser.OptionDefinitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LGFileParser.optionDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitOptionDefinition([NotNull] LGFileParser.OptionDefinitionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LGFileParser.errorDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterErrorDefinition([NotNull] LGFileParser.ErrorDefinitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LGFileParser.errorDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitErrorDefinition([NotNull] LGFileParser.ErrorDefinitionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LGFileParser.templateDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LGFileParser.templateDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LGFileParser.templateNameLine"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTemplateNameLine([NotNull] LGFileParser.TemplateNameLineContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LGFileParser.templateNameLine"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTemplateNameLine([NotNull] LGFileParser.TemplateNameLineContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LGFileParser.templateBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTemplateBody([NotNull] LGFileParser.TemplateBodyContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LGFileParser.templateBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTemplateBody([NotNull] LGFileParser.TemplateBodyContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LGFileParser.templateBodyLine"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTemplateBodyLine([NotNull] LGFileParser.TemplateBodyLineContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LGFileParser.templateBodyLine"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTemplateBodyLine([NotNull] LGFileParser.TemplateBodyLineContext context);
}
