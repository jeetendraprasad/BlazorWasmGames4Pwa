namespace BlazorWasmGames4Pwa.StateContainer
{
    public class Calculator2StateFixed
    {
        public enum ButtonTypes
        {
            Digit,             // All things that are part of numbers. 0 and 1 to 9 and dot(.) and +-/*
            Equal,             // equal button
            Clear,              // clear button
        }

        public class ButtonInformation
        {
            public ButtonTypes Type { get; set; }
            public string Text { get; set; } = "";
        }
        public ButtonInformation[,] ButtonsInfo { get; } = {
            { new() { Text = "7", Type = ButtonTypes.Digit }, new() { Text = "8", Type = ButtonTypes.Digit }, new() { Text = "9", Type = ButtonTypes.Digit }, new() { Text = "/", Type = ButtonTypes.Digit }, },
            { new() { Text = "4", Type = ButtonTypes.Digit }, new() { Text = "5", Type = ButtonTypes.Digit }, new() { Text = "6", Type = ButtonTypes.Digit }, new() { Text = "*", Type = ButtonTypes.Digit }, },
            { new() { Text = "1", Type = ButtonTypes.Digit }, new() { Text = "2", Type = ButtonTypes.Digit }, new() { Text = "3", Type = ButtonTypes.Digit }, new() { Text = "-", Type = ButtonTypes.Digit }, },
            { new() { Text = "0", Type = ButtonTypes.Digit }, new() { Text = ".", Type = ButtonTypes.Digit }, new() { Text = "+", Type = ButtonTypes.Digit }, new() { Text = "=", Type = ButtonTypes.Equal }, },
        };

    }
}
