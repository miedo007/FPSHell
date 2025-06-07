using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellishBattle.Console
{
    public class CheatCommandBase
    {
        private string _commandID;
        private string _commandDescription;
        private string _commandFormat;

        public string commandID { get { return _commandID; } }
        public string commandDescription { get { return _commandDescription; } }
        public string commandFormat { get { return _commandFormat; } }

        public CheatCommandBase(string id, string description, string format)
        {
            _commandID = id;
            _commandDescription = description;
            _commandFormat = format;
        }
    }

    public class CheatCommand : CheatCommandBase
    {
        private Action command;

        public CheatCommand(string id, string description, string format, Action command) : base(id, description, format)
        {
            this.command = command;
        }

        public void Invoke()
        {
            command.Invoke();
        }
    }
    public class CheatCommand<T1> : CheatCommandBase
    {
        private Action<T1> command;

        public CheatCommand(string id, string description, string format, Action<T1> command) : base(id, description, format)
        {
            this.command = command;
        }

        public void Invoke(T1 value)
        {
            command.Invoke(value);
        }
    }

}