namespace ManejoAlquileres.Service.Interface
{
    public interface IGeneradorIdsService
    {
        Task<string> GenerarIdUnicoAsync();
    }
}
