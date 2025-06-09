using ManejoAlquileres.Service.Interface;

namespace ManejoAlquileres.Models.Helpers
{
    public class GeneradorIds : IGeneradorIdsService
    {
        private static readonly char[] caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private static readonly Random random = new();

        private readonly Func<string, Task<bool>> _existeIdAsync;
        public GeneradorIds(Func<string, Task<bool>> existeIdAsync)
        {
            _existeIdAsync = existeIdAsync;
        }

        private string GenerarIdAleatorio(int longitud = 9)
        {
            var idChars = new char[longitud];
            for (int i = 0; i < longitud; i++)
            {
                idChars[i] = caracteres[random.Next(caracteres.Length)];
            }
            return new string(idChars);
        }

        public async Task<string> GenerarIdUnicoAsync()
        {
            string nuevoId;
            bool existe;

            do
            {
                nuevoId = GenerarIdAleatorio();
                existe = await _existeIdAsync(nuevoId);
            } while (existe);

            return nuevoId;
        }
    }
}
