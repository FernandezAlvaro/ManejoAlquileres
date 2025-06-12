using DinkToPdf.Contracts;
using DinkToPdf;
using ManejoAlquileres.Service.Interface;

namespace ManejoAlquileres.Service
{
    public class HtmlToPdfConverter : IHtmlToPdfConverter
    {
        private readonly IConverter _converter;

        public HtmlToPdfConverter(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] ConvertHtmlToPdf(string html)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    Margins = new MarginSettings
                    {
                        Top = 10,
                        Bottom = 10,
                        Left = 10,
                        Right = 10
                    }
                },
                Objects = {
                        new ObjectSettings
                        {
                            HtmlContent = html,
                            WebSettings = { DefaultEncoding = "utf-8" }
                        }
                    }
                };
            return _converter.Convert(doc);
        }
    }
}