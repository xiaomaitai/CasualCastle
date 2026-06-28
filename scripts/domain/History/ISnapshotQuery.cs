namespace CasualCastle.Domain.History;

public interface ISnapshotQuery
{
    CastleSnapshot GetSelectedNightSnapshot(int nightIndex);
}
