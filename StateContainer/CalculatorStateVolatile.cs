namespace BlazorWasmGames4Pwa.StateContainer
{
    public class CalculatorStateVolatile
    {
        private string? exprnString = "";

        public string MathExpression
        {
            get => exprnString ?? string.Empty;
            set
            {
                exprnString = value;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
