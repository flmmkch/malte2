namespace Malte2.Model
{

    /// <summary>
    /// Object de la base de donnée
    /// </summary>
    public interface IHasObjectId
    {
        long? Id { get; set; }
    }

}