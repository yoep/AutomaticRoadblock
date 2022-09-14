namespace AutomaticRoadblocks.Models
{
    public class ModelNotFoundException : ModelException
    {
        public ModelNotFoundException(string scriptName) 
            : base($"Model with script name {scriptName} couldn't be found")
        {
        }
    }
}