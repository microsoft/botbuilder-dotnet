namespace AspNetCore_EchoBot_With_State
{
    /// <summary>
    /// Class for storing conversation state. 
    /// </summary>
    public class SinglePromptState
    {
        public PrompState Prompt { get; set; } = PrompState.Default;
    }

    public enum PrompState
    {
        Default,
        Name
    }
}
