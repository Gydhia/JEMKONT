public interface IObjectPool
{
    bool TryReleaseToPool(object room);
    bool CheckPoolState(bool urgent);
}