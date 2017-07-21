namespace ConceirgeCommon
{
    public interface IMapper<in TA, TB>
    {
        TB MapNew(TA from);
        void MapExisting(TA from, TB to);
    }
}