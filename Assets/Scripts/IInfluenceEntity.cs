namespace Natick.InfluenceMaps
{
    public interface IInfluenceEntity
    {
        bool IsRegisterable();

        EntityInformation GetEntityInformation();
    }
}