namespace PLib.Pooling
{
    public interface IPool
    {
        #region Basic Operations

        object Get();
        bool Put(object item);
        void Clear();

        #endregion
        #region Option: Pre-generation of objects

        void Prewarm(int count, float duration);

        #endregion
        #region Option: Quantity-based Destruction

        void MaxSize(int size);
        void Cull(bool immediate);

        #endregion
        #region Option: Time-based Destruction

        void StaleDuration(float duration);
        void Expire(bool immediate);

        #endregion
        #region Diagnostic

        int Available();
        int InUse();

        #endregion
    }
}
