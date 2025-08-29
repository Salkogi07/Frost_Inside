using System;
using Unity.Netcode;

[Serializable]
public struct Inventory_Item : INetworkSerializable, IEquatable<Inventory_Item>
{
    public int itemId;
    public int price;

    public Inventory_Item(int _itemId, int _price)
    {
        itemId = _itemId;
        price = _price;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemId);
        serializer.SerializeValue(ref price);
    }

    public bool Equals(Inventory_Item other)
    {
        return itemId == other.itemId && price == other.price;
    }

    // 빈 아이템인지 확인. ID가 -1이면 빈 아이템으로 취급.
    public bool IsEmpty()
    {
        return itemId == -1;
    }

    // 기본 생성자는 빈 아이템을 만듭니다.
    public static Inventory_Item Empty => new Inventory_Item(-1, 0);
}