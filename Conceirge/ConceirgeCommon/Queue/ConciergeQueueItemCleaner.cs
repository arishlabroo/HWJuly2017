namespace ConceirgeCommon.Queue
{
    //Generic so that we can get one singleton per T.
    public interface IConciergeQueueItemCleaner<T>
    {
        void Clean(ConceirgeQueueItemInfo itemInfo);
    }
}
