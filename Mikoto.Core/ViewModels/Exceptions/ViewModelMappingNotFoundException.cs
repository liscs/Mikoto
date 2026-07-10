namespace Mikoto.Core.ViewModels.Exceptions
{
    public class ViewModelMappingNotFoundException : Exception
    {
        public Type? ViewModelType { get; }

        public ViewModelMappingNotFoundException()
            : base("The requested ViewModel is not mapped to any View.") { }

        public ViewModelMappingNotFoundException(Type viewModelType)
            : base($"No View mapping found for ViewModel type '{viewModelType.FullName}'.")
        {
            ViewModelType = viewModelType;
        }

        public ViewModelMappingNotFoundException(string? message) : base(message) { }

        public ViewModelMappingNotFoundException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}