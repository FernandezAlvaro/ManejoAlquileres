namespace ManejoAlquileres.Service.Interface
{
    public interface IHtmlToPdfConverter
    {
        byte[] ConvertHtmlToPdf(string html);
    }
}
