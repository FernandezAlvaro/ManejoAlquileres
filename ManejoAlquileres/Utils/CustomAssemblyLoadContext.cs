using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace ManejoAlquileres.Utils
{
    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(string absolutePath)
        {
            return LoadUnmanagedDll(absolutePath);
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllPath)
        {
            if (!System.IO.File.Exists(unmanagedDllPath))
                throw new DllNotFoundException($"No se encontró la DLL en la ruta: {unmanagedDllPath}");

            return NativeLibrary.Load(unmanagedDllPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}