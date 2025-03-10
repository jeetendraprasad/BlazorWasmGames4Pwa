﻿using BlazorWasmGames4Pwa.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
//using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
//using static BlazorWasmGames4Pwa.Pages.Weather;
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
        Stack<Move> _moves = [];

        Integer1 _rowsBlock, _colsBlock;

        const string _blockIdPrefix = "B[{0},{1}]";                // block names are B[{0-based rows},{0-based cols}]
        const string _cellIdPrefix = "{0}:C[{1},{2}]";             // cell names are BLOCK_ID:[{0-based rows},{0-based cols}]
        const string _hintIdPrefix = "H";

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

            DoOperations();
        }

        public void NewMoveForHint(string hintBtnId)
        {
            _moves.Push(new Move() { ControlId = hintBtnId, MoveType = MoveType.HintButtonDisabled, InputNewValue = null, HintButtonNewValue = false, });

            DoOperations();
        }

        public void UpdatePositionsByMoves()
        {
            foreach (Move move in _moves.ToArray().Reverse())
            {
                Console.WriteLine("Move: " + JsonSerializer.Serialize(move));

                if (move.MoveType == MoveType.InputChanged)
                {
                    _positions[move.ControlId].CellValue = move.InputNewValue ?? 0;
                }
                else if (move.MoveType == MoveType.HintButtonDisabled)
                {
                    string hintBtnId = move.ControlId;

                    string cellId = hintBtnId.Split($":{_hintIdPrefix}")[0];
                    int hintNumber = Convert.ToInt32(hintBtnId.Split($":{_hintIdPrefix}")[1], CultureInfo.InvariantCulture);

                    _positions[cellId].DisableHint(hintNumber);
                }
                else
                {
                    // we should not be here
                    // us gali me na ja, jaha teri gf nahi
                }
            }
        }

        public bool MoveUndo()
        {
            if (_moves.Count <= 0)
                return false;

            _moves.Pop();

            DoOperations();

            return true;
        }

        private void Init()
        {
            _rowsBlock = new(_rowsBlockStartVal, _rowsBlockMinVal, _rowsBlockMaxVal);
            _colsBlock = new(_colsBlockStartVal, _colsBlockMinVal, _colsBlockMaxVal);

            ResetAllPositions();
            ResetAllMoves();
        }

        public void ResetAllMoves() => _moves = [];

        static List<SudokuSuType> GetAllSuTypes(bool onlyBasicType = true /* Hori, Vert, Bloc */)
        {
            List<SudokuSuType> retVal
                = new List<SudokuSuType>() { SudokuSuType.Hori, SudokuSuType.Vert, SudokuSuType.Bloc };

            if (!onlyBasicType)
                retVal.Add(SudokuSuType.Any);

            return retVal;
        }

        List<List<string>> GetSuFullByType(SudokuSuType sudokuSuType)
        {
            List<List<string>> retVal = new List<List<string>>();

            if (sudokuSuType == SudokuSuType.Hori)
            {
                retVal = GetSuHoriFull();
            }
            else if (sudokuSuType == SudokuSuType.Vert)
            {
                retVal = GetSuVertFull();
            }
            else if (sudokuSuType == SudokuSuType.Bloc)
            {
                retVal = GetSuBlocFull();
            }
            else
            {
                throw new NotImplementedException($"This sudoku type {sudokuSuType} is not implemented in this function");
            }

            return retVal;
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
        List<List<string>> GetSuBlocFull()
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

            ResetAllPositions();
            ResetAllMoves();
        }

        // rest positions to all cell valies 0 and all the hints enabled
        public void ResetAllPositions()
        {
            _positions = GetSuHoriFull().Flatten().Select(x => new KeyValuePair<string, SudokuCellInfo>(x,
                new SudokuCellInfo(cellId: x,
                //positionType: SudokuPositionTypeEnum.None,
                maxCellValue: _rowsBlock.ValAsInt * _colsBlock.ValAsInt, 0, _hintIdPrefix)
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
        void ResetHintsAndCellValueClashing(bool resetHints)
        {
            for (int i = 0; i < Math.Sqrt(_positions.Count); i++)
            {
                List<SudokuCellInfo> su = _positions.Skip(i * (int)Math.Sqrt(_positions.Count)).Take((int)Math.Sqrt(_positions.Count)).Select(x => x.Value).ToList();

                foreach (SudokuCellInfo cellInfo in su)
                {
                    if (resetHints)
                        cellInfo.ResetHints();
                    cellInfo.CellValueClashing = false;
                }
            }
        }

        public void RenewHints(bool resetHints)
        {
            ResetHintsAndCellValueClashing(resetHints);
            GetAllSuTypes(onlyBasicType: true).ForEach(su => CheckSu(su));
        }

        private void CheckSu(SudokuSuType sudokuSuType)
        {
            Dictionary<string, SudokuCellInfo> _positions1 = GetSuFullByType(sudokuSuType).Flatten().Select(x => new KeyValuePair<string, SudokuCellInfo>(x, _positions[x])).ToDictionary(t => t.Key, t => t.Value);

            for (int i = 0; i < Math.Sqrt(_positions1.Count); i++)
            {
                List<string> su = _positions1.Skip(i * (int)Math.Sqrt(_positions1.Count)).Take((int)Math.Sqrt(_positions1.Count)).Select(x => x.Key).ToList();

                CheckInternal(su);
            }
        }

        private void CheckInternal(List<string> su)
        {
            //Console.WriteLine($"Solving for unit \r\n {JsonSerializer.Serialize(su ?? [])}");

            if (su is not null)
                for (int i = 0; i < su.Count; i++)
                {
                    string our = su[i];
                    List<string> others = su.Where((x, index) => /*index > i &&*/ x != su[i]).ToList(); // to do add value check

                    if (_positions[our].CellValue == 0)
                    {
                        foreach (string other in others)
                        {
                            if (_positions[other].CellValue > 0)
                            {
                                //Console.WriteLine($"From position {our} removing hint {_positions[other].CellValue}");
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
                                //Console.WriteLine($"Our position {our} is clashing other position {other}");
                                _positions[our].CellValueClashing = true;
                            }
                        }

                        //Console.WriteLine($"For position {our} removing all hints");
                        _positions[our].ResetHints();
                    }

                }
        }


        internal delegate List<SudokuTip> Func_FindNextTip(List<string> su, bool onlyFirst = true);

        internal List<SudokuTip> FindNextTip_BySu(List<SudokuSuType> sudokuSuTypes, Func_FindNextTip dele, bool onlyFirst = true)
        {
            List<SudokuTip> retVal = [];

            foreach (SudokuSuType sudokuSuType in sudokuSuTypes)
            {
                Dictionary<string, SudokuCellInfo> _positions3 = GetSuFullByType(sudokuSuType).Flatten().Select(x => new KeyValuePair<string, SudokuCellInfo>(x, _positions[x])).ToDictionary(t => t.Key, t => t.Value);

                for (int i = 0; i < Math.Sqrt(_positions3.Count); i++)
                {
                    List<string> su = _positions3.Skip(i * (int)Math.Sqrt(_positions3.Count)).Take((int)Math.Sqrt(_positions3.Count)).Select(x => x.Key).ToList();

                    List<SudokuTip> retVal1 = dele(su, onlyFirst: onlyFirst);

                    retVal.AddRange(retVal1);

                    if (onlyFirst && retVal.Count > 0)
                        return [retVal[0]];

                }
            }

            return retVal;
        }

        // fn to get all triplets1 (1,2,4 + 1,2 + 1,2,4) OR (1,2 + 2,3 + 1,2,3)
        internal List<SudokuTip> FindNextTip_HintTriplets1FirstOrAll(List<string> su, bool onlyFirst = true)
        {
            List<SudokuTip> retVal = [];

            List<List<string>> triplets
                =
                (from one in su
                 from two in su
                 from thr in su
                 where string.Compare(one, two, false) > 0 && string.Compare(two, thr, false) > 0
                 select new List<string>() { one, two, thr })
                .ToList();

            foreach (List<string> triple in triplets)
            {
                List<int> oneList = _positions[triple[0]].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();
                List<int> twoList = _positions[triple[1]].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();
                List<int> thrList = _positions[triple[2]].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();

                if (
                    (oneList.Count < 2 || oneList.Count > 3)
                    ||
                    (twoList.Count < 2 || twoList.Count > 3)
                    ||
                    (thrList.Count < 2 || thrList.Count > 3)
                    )
                    continue;

                List<int> combined = (
                        oneList
                    .Union(
                        twoList
                        )
                    .Union(
                        thrList
                        )
                    ).ToList();

                if (combined.Count() != 3)
                {
                    // They are not part of triplets1
                    continue;
                }

                SudokuTip tip = new()
                {
                    Su = su,
                    SudokuTipCell = triple,
                    SudokuTipType = SudokuTipType.Triplet1,
                };

                retVal.Add(tip);

                if (onlyFirst)
                    break;
            }

            return retVal;
        }

        internal List<SudokuTip> FindNextTip_HintDoublesFirstOrAll(List<string> su, bool onlyFirst = true)
        {
            List<SudokuTip> retVal = [];

            if (su is not null)
            {
                for (int i = 0; i < su.Count; i++)
                {
                    string our = su[i];
                    List<string> others = su.Where((x, index) => /*index > i &&*/ x != su[i]).ToList(); // to do add value check

                    List<int> ourHints = _positions[our].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();

                    if (ourHints.Count == 2)
                    {
                        foreach (string other in others)
                        {
                            List<int> otherHints = _positions[other].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();

                            if (otherHints.Count == 2 && ourHints.SequenceEqual(otherHints)) // NOTE: We assume that sequence will also be same for ourHints and otherHints
                            {
                                SudokuTip tip = new()
                                {
                                    SudokuTipType = SudokuTipType.NakedPair,
                                    SudokuTipCell = [our, other],
                                    Su = su,
                                };

                                retVal.Add(tip);

                                if (onlyFirst)
                                    return retVal;
                            }
                        }
                    }
                }
            }

            return retVal;
        }


        internal List<SudokuTip> FindNextTip_HiddenSinglesFirstOrAll(List<string> su, bool onlyFirst = true)
        {
            List<SudokuTip> retVal = [];

            if (su is not null)
            {
                for (int i = 0; i < su.Count; i++)
                {
                    string our = su[i];
                    List<string> others = su.Where((x, index) => /*index > i &&*/ x != su[i]).ToList(); // to do add value check

                    List<int> ourHints = _positions[our].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();

                    foreach (string other in others)
                    {
                        List<int> otherHints = _positions[other].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();

                        ourHints = ourHints.Except(otherHints).ToList();
                    }

                    if (ourHints.Count == 1) // NOTE: We assume that sequence will also be same for ourHints and otherHints
                    {
                        SudokuTip tip = new()
                        {
                            SudokuTipType = SudokuTipType.HiddenSingles,
                            SudokuTipCell = [our],
                            Su = su,
                        };

                        retVal.Add(tip);

                        if (onlyFirst)
                            return retVal;
                    }
                }
            }

            return retVal;
        }

        internal List<SudokuTip> FindNextTip_SolvableByLoneHintFirstOrAll(bool onlyFirst = true)
        {
            List<SudokuTip> retVal = [];

            foreach (KeyValuePair<string, SudokuCellInfo> position in _positions)
            {
                bool singleHint = position.Value.Hints.Where(x => x.HintEnabled).Count() == 1;
                if (singleHint)
                {
                    SudokuTip tip = new()
                    {
                        SudokuTipCell = [position.Key],
                        SudokuTipType = SudokuTipType.SolvableByLoneHint,
                        Su = [],
                    };

                    retVal.Add(tip);

                    if (onlyFirst)
                        return retVal;
                }
            }

            return retVal;
        }

        private SudokuSaveData GetSaveData()
        {
            List<Move> newMoves = _moves.ToList();
            newMoves.Reverse();
            return new SudokuSaveData()
            {
                RowsBlock = _rowsBlock.ValAsInt,
                ColsBlock = _colsBlock.ValAsInt,
                Moves = newMoves,
            };
        }

        public string GetSaveDataAsJson()
        {
            SudokuSaveData data = GetSaveData();
            string json = JsonSerializer.Serialize(data);
            return json;
        }

        public bool LoadSaveDataAsJson(string json)
        {
            //JsonSerializerOptions options = new()
            //{
            //    UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip,
            //};
            SudokuSaveData? data = JsonSerializer.Deserialize<SudokuSaveData>(json);

            int oldRows = _rowsBlock.ValAsInt;
            int oldCols = _colsBlock.ValAsInt;

            bool retVal;
            if (data is not null)
            {
                _rowsBlock.ValAsInt = data.RowsBlock;
                _colsBlock.ValAsInt = data.ColsBlock;

                if (_rowsBlock.ValAsInt != data.RowsBlock || _colsBlock.ValAsInt != data.ColsBlock)
                {
                    // the rows and cols are out of range

                    _rowsBlock.ValAsInt = oldRows;                           // Resetting to old values
                    _colsBlock.ValAsInt = oldCols;                           // Resetting to old values

                    retVal = false;
                }
                else
                {
                    _moves = new Stack<Move>(data.Moves);

                    DoOperations();

                    retVal = true;
                }
            }
            else
                retVal = false;

            return retVal;
        }

        internal void DoOperations()
        {
            ResetAllPositions();

            UpdatePositionsByMoves();

            RenewHints(false);

            //ResetAllHighlights();

            GetAndHighlightAllHints();
        }

        public bool NoMoreMoves() => !_positions.Any(x => x.Value.CellValue == 0);

        internal void GetAndHighlightAllHints()
        {
            bool someWereHighlighted = false;
            if (!someWereHighlighted)
            {
                List<SudokuTip> tips = FindNextTip_SolvableByLoneHintFirstOrAll(onlyFirst: false);
                HighlightAllHintsByTips(tips, SudokuTipType.SolvableByLoneHint, out someWereHighlighted);
            }
            if (!someWereHighlighted)
            {
                List<SudokuTip> tips = FindNextTip_BySu(GetAllSuTypes(onlyBasicType: true), FindNextTip_HiddenSinglesFirstOrAll,
                    onlyFirst: false);
                HighlightAllHintsByTips(tips, SudokuTipType.HiddenSingles, out someWereHighlighted);
            }
            if (!someWereHighlighted)
            {
                List<SudokuTip> tips = FindNextTip_BySu(GetAllSuTypes(onlyBasicType: true), FindNextTip_HintDoublesFirstOrAll,
                    onlyFirst: false);
                HighlightAllHintsByTips(tips, SudokuTipType.NakedPair, out someWereHighlighted);
            }
            if (!someWereHighlighted)
            {
                //List<SudokuTip> tips = FindNextTip_HintTriplets1FirstOrAll(onlyFirst: false);
                List<SudokuTip> tips = FindNextTip_BySu(GetAllSuTypes(onlyBasicType: true), FindNextTip_HintTriplets1FirstOrAll,
                    onlyFirst: false);
                HighlightAllHintsByTips(tips, SudokuTipType.Triplet1, out someWereHighlighted);
            }

        }

        internal void HighlightAllHintsByTips(List<SudokuTip> tips, SudokuTipType tipType, out bool someWereHighlighted)
        {
            someWereHighlighted = false;

            if (tipType == SudokuTipType.SolvableByLoneHint)
            {
                foreach (SudokuTip tip in tips)
                {
                    if (tip.SudokuTipType == tipType)
                    {
                        SudokuCellInfo cellInfo = _positions[tip.SudokuTipCell[0]];

                        cellInfo.EnableLoneHintHighlight();
                        someWereHighlighted = true;
                    }
                }
            }
            else if (tipType == SudokuTipType.NakedPair)
            {
                foreach (SudokuTip tip in tips)
                {
                    if (tip.SudokuTipType == tipType)
                    {
                        List<int> doubleHints = _positions[tip.SudokuTipCell[0]].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();

                        foreach (var cell in tip.Su)
                        {
                            //Console.WriteLine("Cell = " + cell);

                            if (tip.SudokuTipCell[0] == cell || tip.SudokuTipCell[1] == cell)
                                continue; // we skip own cell of tips

                            SudokuCellInfo cellInfo = _positions[cell];
                            cellInfo.HighlightTheseEnabledHints(doubleHints, false, out bool highlighted);
                            if (highlighted)
                            {
                                someWereHighlighted = true;
                                //break;
                            }
                        }
                    }
                }
            }
            else if (tipType == SudokuTipType.HiddenSingles)
            {
                foreach (SudokuTip tip in tips)
                {
                    if (tip.SudokuTipType == tipType)
                    {
                        List<int> ourHints = _positions[tip.SudokuTipCell[0]].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();

                        foreach (var otherCell in tip.Su)
                        {
                            //Console.WriteLine("Cell = " + cell);

                            if (tip.SudokuTipCell[0] == otherCell)
                                continue; // we skip own cell of tips

                            List<int> otherHints = _positions[otherCell].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();

                            ourHints = ourHints.Except(otherHints).ToList();
                        }

                        if (ourHints.Count == 1)
                        {
                            SudokuCellInfo cellInfo = _positions[tip.SudokuTipCell[0]];
                            cellInfo.HighlightTheseEnabledHints(ourHints, true, out bool highlighted);
                            if (highlighted)
                            {
                                someWereHighlighted = true;
                                //break;
                            }
                        }
                    }
                }
            }
            else if (tipType == SudokuTipType.Triplet1)
            {
                foreach (SudokuTip tip in tips)
                {
                    if (tip.SudokuTipType == tipType)
                    {
                        List<int> oneList = _positions[tip.SudokuTipCell[0]].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();
                        List<int> twoList = _positions[tip.SudokuTipCell[1]].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();
                        List<int> thrList = _positions[tip.SudokuTipCell[2]].Hints.Where(x => x.HintEnabled).Select(x => x.HintNo).ToList();

                        //if (
                        //    (oneList.Count < 2 || oneList.Count > 3)
                        //    ||
                        //    (twoList.Count < 2 || twoList.Count > 3)
                        //    ||
                        //    (thrList.Count < 2 || thrList.Count > 3)
                        //    )
                        //    continue;

                        List<int> combined = (
                                oneList
                            .Union(
                                twoList
                                )
                            .Union(
                                thrList
                                )
                            ).ToList();

                        foreach (var cell in tip.Su)
                        {
                            Console.WriteLine("Cell = " + cell);

                            if (tip.SudokuTipCell[0] == cell || tip.SudokuTipCell[1] == cell || tip.SudokuTipCell[2] == cell)
                                continue; // we skip own cell of tips

                            SudokuCellInfo cellInfo = _positions[cell];
                            cellInfo.HighlightTheseEnabledHints(combined, false, out bool highlighted);
                            if (highlighted)
                            {
                                someWereHighlighted = true;
                                //break;
                            }
                        }
                    }
                }
            }
        }
    }

    internal class SudokuSaveData
    {
        public int RowsBlock { get; set; }
        public int ColsBlock { get; set; }
        public List<Move> Moves { get; set; } = [];
    }

    internal class SudokuTip
    {
        public SudokuTipType SudokuTipType { get; set; }

        public List<string> SudokuTipCell { get; set; } = [];
        public List<string> Su { get; set; } = [];
    }

    internal enum SudokuSuType
    {
        Any,
        Hori,
        Vert,
        Bloc,
    }

    internal enum SudokuTipType
    {
        SolvableByLoneHint,                // solvable because there is only single hint
        HiddenSingles,                     // 
        NakedPair,                         // Doubles pair. E.g. 2,3 and 2,3 in two cells,
        //HiddenPair,
        Triplet1,                          // Triplets type 1 (1,2,4 + 1,2 + 1,2,4) OR (1,2 + 2,3 + 1,2,3)
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

    internal class HintInfo(string hintId, int hintNo, bool hintEnabled, bool highlighted, bool highlightedIsSolution) : ICloneable
    {
        public string HintId { get; private set; } = hintId;
        public int HintNo { get; private set; } = hintNo;
        public bool HintEnabled { get; private set; } = hintEnabled;
        public bool Highlighted { get; private set; } = highlighted;
        public bool HighlightedIsSolution { get; private set; } = highlightedIsSolution;


        public object Clone()
        {
            HintInfo cloned = new(this.HintId, this.HintNo, this.HintEnabled, this.Highlighted, this.HighlightedIsSolution);

            return cloned;
        }

        public void DisableHint() => HintEnabled = false;

        public void SetHighlightedIsSolution() => HighlightedIsSolution = true;

        //public void DisableHighlight() => Highlighted = false;
        public void EnableHighlight() => Highlighted = true;
    }

    internal class SudokuCellInfo : ICloneable
    {
        readonly string _cellId = "";
        //SudokuPositionTypeEnum _positionType;
        readonly Integer1 _cellValueField1;
        List<HintInfo> _hints = [];
        readonly string _hintIdPrefix = "";

        public bool CellValueClashing { get; set; } = false;

        //public void ResetAllHighlights() => _hints.ForEach(item => item.DisableHighlight());

        public void HighlightTheseEnabledHints(List<int> nums, bool highlightedIsSolution, out bool someWereHighlighted)
        {
            someWereHighlighted = false;

            List<HintInfo> hints = _hints.Where(x => x.HintEnabled && nums.Contains(x.HintNo)).Select(y => y).ToList(); // enabled hints only and that are part of numbers

            hints.ForEach(
                (x) =>
                {
                    x.EnableHighlight();
                    if (highlightedIsSolution)
                        x.SetHighlightedIsSolution();
                }
                );

            if (hints.Count > 0)
                someWereHighlighted = true;
        }

        public void EnableLoneHintHighlight()
        {
            List<HintInfo> hints = _hints.Where(x => x.HintEnabled).Select(y => y).ToList();

            Debug.Assert(hints.Count == 1); // we should have only single hint enabled

            hints.ForEach((x) => { x.EnableHighlight(); x.SetHighlightedIsSolution(); });
        }

        public SudokuCellInfo(string cellId,
            //SudokuPositionTypeEnum positionType,
            int maxCellValue, int cellValue, string hintIdPrefix)
        {
            _cellId = cellId;
            //_positionType = positionType;
            _cellValueField1 = new(cellValue, 0, maxCellValue)
            {
                ValAsInt = cellValue
            };

            _hintIdPrefix = hintIdPrefix;

            _hints = Enumerable.Range(1, maxCellValue).Select(x => new HintInfo(hintId: _cellId + $":{hintIdPrefix}{x}", hintNo: x, hintEnabled: true, highlighted: false, highlightedIsSolution: false)).ToList();
        }

        public void DisableHint(int hintToDisable)
        {
            _hints.ForEach((item) => { if (item.HintNo == hintToDisable) item.DisableHint(); });
        }

        public void ResetHints()
        {
            if (CellValue > 0)
                _hints = Enumerable.Range(1, _cellValueField1.GetMaxValue()).Select(x => new HintInfo(hintId: _cellId + $":{_hintIdPrefix}{x}", hintNo: x, hintEnabled: false, highlighted: false, highlightedIsSolution: false)).ToList();
            else
                _hints = Enumerable.Range(1, _cellValueField1.GetMaxValue()).Select(x => new HintInfo(hintId: _cellId + $":{_hintIdPrefix}{x}", hintNo: x, hintEnabled: true, highlighted: false, highlightedIsSolution: false)).ToList();
        }

        public ReadOnlyCollection<HintInfo> Hints
        {
            get
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
            SudokuCellInfo cloned = new(this._cellId, this._cellValueField1.GetMaxValue(), this.CellValue, this._hintIdPrefix)
            {
                _hints = this.Hints.Select(x => (HintInfo)x.Clone()).ToList(),
                CellValueClashing = this.CellValueClashing,
            };

            return cloned;
        }
    }

    internal class Move
    {
        public required string ControlId { get; set; } // this is either for input control id or hint button id
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
