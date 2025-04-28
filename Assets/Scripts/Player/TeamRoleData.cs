using System;
using Unity.Netcode;

public struct TeamRoleData : IEquatable<TeamRoleData>, INetworkSerializable
{
    public TeamType team;
    public RoleType role;
    public int teamNumber;
    public ulong clientId;

    public bool Equals(TeamRoleData other)
    {
        return team == other.team && role == other.role && teamNumber == other.teamNumber && clientId == other.clientId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref team);
        serializer.SerializeValue(ref role);
        serializer.SerializeValue(ref teamNumber);
        serializer.SerializeValue(ref clientId); 
    }
}

public enum TeamType { None = 0, TeamA, TeamB, TeamC, TeamD }
public enum RoleType { None = 0, Driver, Shooter }
