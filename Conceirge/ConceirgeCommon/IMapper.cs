namespace ConceirgeCommon
{
    public interface IMapper<in TA, TB>
    {
        TB MapNew(TA from);
        TB MapExisting(TA from, TB to);
    }
}