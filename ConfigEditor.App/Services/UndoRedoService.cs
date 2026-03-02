using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ConfigEditor.App.Services
{
    // Generic undo/redo system using the command pattern
    // Each action is recorded as an undoable operation with  the ability to reverse and apply it aggain
    public class UndoRedoService
    {
        private readonly Stack<UndoableAction> _undoStack = new();
        private readonly Stack<UndoableAction> _redoStack = new();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;

        public string? LastUndoDescription => _undoStack.Count > 0 ? _undoStack.Peek().Description : null;
        public string? LastRedoDescription => _redoStack.Count > 0 ? _redoStack.Peek().Description : null;

        public event Action? StateChanged;

        // execute an action and record it
        public void Execute(UndoableAction action)
        {
            action.Execute();
            _undoStack.Push(action);
            _redoStack.Clear(); // new action invalidates redo history
            StateChanged?.Invoke();
        }

        // undo last action
        public void Undo()
        {
            if (!CanUndo) return;
            var action = _undoStack.Pop();
            action.Undo();
            _redoStack.Push(action);
            StateChanged?.Invoke();
        }
    
        // in order to redo the last undone action
        public void Redo()
        {
            if (!CanRedo) return;
            var action = _redoStack.Pop();
            action.Execute();
            _undoStack.Push(action);
            StateChanged?.Invoke();
        }

        //clear the history
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            StateChanged?.Invoke();
        }
    }
    
    // to represent a single undoable action
    public class UndoableAction
    {
        public string Description { get; }
        private readonly Action _execute;
        private readonly Action _undo;

        public UndoableAction(string description, Action execute, Action undo)
        {
            Description = description;
            _execute = execute;
            _undo = undo;
        }

        public void Execute() => _execute();
        public void Undo() => _undo();
    }
}

