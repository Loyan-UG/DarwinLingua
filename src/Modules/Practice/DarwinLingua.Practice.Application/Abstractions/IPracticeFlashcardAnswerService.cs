using DarwinLingua.Practice.Application.Models;

namespace DarwinLingua.Practice.Application.Abstractions;

/// <summary>
/// Coordinates flashcard-answer submission workflows.
/// </summary>
public interface IPracticeFlashcardAnswerService
{
    /// <summary>
    /// Persists one flashcard answer and updates scheduling state.
    /// </summary>
    Task<PracticeFlashcardAnswerResultModel> SubmitAsync(
        PracticeFlashcardAnswerRequestModel request,
        CancellationToken cancellationToken);
}
