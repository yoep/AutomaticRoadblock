namespace AutomaticRoadblocks.Models
{
    public class InvalidModelException : ModelException
    {
        public InvalidModelException(IModel model)
            : base($"Model instance is invalid for {model}")
        {
        }
    }
}