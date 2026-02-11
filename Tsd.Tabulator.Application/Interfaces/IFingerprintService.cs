namespace Tsd.Tabulator.Application.Interfaces;

public interface IFingerprintService
{
    IReadOnlyList<string> SplitAndNormalizeParticipants(string participantsRaw);
    string NormalizeName(string name);
    string ComputeRoutineFingerprint(
        string studioName,
        string routineTitle,
        string entryType,
        string? category,
        string? @class,
        IReadOnlyList<string> participantsNormalized
    );
}
