namespace ReviewerPortal.API.Application.Constants;

internal static class EligibilityRules
{
    internal const int MinimumPublications = 3;
    internal const decimal MinimumUniversityScore = 60m;
    internal const string InvitationSuccessMessage = "Invitation was successful";
    internal const string InvitationFailureMessage = "Invitation could not be sent";
}
