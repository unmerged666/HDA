namespace HDA.Domain.Enums;

public enum UserRole
{
    Regular = 0,
    ProPlayer = 1,
    Admin = 2
}

public enum ProPlayerStatus
{
    PendingApproval = 0,
    Approved = 1,
    Rejected = 2
}

public enum PlayerRole
{
    Carry = 1,
    Midlaner = 2,
    Offlaner = 3,
    SoftSupport = 4,
    HardSupport = 5
}

public enum MatchFormat
{
    Bo1 = 1,
    Bo2 = 2,
    Bo3 = 3,
    Bo5 = 5
}

public enum MatchStatus
{
    Scheduled = 0,
    Live = 1,
    Completed = 2,
    Cancelled = 3,
    Postponed = 4
}

public enum TournamentTier
{
    S = 1,  // The International
    A = 2,  // Major
    B = 3,  // Minor
    C = 4,  // Regional league
    D = 5   // Open qualifier
}

public enum TournamentStatus
{
    Upcoming = 0,
    Ongoing = 1,
    Completed = 2
}

public enum StageType
{
    GroupStage = 0,
    Playoffs = 1,
    GrandFinals = 2,
    Qualifier = 3
}

public enum DraftAction
{
    Pick = 0,
    Ban = 1
}
