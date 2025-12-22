using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Core.Helpers;

public static class RoomExtension
{
    extension(Room room)
    {
        public bool HasSeat(int row, int col)
        {
            if (row < 0 || col < 0)
                return false;
            return room.Seats.Any(x => x.Row == row && x.Col == col);
        }
    }
}