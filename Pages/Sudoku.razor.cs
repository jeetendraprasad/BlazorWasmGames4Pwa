using BlazorWasmGames4Pwa.Code;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;



namespace BlazorWasmGames4Pwa.Pages
{
    public partial class Sudoku
    {
        SudokuGame _sudokuGame = new();
        readonly SudokuUi _sudokuUi = new();

        bool _render = true;

        protected override bool ShouldRender() => _render;

        private void OnChangeGrid()
        {
            _render = false;

            _sudokuGame.ReInit(_sudokuUi.RowsBlock, _sudokuUi.ColsBlock);

            UpdateUISizeBindings();

            _render = true;
            //this.StateHasChanged();
        }

        async Task OnChangeCellInput(int value, string cellInputId)
        {
            ////////////////////////// OK

            _render = false;

            _sudokuGame.NewMoveForInput(value, cellInputId);

            //_sudokuGame.ResetAllPositions();

            //_sudokuGame.UpdatePositionsByMoves();

            //_sudokuGame.RenewHints(false);

            _sudokuUi.SetPositions(_sudokuGame.GetPositionsCloned());

            _render = true;

            await Task.FromResult(0);
        }

        async Task OnChangeGridSize(int value, string controlId)
        {
            _render = false;

            if ("rows_size" == controlId)
                _sudokuUi.SetRowsBlock(value);
            else if ("cols_size" == controlId)
                _sudokuUi.SetColsBlock(value);
            else {; }

            //UpdateUISizeBindings();
            //_sudokuUi.GridSize = _sudokuUi.RowsBlock * _sudokuUi.ColsBlock;
            Integer1 rowsBlock1 = new(0, _sudokuGame.RowsBlockMinVal, _sudokuGame.RowsBlockMaxVal);
            Integer1 colsBlock1 = new(0, _sudokuGame.ColsBlockMinVal, _sudokuGame.ColsBlockMaxVal);

            rowsBlock1.ValAsInt = _sudokuUi.RowsBlock;
            colsBlock1.ValAsInt = _sudokuUi.ColsBlock;

            _sudokuUi.SetRowsBlock(rowsBlock1.ValAsInt);
            _sudokuUi.SetColsBlock(colsBlock1.ValAsInt);

            //Console.WriteLine(JsonSerializer.Serialize(_sudokuUi));

            _render = true;

            //this.StateHasChanged();

            await Task.FromResult(0);
        }

        protected override async Task OnInitializedAsync()
        {
            _sudokuGame = new SudokuGame();
            //_sudokuUi = _sudokuGame.GetSudokuUi();
            UpdateUISizeBindings();

            await Task.FromResult(0);
        }

        void UpdateUISizeBindings()
        {
            _sudokuUi.SetRowsBlock(_sudokuGame.GetRowsBlock());
            _sudokuUi.SetColsBlock(_sudokuGame.GetColsBlock());
            //_sudokuUi.GridSize = _sudokuUi.RowsBlock * _sudokuUi.ColsBlock;
            _sudokuUi.SetPositions(_sudokuGame.GetPositionsCloned());
        }



        private async Task OnClickUndoBtn()
        {
            _render = false;

            if (!_sudokuGame.MoveUndo())
            {
                SweetAlertResult result = await swal.FireAsync(new SweetAlertOptions
                {
                    Icon = "error",
                    Title = "Oops...",
                    Text = "Nothing to undo."
                });

                _render = true;

                await Task.FromResult(0);

                return;
            }

            //_sudokuGame.ResetAllPositions();

            //_sudokuGame.UpdatePositionsByMoves();

            //_sudokuGame.RenewHints(false);

            _sudokuUi.SetPositions(_sudokuGame.GetPositionsCloned());

            _render = true;

            await Task.FromResult(0);
        }
        private void OnClickHintBtn(string hintBtnId)
        {
            ////////////////////////////////// OK

            _render = false;

            _sudokuGame.NewMoveForHint(hintBtnId);

            //_sudokuGame.ResetAllPositions();

            //_sudokuGame.UpdatePositionsByMoves();

            //_sudokuGame.RenewHints(false);

            _sudokuUi.SetPositions(_sudokuGame.GetPositionsCloned());

            _render = true;
        }

        private async Task OnNextTipBtn()
        {
            List<SudokuTip> tip = _sudokuGame.FindNextTip();

            string json = JsonSerializer.Serialize(tip ?? new object());

            SweetAlertResult result = await swal.FireAsync(new SweetAlertOptions
            {
                Icon = "success",
                Title = "Tip",
                Text = json,
            });

            await Task.FromResult(0);
        }
        private async Task SudokuFileUpload(InputFileChangeEventArgs e)
        {
            _render = false;

            if (e.File is not null)
            {
                IBrowserFile file = e.File;
                using MemoryStream ms = new();
                await file.OpenReadStream().CopyToAsync(ms);
                byte[] bytes = ms.ToArray();
                string json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                bool success = _sudokuGame.LoadSaveDataAsJson(json);

                if (!success)
                {
                    SweetAlertResult result = await swal.FireAsync(new SweetAlertOptions
                    {
                        Icon = "error",
                        Title = "Error",
                        Text = "Error loading json file. Is this valis json file?",
                    });
                }
                else
                {
                    SweetAlertResult result = await swal.FireAsync(new SweetAlertOptions
                    {
                        Icon = "success",
                        Title = "Success",
                        Text = "Game imported successfully.",
                    });
                }
            }
            else
            {
                SweetAlertResult result = await swal.FireAsync(new SweetAlertOptions
                {
                    Icon = "error",
                    Title = "Error",
                    Text = "Error loading json file. Is this valis json file?",
                });
            }

            _sudokuUi.SetPositions(_sudokuGame.GetPositionsCloned());

            _render = true;

            await Task.FromResult(0);
        }
        private async Task SudokuFileDownload()
        {
            string json = _sudokuGame.GetSaveDataAsJson();
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            using MemoryStream fileStream = new MemoryStream(byteArray);
            string fileName = "Sudoku.bwg4pwa";

            using DotNetStreamReference streamRef = new DotNetStreamReference(stream: fileStream);

            await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);

            await Task.FromResult(0);
        }
        private async Task OnHighlightAllLoneHintsBtn()
        {
            _render = false;

            _sudokuGame.ResetAllHighlights();

            List<SudokuTip> tips = _sudokuGame.FindNextTip_SolvableByLoneHintFirstOrAll(onlyFirst: false);

            _sudokuGame.HighlightAllHintsByTips(tips);

            _sudokuUi.SetPositions(_sudokuGame.GetPositionsCloned());

            _render = true;

            await Task.FromResult(0);
        }
    }

    internal class SudokuUi
    {
        int _rowsBlock, _colsBlock;
        Dictionary<string, SudokuCellInfo> _positions = [];

        public Dictionary<string, SudokuCellInfo> Positions
        {
            get { return _positions; }
        }

        public void SetPositions(Dictionary<string, SudokuCellInfo> positions)
        {
            if (_positions.Count != positions.Count)
                this.GridUpdated = false;

            _positions = positions;
        }

        public int RowsBlock
        {
            get { return _rowsBlock; }
        }
        public int ColsBlock
        {
            get { return _colsBlock; }
        }

        public void SetRowsBlock(int value)
        {
            GridUpdated = true;
            _rowsBlock = value;
        }

        public void SetColsBlock(int value)
        {
            GridUpdated = true;
            _colsBlock = value;
        }

        public bool GridUpdated { get; private set; } = false;

        private static void GetBlockRowAndBlockColFromCellId(string cellId, out int row, out int col)
        {
            // cellIdPrefix = "{0}:C[{1},{2}]"
            string[]? splitted = cellId.Split('[', ',', ']');
            _ = int.TryParse(splitted[1], out row);
            _ = int.TryParse(splitted[2], out col);
        }

        string GetBlockColor(int row, int col)
        {
            // https://en.wikipedia.org/wiki/Web_colors#Basic_colors
            //List<string> basicHtmlColors = ["silver", "gray", "red", "yellow", "lime", "aqua", "teal", "olive", "fuchsia", "purple", "green", "maroon", "blue", "navy", "white", "black",];
            // https://www.w3schools.com/colors/colors_names.asp
            //List<string> extendedHtmlColors = ["AliceBlue", "AntiqueWhite", "Aqua", "Aquamarine", "Azure", "Beige", "Bisque", "Black", "BlanchedAlmond", "Blue", "BlueViolet", "Brown", "BurlyWood", "CadetBlue", "Chartreuse", "Chocolate", "Coral", "CornflowerBlue", "Cornsilk", "Crimson", "Cyan", "DarkBlue", "DarkCyan", "DarkGoldenRod", "DarkGray", "DarkGreen", "DarkGrey", "DarkKhaki", "DarkMagenta", "DarkOliveGreen", "DarkOrange", "DarkOrchid", "DarkRed", "DarkSalmon", "DarkSeaGreen", "DarkSlateBlue", "DarkSlateGray", "DarkSlateGrey", "DarkTurquoise", "DarkViolet", "DeepPink", "DeepSkyBlue", "DimGray", "DimGrey", "DodgerBlue", "FireBrick", "FloralWhite", "ForestGreen", "Fuchsia", "Gainsboro", "GhostWhite", "Gold", "GoldenRod", "Gray", "Green", "GreenYellow", "Grey", "HoneyDew", "HotPink", "IndianRed", "Indigo", "Ivory", "Khaki", "Lavender", "LavenderBlush", "LawnGreen", "LemonChiffon", "LightBlue", "LightCoral", "LightCyan", "LightGoldenRodYellow", "LightGray", "LightGreen", "LightGrey", "LightPink", "LightSalmon", "LightSeaGreen", "LightSkyBlue", "LightSlateGray", "LightSlateGrey", "LightSteelBlue", "LightYellow", "Lime", "LimeGreen", "Linen", "Magenta", "Maroon", "MediumAquaMarine", "MediumBlue", "MediumOrchid", "MediumPurple", "MediumSeaGreen", "MediumSlateBlue", "MediumSpringGreen", "MediumTurquoise", "MediumVioletRed", "MidnightBlue", "MintCream", "MistyRose", "Moccasin", "NavajoWhite", "Navy", "OldLace", "Olive", "OliveDrab", "Orange", "OrangeRed", "Orchid", "PaleGoldenRod", "PaleGreen", "PaleTurquoise", "PaleVioletRed", "PapayaWhip", "PeachPuff", "Peru", "Pink", "Plum", "PowderBlue", "Purple", "RebeccaPurple", "Red", "RosyBrown", "RoyalBlue", "SaddleBrown", "Salmon", "SandyBrown", "SeaGreen", "SeaShell", "Sienna", "Silver", "SkyBlue", "SlateBlue", "SlateGray", "SlateGrey", "Snow", "SpringGreen", "SteelBlue", "Tan", "Teal", "Thistle", "Tomato", "Turquoise", "Violet", "Wheat", "White", "WhiteSmoke", "Yellow", "YellowGreen",];

            int ind = (int)Math.Sqrt(_positions.Count) * row + col;
            List<string> basicHtmlColors = ["silver", "gray", "red", "yellow", "lime", "aqua", "teal", "olive", "fuchsia", "purple", "green", "maroon", "blue", "navy", "white", "black",];
            return basicHtmlColors[ind % basicHtmlColors.Count];
        }

        readonly int sodukuSizeInPx = 900;

        public string GetSodukuStyle() => $"width: {sodukuSizeInPx}px;height: {sodukuSizeInPx}px;";

        public string GetCellStyle(string cellId)
        {
            string style = $"width: {sodukuSizeInPx / Math.Sqrt(_positions.Count)}px;height: {sodukuSizeInPx / Math.Sqrt(_positions.Count)}px;float:left;border: solid;padding: 0px; font-size: 100%;"; // display:flex; flex-direction: column;font-size: 2em;

            //if (cellId != null)
            {
                GetBlockRowAndBlockColFromCellId(cellId, out int x, out int y);
                string color = GetBlockColor(x, y);
                style += $"background-color:{color};";
            }

            return style;
        }

        public static string GetInputStyle(bool cellValueClashing)
        {
            string retVal = "overflow : hidden; height:20%; width : 100%; " + (!cellValueClashing ? "" : "text-decoration: line-through; padding: 0px;"); //  font-size: 80%;

            return retVal;
        }

        public static string GetButtonStyle(bool highlighted) => "height:20%; width:20%; padding: 0px;" + (highlighted ? "background-color: red;" : ""); // $"width: {sodukuSizeInPx}px;height: {sodukuSizeInPx}px;"; // font-size: 80%;
    }
}
