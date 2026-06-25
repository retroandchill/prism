/**
 * @file runtime.ixx
 * @author Francesco Corso
 * @date 6/25/2026
 * @brief
 */
module;

#include <antlr4-runtime.h>

export module antlr.runtime;

/*
Discovered Macros:
- ANTLR4CPP_ATN_DECLARED
- ANTLR4CPP_HAVE_BUILTIN
- ANTLR4CPP_INTERNAL_STRINGIFY
- ANTLR4CPP_NO_THREAD_SAFTEY_ANALYSIS
- ANTLR4CPP_PUBLIC
- ANTLR4CPP_STRINGIFY
- ANTLR4CPP_USING_ABSEIL
- ANTLRCPP_MAKE_VERSION
- ANTLRCPP_VERSION
- ANTLRCPP_VERSION_MAJOR
- ANTLRCPP_VERSION_MINOR
- ANTLRCPP_VERSION_PATCH
- ANTLRCPP_VERSION_STRING
- INVALID_INDEX
*/
#ifdef _WIN32
#ifdef _WIN64
export using ::ssize_t;
#endif
#endif
namespace antlr4
{
    export using antlr4::ANTLRErrorListener;
    export using antlr4::ANTLRErrorStrategy;
    export using antlr4::ANTLRFileStream;
    export using antlr4::ANTLRInputStream;
    export using antlr4::BailErrorStrategy;
    export using antlr4::BaseErrorListener;
    export using antlr4::BufferedTokenStream;
    export using antlr4::CharStream;
    export using antlr4::CommonToken;
    export using antlr4::CommonTokenFactory;
    export using antlr4::CommonTokenStream;
    export using antlr4::ConsoleErrorListener;
    export using antlr4::DefaultErrorStrategy;
    export using antlr4::DiagnosticErrorListener;
    export using antlr4::EmptyStackException;
    export using antlr4::FailedPredicateException;
    export using antlr4::IllegalArgumentException;
    export using antlr4::IllegalStateException;
    export using antlr4::InputMismatchException;
    export using antlr4::IntStream;
    export using antlr4::InterpreterRuleContext;
    export using antlr4::Lexer;
    export using antlr4::LexerInterpreter;
    export using antlr4::LexerNoViableAltException;
    export using antlr4::ListTokenSource;
    export using antlr4::NoSuchElementException;
    export using antlr4::NoViableAltException;
    export using antlr4::NullPointerException;
    export using antlr4::ParseCancellationException;
    export using antlr4::Parser;
    export using antlr4::ParserInterpreter;
    export using antlr4::ParserRuleContext;
    export using antlr4::ProxyErrorListener;
    export using antlr4::RecognitionException;
    export using antlr4::Recognizer;
    export using antlr4::RuleContext;
    export using antlr4::Token;
    export using antlr4::TokenFactory;
    export using antlr4::TokenSource;
    export using antlr4::TokenStream;
    export using antlr4::TokenStreamRewriter;
    export using antlr4::UnbufferedCharStream;
    export using antlr4::UnbufferedTokenStream;
    export using antlr4::WritableToken;
    namespace misc
    {
        export using misc::InterpreterDataReader;
        export using misc::Interval;
        export using misc::IntervalSet;
        export using misc::MurmurHash;
        export using misc::Utils;
        export using misc::Predicate;
        export using misc::numericToSymbol;
        export using misc::symbolToNumeric;
        export using misc::InterpreterData;
    } // namespace misc
    namespace atn
    {
        export using atn::ATN;
        export using atn::ATNConfig;
        export using atn::ATNConfigSet;
        export using atn::ATNDeserializationOptions;
        export using atn::ATNDeserializer;
        export using atn::ATNSerializer;
        export using atn::ATNSimulator;
        export using atn::ATNState;
        export using atn::ATNType;
        export using atn::ActionTransition;
        export using atn::ArrayPredictionContext;
        export using atn::AtomTransition;
        export using atn::BasicBlockStartState;
        export using atn::BasicState;
        export using atn::BlockEndState;
        export using atn::BlockStartState;
        export using atn::DecisionState;
        export using atn::EpsilonTransition;
        export using atn::LL1Analyzer;
        export using atn::LexerAction;
        export using atn::LexerActionExecutor;
        export using atn::LexerATNConfig;
        export using atn::LexerATNSimulator;
        export using atn::LexerMoreAction;
        export using atn::LexerPopModeAction;
        export using atn::LexerSkipAction;
        export using atn::LookaheadEventInfo;
        export using atn::LoopEndState;
        export using atn::NotSetTransition;
        export using atn::OrderedATNConfigSet;
        export using atn::ParseInfo;
        export using atn::ParserATNSimulator;
        export using atn::PlusBlockStartState;
        export using atn::PlusLoopbackState;
        export using atn::PrecedencePredicateTransition;
        export using atn::PredicateTransition;
        export using atn::PredictionContext;
        export using atn::PredictionMode;
        export using atn::PredictionModeClass;
        export using atn::RangeTransition;
        export using atn::RuleStartState;
        export using atn::RuleStopState;
        export using atn::RuleTransition;
        export using atn::SemanticContext;
        export using atn::SetTransition;
        export using atn::SingletonPredictionContext;
        export using atn::StarBlockStartState;
        export using atn::StarLoopEntryState;
        export using atn::StarLoopbackState;
        export using atn::TokensStartState;
        export using atn::Transition;
        export using atn::WildcardTransition;
        export using atn::SerializedATNView;
        export using atn::operator==;
        export using atn::operator!=;
        export using atn::operator<;
        export using atn::TransitionType;
        export using atn::transitionTypeName;
        export using atn::ConstTransitionPtr;
        export using atn::ATNStateType;
        export using atn::atnStateTypeName;
        export using atn::PredictionContextType;
        export using atn::PredictionContextCache;
        export using atn::PredictionContextMergeCache;
        export using atn::SemanticContextType;
        export using atn::LexerActionType;
        export using atn::DecisionEventInfo;
        export using atn::AmbiguityInfo;
        export using atn::ContextSensitivityInfo;
        export using atn::PredicateEvalInfo;
        export using atn::ErrorInfo;
        export using atn::DecisionInfo;
        export using antlr4::Lexer;
        export using atn::LexerChannelAction;
        export using atn::LexerCustomAction;
        export using atn::LexerIndexedCustomAction;
        export using atn::LexerModeAction;
        export using atn::LexerPushModeAction;
        export using atn::LexerTypeAction;
        export using atn::ProfilingATNSimulator;
        export using atn::PredictionContextMergeCacheOptions;
        export using atn::ParserATNSimulatorOptions;
    } // namespace atn
    namespace dfa
    {
        export using dfa::DFA;
        export using dfa::DFASerializer;
        export using dfa::DFAState;
        export using dfa::LexerDFASerializer;
        export using dfa::Vocabulary;
        export using dfa::operator==;
        export using dfa::operator!=;
    } // namespace dfa
    namespace tree
    {
        export using tree::AbstractParseTreeVisitor;
        export using tree::ErrorNode;
        export using tree::ErrorNodeImpl;
        export using tree::ParseTree;
        export using tree::ParseTreeListener;
        export using tree::ParseTreeProperty;
        export using tree::ParseTreeVisitor;
        export using tree::ParseTreeWalker;
        export using tree::SyntaxTree;
        export using tree::TerminalNode;
        export using tree::TerminalNodeImpl;
        export using tree::Tree;
        export using tree::Trees;
        namespace pattern
        {
            export using pattern::Chunk;
            export using pattern::ParseTreeMatch;
            export using pattern::ParseTreePattern;
            export using pattern::ParseTreePatternMatcher;
            export using pattern::RuleTagToken;
            export using pattern::TagChunk;
            export using pattern::TextChunk;
            export using pattern::TokenTagToken;
        } // namespace pattern
        namespace xpath
        {
            export using xpath::XPath;
            export using xpath::XPathElement;
            export using xpath::XPathLexerErrorListener;
            export using xpath::XPathRuleAnywhereElement;
            export using xpath::XPathRuleElement;
            export using xpath::XPathTokenAnywhereElement;
            export using xpath::XPathTokenElement;
            export using xpath::XPathWildcardAnywhereElement;
            export using xpath::XPathWildcardElement;
        } // namespace xpath
        export using tree::ParseTreeType;
        export using tree::ParseTreeTracker;
    } // namespace tree
    export using antlr4::RuntimeException;
    export using antlr4::IndexOutOfBoundsException;
    export using antlr4::UnsupportedOperationException;
    export using antlr4::IOException;
    export using antlr4::CancellationException;
#if ANTLR4CPP_USING_ABSEIL
    export using antlr4::FlatHashSet;
#endif
    export using antlr4::RuleContextWithAltNum;
    export using antlr4::RuntimeMetaData;
#if ANTLR4CPP_USING_ABSEIL
    export using antlr4::FlatHashMap;
#endif
} // namespace antlr4
export using ::Ref;
namespace antlrcpp
{
    export using antlrcpp::BitSet;
    export using antlrcpp::Any;
    export using antlrcpp::join;
    export using antlrcpp::toMap;
    export using antlrcpp::escapeWhitespace;
    export using antlrcpp::toHexString;
    export using antlrcpp::arrayToString;
    export using antlrcpp::replaceString;
    export using antlrcpp::split;
    export using antlrcpp::indent;
    export using antlrcpp::FinalAction;
    export using antlrcpp::finally;
    export using antlrcpp::is;
    export using antlrcpp::toString;
    export using antlrcpp::what;
    export using antlrcpp::downCast;
    export using antlrcpp::Arrays;
} // namespace antlrcpp
export using ::XPathLexer;
