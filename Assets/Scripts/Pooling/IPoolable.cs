public interface IPoolable
{
    /// <summary>
    /// Called at runtime when the Pool is asked to get back this object. Is called in <see cref="ObjectPool{ObjectPoolType}.TryReleasePooled(ObjectPoolType)"/>
    /// Never call this directly !
    /// </summary>
    void DisableFromPool();

    public bool TryReleaseToPool();

    public IObjectPool Pool { get; set; }
    bool Pooled { get; set; }
}