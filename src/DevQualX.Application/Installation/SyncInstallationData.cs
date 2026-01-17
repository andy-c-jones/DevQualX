using DevQualX.Domain.Data;
using DevQualX.Domain.Infrastructure;
using DevQualX.Functional;

namespace DevQualX.Application.Installation;

/// <summary>
/// Syncs installation data (repositories and teams) from GitHub to the database.
/// </summary>
public class SyncInstallationData(
    IGitHubAppService gitHubAppService,
    IGitHubApiService gitHubApiService,
    IInstallationRepository installationRepository,
    IRepositoryRepository repositoryRepository,
    ITeamRepository teamRepository) : ISyncInstallationData
{
    /// <inheritdoc />
    public async Task<Result<SyncResult, Error>> ExecuteAsync(
        long installationId,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Get installation from database to get internal ID
        var installationResult = await installationRepository.GetByGitHubIdAsync(
            installationId,
            cancellationToken);

        if (installationResult is Failure<Domain.Models.Installation, Error> installationFailure)
        {
            return installationFailure.Error;
        }

        var installation = ((Success<Domain.Models.Installation, Error>)installationResult).Value;

        // Step 2: Generate installation access token
        var tokenResult = await gitHubAppService.CreateInstallationTokenAsync(
            installationId,
            cancellationToken);

        if (tokenResult is Failure<string, Error> tokenFailure)
        {
            return tokenFailure.Error;
        }

        var installationToken = ((Success<string, Error>)tokenResult).Value;

        // Step 3: Fetch repositories from GitHub
        var reposResult = await gitHubApiService.GetInstallationRepositoriesAsync(
            installationToken,
            cancellationToken);

        if (reposResult is Failure<IReadOnlyList<Domain.Models.GitHubRepository>, Error> reposFailure)
        {
            return reposFailure.Error;
        }

        var repositories = ((Success<IReadOnlyList<Domain.Models.GitHubRepository>, Error>)reposResult).Value;

        // Step 4: Sync repositories to database
        var syncReposResult = await repositoryRepository.SyncRepositoriesAsync(
            installation.Id,
            repositories.ToArray(),
            cancellationToken);

        if (syncReposResult is Failure<Unit, Error> syncReposFailure)
        {
            return syncReposFailure.Error;
        }

        // Step 5: Sync teams if this is an organization installation
        var teamsSynced = 0;
        if (installation.AccountType == Domain.Models.AccountType.Organization)
        {
            // Get organization teams
            var teamsResult = await gitHubApiService.GetOrganizationTeamsAsync(
                installationToken,
                installation.AccountLogin,
                cancellationToken);

            if (teamsResult is Failure<IReadOnlyList<Domain.Models.GitHubTeam>, Error> teamsFailure)
            {
                // Don't fail the entire sync if teams fail - just log and continue
                // Teams might not be accessible with current permissions
                teamsSynced = 0;
            }
            else
            {
                var teams = ((Success<IReadOnlyList<Domain.Models.GitHubTeam>, Error>)teamsResult).Value;

                var syncTeamsResult = await teamRepository.SyncTeamsAsync(
                    installation.Id,
                    teams.ToArray(),
                    cancellationToken);

                if (syncTeamsResult is Success<Unit, Error>)
                {
                    teamsSynced = teams.Count;
                }
            }
        }

        return new SyncResult(
            InstallationId: installationId,
            RepositoriesSynced: repositories.Count,
            TeamsSynced: teamsSynced);
    }
}
