using NxEditor.Core.Components;

namespace NxEditor.Core.Models;

public record FindResult(int OccurrenceCount, List<FindResultDocument> Documents, List<FindResultToken> Tokens);
public record FindResultDocument(INxDocument Document, int OccurrenceCount, List<FindResultToken> Tokens);
public record FindResultToken(string Prefix, string Suffix);
