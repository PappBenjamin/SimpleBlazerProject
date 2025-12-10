namespace BlazorApp1.Services
{
    public interface IFileReader
    {
        Task<List<string[]>> ReadFile(Stream stream);
    }
}
