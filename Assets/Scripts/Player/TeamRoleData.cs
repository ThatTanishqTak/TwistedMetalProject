using System;
using Unity.Netcode;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public struct TeamRoleData : IEquatable<TeamRoleData>, INetworkSerializable
{
    public TeamType team;
    public RoleType role;
    public int teamNumber;
    public ulong clientId; // This is the client ID of the player assigned to this role

    public bool Equals(TeamRoleData other)
    {
        return team == other.team && role == other.role && teamNumber == other.teamNumber;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref team);
        serializer.SerializeValue(ref role);
        serializer.SerializeValue(ref teamNumber);
        serializer.SerializeValue(ref clientId); //serialize the clientId
    }
}

public enum TeamType { None = 0, TeamA, TeamB, TeamC, TeamD, TeamE }
public enum RoleType { None = 0, Driver, Shooter }