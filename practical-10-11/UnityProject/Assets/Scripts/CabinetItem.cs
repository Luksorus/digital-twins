public class CabinetItem : PortableItem
{
    public HingedDoor door;
    public override bool CanTake => door&&door.IsOpen;
}
