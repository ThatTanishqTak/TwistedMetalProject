using System;
using Unity.Netcode;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public struct TeamRoleData : IEquatable<TeamRoleData>, INetworkSerializable
{
    public TeamType team;
    public RoleType role;
    public int teamNumber;

    public bool Equals(TeamRoleData other)
    {
        return team == other.team && role == other.role && teamNumber == other.teamNumber;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref team);
        serializer.SerializeValue(ref role);
        serializer.SerializeValue(ref teamNumber);
    }
}

public enum TeamType { None = 0, TeamA, TeamB, TeamC, TeamD, TeamE }
public enum RoleType { None = 0, Driver, Shooter }