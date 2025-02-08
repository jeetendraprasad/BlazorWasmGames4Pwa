using BlazorWasmGames4Pwa.Pages;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlazorWasmGames4Pwa.Code
{
    internal class SudokuGame
    {
        readonly int _rowsBlockMinVal = 2;
        readonly int _rowsBlockMaxVal = 3;
        readonly int _rowsBlockStartVal = 2;
        readonly int _colsBlockMinVal = 2;
        readonly int _colsBlockMaxVal = 3;
        readonly int _colsBlockStartVal = 2;

        Dictionary<string, SudokuCellInfo> _positions = [];
        readonly Stack<Move> _moves = [];

        Integer1 _rowsBlock, _colsBlock;

        const string _blockIdPrefix = "B[{0},{1}]";                // block names are B[{0-based rows},{0-based cols}]
        const string _cellIdPrefix = "{0}:C[{1},{2}]";             // cell names are BLOCK_ID:[{0-based rows},{0-based cols}]

        public SudokuGame()
        {
            _rowsBlock = new(_rowsBlockStartVal, _rowsBlockMinVal, _rowsBlockMaxVal);
            _colsBlock = new(_colsBlockStartVal, _colsBlockMinVal, _colsBlockMaxVal);

            Init();
        }

        public Dictionary<string, SudokuCellInfo> GetPositionsCloned()
        {
            Dictionary<string, SudokuCellInfo> newClonedDictionary = _positions.ToDictionary(entry => entry.Key,
                                               entry => (SudokuCellInfo)entry.Value.Clone());

            return newClonedDictionary;
        }

        public void NewMoveForInput(int value, string cellInputId)
        {
            _moves.Push(new Move() { ControlId = cellInputId, MoveType = MoveType.InputChanged, InputNewValue = value, HintButtonNewValue = null, });
            //            _moves.Add(new(cellInputId, value));
            //_positions[cellInputId].CellValue = value;

            UpdatePositionsByMoves(true);

            //Console.WriteLine(JsonSerializer.Serialize(_positions));
        }

        void UpdatePositionsByMoves(bool doInit)
        {
            if(doInit)
                Init();

            foreach (Move move in _moves)
            {
                if(move.MoveType == MoveType.InputChanged)
                {
                    _positions[move.ControlId].CellValue = move.InputNewValue ?? 0;
                }
            }
        }

        public bool MoveUndo()
        {
            if(_moves.Count <= 0)
                return false;

            _moves.Pop();

            UpdatePositionsByMoves(true);

            return true;
        }

        void Init()
        {
            _rowsBlock = new(_rowsBlockStartVal, _rowsBlockMinVal, _rowsBlockMaxVal);
            _colsBlock = new(_colsBlockStartVal, _colsBlockMinVal, _colsBlockMaxVal);

            _positions = GetSuHoriFull().Flatten().Select(x => new KeyValuePair<string, SudokuCellInfo>(x,
                new SudokuCellInfo(cellId: x,
                //positionType: SudokuPositionTypeEnum.None,
                maxCellValue: _rowsBlock.ValAsInt * _colsBlock.ValAsInt, 0)
            ))
                .ToDictionary(t => t.Key, t => t.Value);
        }

        // Get Solving Unit - Horigontal list - Full
        List<List<string>> GetSuHoriFull()
        {
            List<List<string>> retVal = [];

            for (int i = 0; i < _rowsBlock.ValAsInt * _colsBlock.ValAsInt; i++)
            {
                //if(i == 1)
                {
                    List<string> list = GetSuHori(i);
                    retVal.Add(list);
                }
            }

            return retVal;
        }
        // Get Solving Unit - Horigontal list
        List<string> GetSuHori(int rowNoSoduku)
        {
            List<string> retVal = [];

            int p = (int)(rowNoSoduku / _colsBlock.ValAsInt);
            int r = (int)(rowNoSoduku % _colsBlock.ValAsInt);

            for (int j = 0; j < _rowsBlock.ValAsInt * _colsBlock.ValAsInt; j++)
            {
                int q = (int)(j / _rowsBlock.ValAsInt);
                int s = (int)(j % _rowsBlock.ValAsInt);

                string blockId = GetBlockId(p, q);

                retVal.Add(string.Format(_cellIdPrefix, blockId, r, s));
            }

            return retVal;
        }

        // Get Solving Unit - Vertical list - Full
        List<List<string>> GetSuVertFull()
        {
            List<List<string>> retVal = [];

            for (int i = 0; i < _rowsBlock.ValAsInt * _colsBlock.ValAsInt; i++)
            {
                //if(i == 1)
                {
                    List<string> list = GetSuVert(i);
                    retVal.Add(list);
                }
            }

            return retVal;
        }
        // Get Solving Unit - Vertical list
        List<string> GetSuVert(int colNoSoduku)
        {
            List<string> retVal = [];

            int q = (int)(colNoSoduku / _rowsBlock.ValAsInt);
            int s = (colNoSoduku % _rowsBlock.ValAsInt);

            for (int i = 0; i < _rowsBlock.ValAsInt * _colsBlock.ValAsInt; i++)
            {
                int p = (int)(i / _colsBlock.ValAsInt);
                int r = (int)(i % _colsBlock.ValAsInt);

                string blockId = GetBlockId(p, q);

                retVal.Add(string.Format(_cellIdPrefix, blockId, r, s));
            }

            return retVal;
        }

        // -----------
        // Get Solving Unit - for a block - Full
        List<List<string>> GetSuBlockFull()
        {
            List<List<string>> retVal = [];

            for (int i = 0; i < _rowsBlock.ValAsInt; i++)
                for (int j = 0; j < _colsBlock.ValAsInt; j++)
                {
                    List<string> list = GetSuBlock(i, j);
                    retVal.Add(list);
                }

            return retVal;
        }
        // Get Solving Unit - for a block
        List<string> GetSuBlock(int rowNoBlock, int colNoBlock)
        {
            List<string> retVal = [];

            string blockId = string.Format(_blockIdPrefix, rowNoBlock, colNoBlock);

            for (int i = 0; i < _rowsBlock.ValAsInt; i++)
                for (int j = 0; j < _colsBlock.ValAsInt; j++)
                {
                    string id = string.Format(_cellIdPrefix, blockId, j, i);
                    //Console.WriteLine($"ID = {id}");
                    retVal.Add(id);
                }


            return retVal;
        }
        // -----------

        public static string GetBlockId(int x, int y) => string.Format(_blockIdPrefix, x, y);

        public int RowsBlockMinVal { get => _rowsBlockMinVal; }
        public int RowsBlockMaxVal { get => _rowsBlockMaxVal; }
        public int ColsBlockMinVal { get => _colsBlockMinVal; }
        public int ColsBlockMaxVal { get => _colsBlockMaxVal; }

        public void ReInit(int rowsBlock, int colsBlock)
        {
            _rowsBlock.ValAsInt = rowsBlock;
            _colsBlock.ValAsInt = colsBlock;

            _positions = GetSuHoriFull().Flatten().Select(x => new KeyValuePair<string, SudokuCellInfo>(x,
                new SudokuCellInfo(cellId: x,
                //positionType: SudokuPositionTypeEnum.None,
                maxCellValue: _rowsBlock.ValAsInt * _colsBlock.ValAsInt, 0)
            ))
                .ToDictionary(t => t.Key, t => t.Value);
        }

        public int GetRowsBlock()
        {
            return _rowsBlock.ValAsInt;
        }

        public int GetColsBlock()
        {
            return _colsBlock.ValAsInt;
        }

        // resets all cells hints and all cell value clashings
        void ResetHintsAndCellValueClashing()
        {
            for (int i = 0; i < Math.Sqrt(_positions.Count); i++)
            {
                List<SudokuCellInfo> su = _positions.Skip(i * (int)Math.Sqrt(_positions.Count)).Take((int)Math.Sqrt(_positions.Count)).Select(x => x.Value).ToList();

                foreach (SudokuCellInfo cellInfo in su)
                {
                    cellInfo.ResetHints();
                    cellInfo.CellValueClashing = false;
                }
            }
        }

        public void RenewHints()
        {
            ResetHintsAndCellValueClashing();
            CheckHori();
            CheckVert();
            CheckBlock();
        }

        private void CheckHori()
        {
            Dictionary<string, SudokuCellInfo> _positions1 = GetSuHoriFull().Flatten().Select(x => new KeyValuePair<string, SudokuCellInfo>(x, _positions[x])).ToDictionary(t => t.Key, t => t.Value);

            for (int i = 0; i < Math.Sqrt(_positions1.Count); i++)
            {
                List<string> su = _positions1.Skip(i * (int)Math.Sqrt(_positions1.Count)).Take((int)Math.Sqrt(_positions1.Count)).Select(x => x.Key).ToList();

                CheckInternal(su);
            }
        }

        private void CheckVert()
        {
            Dictionary<string, SudokuCellInfo> _positions1 = GetSuVertFull().Flatten().Select(x => new KeyValuePair<string, SudokuCellInfo>(x, _positions[x])).ToDictionary(t => t.Key, t => t.Value);

            for (int i = 0; i < Math.Sqrt(_positions1.Count); i++)
            {
                List<string> su = _positions1.Skip(i * (int)Math.Sqrt(_positions1.Count)).Take((int)Math.Sqrt(_positions1.Count)).Select(x => x.Key).ToList();

                CheckInternal(su);
            }
        }

        private void CheckBlock()
        {
            Dictionary<string, SudokuCellInfo> _positions1 = GetSuBlockFull().Flatten().Select(x => new KeyValuePair<string, SudokuCellInfo>(x, _positions[x])).ToDictionary(t => t.Key, t => t.Value);

            for (int i = 0; i < Math.Sqrt(_positions1.Count); i++)
            {
                List<string> su = _positions1.Skip(i * (int)Math.Sqrt(_positions1.Count)).Take((int)Math.Sqrt(_positions1.Count)).Select(x => x.Key).ToList();

                CheckInternal(su);
            }
        }

        private void CheckInternal(List<string> su)
        {
            //Console.WriteLine($"Solving for unit \r\n {JsonSerializer.Serialize(su ?? [])}");

            if(su is not null)
                for (int j = 0; j < su.Count; j++)
                {
                    string our = su[j];
                    List<string> others = su.Where(x => x != su[j]).ToList(); // to do add value check

                    if (_positions[our].CellValue == 0)
                    {
                        foreach (string other in others)
                        {
                            if (_positions[other].CellValue > 0)
                            {
                                Console.WriteLine($"From position {our} removing hint {_positions[other].CellValue}");
                                _positions[our].DisableHint(_positions[other].CellValue);
                            }
                        }

                        _positions[our].CellValueClashing = false;
                    }
                    else // _positions[our].CellValue > 0
                    {
                        foreach (string other in others)
                        {
                            if (_positions[other].CellValue > 0 && _positions[our].CellValue == _positions[other].CellValue)
                            {
                                Console.WriteLine($"Our position {our} is clashing other position {other}");
                                _positions[our].CellValueClashing = true;
                            }
                        }

                        //Console.WriteLine($"For position {our} removing all hints");
                        _positions[our].ResetHints();
                    }

                }
        }
    }



    internal class Integer1 : ICloneable
    {
        readonly int _minValue = int.MinValue;
        readonly int _maxValue = int.MaxValue;

        int _value = int.MinValue;

        public Integer1(int value, int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            SetValue(value);
        }

        public int GetMaxValue() => _maxValue;

        private int GetValue() => _value;
        private int SetValue(int value)
        {
            Debug.Assert(_minValue <= _maxValue);

            if (value < _minValue)
                _value = _minValue;
            else if (value > _maxValue)
                _value = _maxValue;
            else
                _value = value;

            return _value;
        }

        public object Clone()
        {
            Integer1 cloned = new(this._value, this._minValue, this._maxValue);
            return cloned;
        }

        public int ValAsInt
        {
            get => GetValue();
            set => SetValue(value);
        }
    }

    internal class HintInfo(string hintId, int hintNo, bool hintEnabled) : ICloneable
    {
        public string HintId { get; private set; } = hintId;
        public int HintNo { get; private set; } = hintNo;
        public bool HintEnabled { get; private set; } = hintEnabled;

        public object Clone()
        {
            HintInfo cloned = new(this.HintId, this.HintNo, this.HintEnabled);

            return cloned;
        }

        public void DisableHint() => HintEnabled = false;
    }

    internal class SudokuCellInfo : ICloneable
    {
        readonly string _cellId = "";
        //SudokuPositionTypeEnum _positionType;
        readonly Integer1 _cellValueField1;
        List<HintInfo> _hints = [];

        public bool CellValueClashing { get; set; } = false;

        ///// <summary>
        ///// // solvable by means of single hint enabled
        ///// </summary>
        //public bool SolvableByLoneHint
        //{
        //    get
        //    {
        //        return _hints.FindAll(x => x.HintEnabled).Count == 1;
        //    }
        //}

        public SudokuCellInfo(string cellId,
            //SudokuPositionTypeEnum positionType,
            int maxCellValue, int cellValue)
        {
            _cellId = cellId;
            //_positionType = positionType;
            _cellValueField1 = new(cellValue, 0, maxCellValue)
            {
                ValAsInt = cellValue
            };

            _hints = Enumerable.Range(1, maxCellValue).Select( x => new HintInfo(hintId: _cellId + $":H{x}", hintNo: x, hintEnabled: true) ).ToList();
        }

        public void DisableHint(int hintToDisable)
        {
            //_hints.RemoveAll( x => x.HintNo == hintToRemove );
            _hints.ForEach( (item) => { if (item.HintNo == hintToDisable) item.DisableHint(); } );
        }

        public void ResetHints()
        {
            if (CellValue > 0)
                _hints = Enumerable.Range(1, _cellValueField1.GetMaxValue()).Select(x => new HintInfo(hintId: _cellId + $":H{x}", hintNo: x, hintEnabled: false) ).ToList();
            else
                _hints = Enumerable.Range(1, _cellValueField1.GetMaxValue()).Select(x => new HintInfo(hintId: _cellId + $":H{x}", hintNo: x, hintEnabled: true) ).ToList();
        }

        public ReadOnlyCollection<HintInfo> Hints { get
            {
                ReadOnlyCollection<HintInfo> hintReadOnly = _hints.AsReadOnly();
                return hintReadOnly;
            }
        }

        public int CellValue
        {
            get => _cellValueField1.ValAsInt;
            set => _cellValueField1.ValAsInt = value;
        }

        public object Clone()
        {
            SudokuCellInfo cloned = new(this._cellId, this._cellValueField1.GetMaxValue(), this.CellValue)
            {
                _hints = this.Hints.Select(x => (HintInfo)x.Clone()).ToList(),
                CellValueClashing = this.CellValueClashing,
            };

            return cloned;
        }
    }

    internal class Move
    {
        public required string ControlId { get; set; }
        public MoveType MoveType { get; set; }
        public int? InputNewValue { get; set; }
        public bool? HintButtonNewValue { get; set; }// true is enabled and false is disabled
    }

    internal enum MoveType
    {
        InputChanged,
        HintButtonDisabled,
    }

}
